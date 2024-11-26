using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Threading
{
    public class MainThreadRunner : MonoBehaviour
    {
        private static MainThreadRunner _instance;

        public static readonly ManualScheduler UnityThreadScheduler = new ManualScheduler();
        
        void Awake()
        {
            if (_instance == null)
            {
                //assign instance
                _instance = this;
                
                //make sure the main thread runner isn't destroyed
                DontDestroyOnLoad(this);
                return;
            }
            
            Debug.LogWarning("Only one mainThreadRunner is required!");
        }

        void Update()
        {
            //process all delegated actions
            UnityThreadScheduler.ExecuteScheduledTasks();
        }

        void OnDestroy()
        {
            //remove instance reference
            if (_instance == this) _instance = null;
        }

        public static T Delegate<T>(T task) where T : Task 
        {
            if(_instance == null)
                Debug.LogWarning("No MainThreadRunner component was setup. Make sure one is added to the scene, or delegated actions won't be executed!");
            
            //let Unity's main thread get the task
            task.Start(UnityThreadScheduler);

            return task;
        }
        
        public static Task Delegate(Func<Task> func)
        {
            if (_instance == null)
                Debug.LogWarning("No MainThreadRunner component was setup. Make sure one is added to the scene, or delegated actions won't be executed!");

            var task = new Task<Task>(func);
            task.Start(UnityThreadScheduler);

            return task.Unwrap();
        }
        
        public static Task Delegate(Action action) => Delegate(new Task(action));
    }
    
    public static class MainThreadRunnerUtility
    {
        public static Task ContinueOnMainThread<T>(this Task<T> task, Action<T> action)
        {
            return task.ContinueWith((t =>
            {
                return MainThreadRunner.Delegate(() => action(t.Result));
            })).Unwrap();
        }
        
        public static Task ContinueOnMainThread<T>(this Task<T> task, Func<T, Task> func)
        {
            return task.ContinueWith(async t =>
            {
                try
                {
                    await MainThreadRunner.Delegate(() => func(t.Result));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
            }).Unwrap();
        }
    }
}