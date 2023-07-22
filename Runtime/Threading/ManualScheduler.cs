using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Threading
{
    public class ManualScheduler : TaskScheduler
    {
        private readonly ConcurrentQueue<Task> _toExecute = new ConcurrentQueue<Task>();

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _toExecute;
        }

        protected override void QueueTask(Task task)
        {
            _toExecute.Enqueue(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            //tasks must be executed on the main thread
            return false;
        }

        public void ExecuteScheduledTasks()
        {
            while (_toExecute.TryDequeue(out Task task))
            {
                TryExecuteTask(task);
                
                //log any exceptions which occur
                if(task.IsFaulted) Debug.LogException(task.Exception);
            }
        }
    }
}