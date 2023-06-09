using Data_Management_for_Unity.Runtime.Networking;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using Data_Management_for_Unity.Runtime.Serializer;
using NUnit.Framework;

public class NewTestScript
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestString()
    {
        var test = "My mother told me, one day I would buy, a galley with good oars, sail to distant shores!";

        Assert.AreEqual(test, Copy(test));
        Assert.AreEqual(null, Copy<string>(null));
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
    public void TestNetworkSerializer()
    {
        byte[] test = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        var serializer = new NetworkSerializer();
        Assert.AreEqual(test, serializer.Deserialize(NetworkSerializer.Serialize(test))[0]);
    }

    /// <summary>
    ///     Given an object, tries serializing and deserializing it, returning a copy
    /// </summary>
    public T Copy<T>(T data)
    {
        return Serialization.Deserialize<T>(Serialization.Serialize(data));
    }
}