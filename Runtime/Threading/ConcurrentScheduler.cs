using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Threading
{
    public class ConcurrentScheduler : TaskScheduler
    {
        /// <summary>
        /// Amount of idle threads which won't be terminated automatically
        /// </summary>
        private const int MaxIdleThreads = 3;

        /// <summary>
        /// Amount of time an idle helper thread will wait for new tasks before terminating
        /// </summary>
        private const int IdleTimeBeforeTermination = 1000;

        /// <summary>
        /// Amount of idle helper threads to terminate at the same time
        /// </summary>
        private const int TerminationCount = 2;

        //tracks tasks to be executed
        private readonly ConcurrentQueue<Task> _toExecuteTasks = new ConcurrentQueue<Task>();
        
        //event signaling a single thread to start dequeuing tasks
        private readonly AutoResetEvent _taskAddedEvent = new AutoResetEvent(false);
        
        //track idle thread count
        public int IdleThreadCount => _idleThreadCount;
        private int _idleThreadCount = 0;
        
        //track active threads
        private int _threadCount = 0;

        protected override void QueueTask(Task task)
        {
            _toExecuteTasks.Enqueue(task);

            //start thread which will execute tasks
            if (_idleThreadCount == 0)
            {
                //background threads terminate automatically, no need to track them
                Thread thread = new Thread(StartThread) { IsBackground = true };
                thread.Start();
            }
            else
            {
               //signal a waiting thread that new work was added
                _taskAddedEvent.Set();
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return !taskWasPreviouslyQueued && TryExecuteTask(task);
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _toExecuteTasks;
        }

        private void StartThread() => ExecuteTasks(Interlocked.Increment(ref _threadCount));
        
        private void ExecuteTasks(int threadIndex)
        {
            while (true)
            {
                while (_toExecuteTasks.TryDequeue(out Task task))
                {
                    TryExecuteTask(task);
                    
                    //log any exceptions which occur
                    if(task.IsFaulted)
                        Debug.LogException(task.Exception);
                }

                //signal that thread is now idle
                Interlocked.Increment(ref _idleThreadCount);
                
                //if thread was created to deal with onslaught of new tasks
                if (threadIndex >= MaxIdleThreads)
                {
                    /*
                     * Terminate newest created thread if no new work for thread is received within a short timespan
                     * to iteratively terminate idle threads
                     */
                    
                    
                    //wait for more work within a short timeframe, adding additional wait time proportional to queue position
                    while (!_taskAddedEvent.WaitOne(CalculateWaitTime(threadIndex)))
                    {
                        //if current thread isn't the newest created one: Try to wait again, checking queue position again
                        if (threadIndex + TerminationCount < _threadCount) continue;

                        //thread will be terminating and is no longer idle
                        Interlocked.Decrement(ref _idleThreadCount);
                        
                        //thread no longer exists
                        Interlocked.Decrement(ref _threadCount);

                        //terminate thread
                        return;
                    }
                }
                else
                {
                    //wait for more work
                    _taskAddedEvent.WaitOne();
                    Interlocked.Decrement(ref _idleThreadCount);   
                }
            }
        }

        private int CalculateWaitTime(int threadIndex)
        {
            int queuePosition = Math.Max(0, _threadCount - threadIndex - TerminationCount);
            int waitTime = (1 + queuePosition/TerminationCount) * (IdleTimeBeforeTermination + 10);

            return waitTime;
        }
    }
}