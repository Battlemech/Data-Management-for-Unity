using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Networking;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using Data_Management_for_Unity.Runtime.Serializer;
using NUnit.Framework;
using UnityEngine;

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
            var message = Message.Create(expected);

            Assert.AreEqual(expected, message.Deserialize(out var type));
            Assert.AreEqual(expected.GetType(), type);
        }

        [Test]
        public void TestNullMessage()
        {
            try
            {
                Message.Create<string>(null);
                Assert.Fail("Didn't catch expected exception");
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
            
            Assert.AreEqual(set.GetType(), Copy(set).GetType());
            Assert.AreEqual(set.DatabaseId, Copy(set).DatabaseId);
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
            throw new NotImplementedException();
        }


        /// <summary>
        /// Given an object, tries serializing and deserializing it, returning a copy
        /// </summary>
        public T Copy<T>(T data)
        {
            return SerializationPCK.Deserialize<T>(SerializationPCK.Serialize(data));
        }
    }
}