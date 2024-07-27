using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Scheduler = System.Threading.Tasks.TaskScheduler;

namespace Data_Management_for_Unity.Runtime.Threading
{
    public class QueuedScheduler : Scheduler
    {
        /// <summary>
        /// Number of tasks still scheduled
        /// </summary>
        public int QueuedTasksCount => _queuedTasks.Count + (ExecutingTasks ? 1 : 0);
        
        /// <summary>
        /// True if tasks are currently being processed, otherwise false
        /// </summary>
        public bool ExecutingTasks => _executingThread != null;

        /// <summary>
        /// Queue of tasks to execute
        /// </summary>
        private readonly ConcurrentQueue<Task> _queuedTasks = new ConcurrentQueue<Task>();
        
        /// <summary>
        /// Thread processing tasks
        /// </summary>
        private Thread _executingThread;

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _queuedTasks;
        }
        
        protected override void QueueTask(Task task)
        {
            _queuedTasks.Enqueue(task);

            //start executing tasks if no thread is doing that already
            lock (_queuedTasks)
            {
                if(_executingThread != null) return;
                _executingThread = new Thread(Execute);
            }
            
            //start executing
            _executingThread.Start();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            //executing tasks in line is too inefficient: It is disabled
            return false;
        }

        private void Execute()
        {
            while (true)
            {
                //execute queued tasks
                while (_queuedTasks.TryDequeue(out Task task))
                {
                    TryExecuteTask(task);
                    
                    //continue executing tasks if execution was successful
                    if(task.Exception == null) continue;
                    
                    //log exception
                    Debug.LogException(task.Exception);
                }

                //try to stop executing tasks
                lock (_queuedTasks)
                {
                    //if no tasks are queued: stop executing
                    if (_queuedTasks.IsEmpty)
                    {
                        _executingThread = null;
                        return;
                    }
                }

                //continue executing queued tasks
            }
        }
    }
}