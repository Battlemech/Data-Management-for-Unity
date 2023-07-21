using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime
{
    public class MainThreadRunner : MonoBehaviour
    {
        private static readonly ConcurrentQueue<Action> MainThreadActions = new ConcurrentQueue<Action>();

        private static MainThreadRunner _instance;
        
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
            while (MainThreadActions.TryDequeue(out Action action))
            {
                action.Invoke();
            }
        }

        void OnDestroy()
        {
            //remove instance reference
            if (_instance == this) _instance = null;
        }

        public static void Delegate(Action action)
        {
            if(_instance == null)
                Debug.LogWarning("No MainThreadRunner component was setup. Make sure one is added to the scene, or delegated actions won't be executed!");
            
            MainThreadActions.Enqueue(action);
        }
    }
}