using System;
using System.Threading.Tasks;
using DMP.Threading;

namespace Data_Management_for_Unity.Runtime.Threading
{
    public static class Delegation
    {
        public static readonly QueuedScheduler QueuedScheduler = new QueuedScheduler();
        public static readonly ConcurrentScheduler ConcurrentScheduler = new ConcurrentScheduler();

        public static Task DelegateTask(Task task)
        {
            task.Start(ConcurrentScheduler);
            return task;
        }
        
        public static Task DelegateAction(Action action)
        {
            return DelegateTask(new Task(action));
        }

        public static Task EnqueueTask(Task task)
        {
            task.Start(QueuedScheduler);
            return task;
        }

        public static Task EnqueueAction(Action action)
        {
            return EnqueueTask(new Task(action));
        }
    }
}