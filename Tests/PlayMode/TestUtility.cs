using System;
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
        
        public static void AreEqual<T>(T expected, GetValue<T> get, string name="Test", int timeout = Options.DefaultTimeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds <= timeout)
            {
                //test succeeded
                if (expected.Equals(get.Invoke()))
                {
                    Debug.Log(name + $" succeeded after {stopwatch.ElapsedMilliseconds} ms");
                    return;
                }
                
                Debug.Log($"Expected {expected}, got {get.Invoke()}");
                
                //wait for states to change
                Thread.Sleep(10);
            }
            
            Assert.AreEqual(expected, get.Invoke(), name);
        }
    }
}