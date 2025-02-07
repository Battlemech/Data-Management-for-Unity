using System.Threading.Tasks;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Threading
{
    public static class ThreadingUtility
    {
        //all async function: call onCompleted and check for error
        public static Task LogOnFailure(this Task task)
        {
            return task.ContinueWith((t =>
            {
                if (t.Exception == null) return;

                Debug.LogException(t.Exception);
            }));
        }
    }
}