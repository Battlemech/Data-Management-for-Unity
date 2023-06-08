using System;
using System.Collections;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using Data_Management_for_Unity.Runtime.Serializer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NewTestScript
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestString()
    {
        string test = "My mother told me, one day I would buy, a galley with good oars, sail to distant shores!";
        
        Assert.AreEqual(test, Copy(test));
        Assert.AreEqual(null, Copy<string>(null));
    }

    [Test]
    public void TestFloat()
    {
        float test = 3123123.231f;
        
        Assert.AreEqual(test, Copy(test));
        Assert.AreEqual((float)default, Copy<float>(default));
    }

    [Test]
    public void TestMessage()
    {
        //object to pack
        string expected = "123456789,10,11, and so on";
        
        //message
        Message message = Message.Create(expected);

        Assert.AreEqual(expected, message.Deserialize(out Type type));
        Assert.AreEqual(expected.GetType(), type);
    }
    
    /// <summary>
    /// Given an object, tries serializing and deserializing it, returning a copy
    /// </summary>
    public T Copy<T>(T data)
    {
        return Serialization.Deserialize<T>(Serialization.Serialize(data));
    }
}
