using System.Threading.Tasks;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Threading
{
    public static class ThreadingUtility
    {
        //all async function: call onCompleted and check for error
        public static void EnsureSuccess(this Task task)
        {
            task.ContinueWith((t =>
            {
                if (!t.IsFaulted) return;

                Debug.LogException(t.Exception);
            }));
        }
    }
}