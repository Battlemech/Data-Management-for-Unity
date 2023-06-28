using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Data_Management_for_Unity.Runtime;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    public static class TestUtility
    {
        public delegate T GetValue<T>();
        
        public static IEnumerator AreEqual<T>(T expected, GetValue<T> get, string name="Test", int timeout = Options.DefaultTimeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds <= timeout)
            {
                //test succeeded
                if (expected.Equals(get.Invoke()))
                {
                    Debug.Log(name + $" succeeded after {stopwatch.ElapsedMilliseconds} ms");
                    yield break;
                }
                
                //wait for states to change
                yield return null;
            }
            
            Assert.AreEqual(expected, get.Invoke(), name);
        }
    }
}