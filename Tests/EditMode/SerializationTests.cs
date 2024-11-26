using System;
using System.Collections.Generic;
using System.Diagnostics;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Networking;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using Data_Management_for_Unity.Runtime.Objects;
using Data_Management_for_Unity.Runtime.Serializer;
using Data_Management_for_Unity.Tests.PlayMode;
using NUnit.Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Data_Management_for_Unity.Tests.EditMode
{
    public class SerializationTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestString()
        {
            var test = "My mother told me, one day I would buy, a galley with good oars, sail to distant shores!";

            Assert.AreEqual(test, Copy(test));
            Assert.AreEqual(null, Copy<string>(null));
            
            Debug.Log(test + " - " + Copy(test));
        }

        [Test]
        public void TestFloat()
        {
            var test = 3123123.231f;

            Assert.AreEqual(test, Copy(test));
            Assert.AreEqual((float)default, Copy<float>(default));
        }

        [Test]
        public void TestMessage()
        {
            //object to pack
            var expected = "123456789,10,11, and so on";

            //message
            var message = new SerializedObject(expected);

            Assert.AreEqual(expected, message.Deserialize(out var type));
            Assert.AreEqual(expected.GetType(), type);
        }

        [Test]
        public void TestNullMessage()
        {
            try
            {
                SerializedObject o = new SerializedObject(null);
                Assert.IsNull(o, "Didn't catch expected exception");
            }
            catch (ArgumentNullException)
            {
                //successfully caught expected exception
            }
        }

        [Test]
        public void TestNetworkSerializer()
        {
            byte[] test = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            var serializer = new NetworkSerializer();
            Assert.AreEqual(test, serializer.Deserialize(NetworkSerializer.Serialize(test))[0]);
        }

        [Test]
        public void TestOperationMessage()
        {
            SynchronisedSet<byte[]> set = new SynchronisedSet<byte[]>("123", "213213", Array.Empty<byte>(), typeof(string), null);
            SynchronisedOperation generic = new SynchronisedSet<byte[]>("1234", "212343213", Array.Empty<byte>(), typeof(int), null);
            
            Assert.AreEqual(set.GetType(), Copy(set).GetType());
            Assert.AreEqual(set.DatabaseId, Copy(set).DatabaseId);
            
            Assert.AreEqual(generic.GetType(), Copy(generic).GetType());
            Assert.AreEqual(generic.DatabaseId, Copy(generic).DatabaseId);
        }

        [Test]
        public void TestOperationRequest()
        {
            OperationRequest request = new OperationRequest(new SynchronisedSet<byte[]>("123", "213213", Array.Empty<byte>(), typeof(string), null));
            
            Assert.AreEqual(request.GetType(), Copy(request).GetType());
            Assert.AreEqual(request.Operation.GetType(), Copy(request).Operation.GetType());
            Assert.AreEqual(request.Id, Copy(request).Id);
        }

        [Test]
        public void TestAbstractClass()
        {
            AbstractClass2 abstractClass2 = new AbstractClass2(false, 3);
            
            Assert.AreEqual(abstractClass2, Copy(abstractClass2));
        }

        [Test]
        public void TestAbstractQueue()
        {
            List<AbstractClass> list = new List<AbstractClass>();
            list.Add(new AbstractClass1( false, "name"));
            list.Add(new AbstractClass2(true, 1));

            List<AbstractClass> copy = Copy(list);
            
            Assert.AreEqual(list.Count, copy.Count);
            for (int i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(list[i], copy[i]);
            }
        }

        [Test]
        public void TestNull()
        {
            Assert.AreEqual(null, Copy<object>(null));
        }

        [Test]
        public void TestSerializedObject()
        {
            foreach (var o in new List<object>(){"123", 123, new List<string>(){"1", "2", "3"}, false, ""})
            {
                Assert.AreEqual(o, CopyAndPack(o, o.GetType()));
            }
        }

        [Test]
        public void TestPerformance()
        {
            int count = 10000;
            int stringLength = 512;
            
            //generate a random string
            string randomString = "";
            for (int i = 0; i < stringLength; i++)
            {
                randomString += (char) (UnityEngine.Random.Range(0, 255));
            }
            Debug.Log("Random string: " + randomString);
            
            //start timer
            Stopwatch stopwatch = new Stopwatch();
            
            for (int i = 0; i < count; i++)
            {
                //start timer after first iteration to allow caching
                if(i == 1) stopwatch.Start();
                
                Assert.AreEqual(randomString, Copy(randomString)); //1 object
                TestNull(); //1 object
                TestAbstractClass(); //1 object 
                TestAbstractQueue(); //2 objects
                TestMessage(); //1 object
                TestOperationMessage(); //2 objects
                TestOperationRequest(); //2 objects
            }
            
            //stop timer
            stopwatch.Stop();
            
            const int objectsPerSerialization = 10;
            
            //calculate amount of objects serialized per millisecond
            float objectsPerMillisecond = (float)(count * objectsPerSerialization) / stopwatch.ElapsedMilliseconds;
            
            Debug.Log($"Serialized {count * objectsPerSerialization} objects in {stopwatch.ElapsedMilliseconds}ms, {objectsPerMillisecond} objects/ms w. assertions");
        }

        private object CopyAndPack(object o, Type t)
        {
            SerializedObject so = new SerializedObject(o, t);
            object copy = so.Deserialize(out Type type);
            Assert.AreEqual(o.GetType(), type);
            return copy;
        }


        /// <summary>
        /// Given an object, tries serializing and deserializing it, returning a copy
        /// </summary>
        public static T Copy<T>(T data)
        {
            return (T) SerializationPCK.Deserialize(SerializationPCK.Serialize(data, out var type), type);
        }
    }
}