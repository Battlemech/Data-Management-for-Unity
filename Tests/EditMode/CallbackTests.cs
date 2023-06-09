using System;
using Data_Management_for_Unity.Runtime.Callbacks;
using NUnit.Framework;

namespace Data_Management_for_Unity.Tests.EditMode
{
    public class CallbackTests
    {
        [Test]
        public void TestAddRemove()
        {
            CallbackHandler<string> callbackHandler = new CallbackHandler<string>();
            
            //no callbacks saved yet
            Assert.AreEqual(callbackHandler.GetCallbackCount(""), 0);
            
            //add callbacks
            Assert.IsTrue(callbackHandler.AddCallback<string>("", (_) => {}));
            Assert.IsTrue(callbackHandler.AddCallback<string>("", (_) => {}));
            Assert.AreEqual(2, callbackHandler.GetCallbackCount(""));
            
            //add callbacks with names
            Assert.IsTrue(callbackHandler.AddCallback<string>("", (_) => {}, "test"));
            Assert.IsTrue(callbackHandler.AddCallback<string>("", (_) => {}, "test"));
            Assert.IsFalse(callbackHandler.AddCallback<string>("", (_) => {}, "test", unique:true), "Unique callback added");
            Assert.AreEqual(2, callbackHandler.GetCallbackCount("", "test"));
            Assert.AreEqual(4, callbackHandler.GetCallbackCount(""));
            
            //remove callbacks
            Assert.AreEqual(2, callbackHandler.RemoveCallbacks("", "test"));
            Assert.AreEqual(2, callbackHandler.GetCallbackCount(""));
            Assert.AreEqual(0, callbackHandler.GetCallbackCount("", "test"));
            Assert.AreEqual(2, callbackHandler.RemoveCallbacks(""));
            Assert.AreEqual(0, callbackHandler.GetCallbackCount(""));
        }

        [Test]
        public void TestInvoke()
        {
            int invoked = 0;
            int toInvoke = 10;
            
            //create callback, setting invoked value
            CallbackHandler<int> callbackHandler = new CallbackHandler<int>();
            Assert.IsTrue(callbackHandler.AddCallback<int>(0, (i) => invoked = i));
            Assert.AreEqual(1, callbackHandler.GetCallbackCount(0));

            //invoke callbacks
            Assert.AreEqual(1, callbackHandler.Invoke(0, toInvoke));
            Assert.AreEqual(toInvoke, invoked);
        }

        [Test]
        public void TestRemoveOnError()
        {
            CallbackHandler<char> callbackHandler = new CallbackHandler<char>();

            //add faulty function
            Assert.IsTrue(callbackHandler.AddCallback<char>('a', c => throw new NotImplementedException(), removeOnError:true));
            Assert.AreEqual(1, callbackHandler.GetCallbackCount('a'));
            
            //invoke it
            Assert.AreEqual(1, callbackHandler.Invoke('a', 'b'));
            
            //make sure its removed
            Assert.AreEqual(0, callbackHandler.GetCallbackCount('a'));
            
            /*
             * Add faulty callback without removeOnError, catching exception
             */
            
            Assert.IsTrue(callbackHandler.AddCallback<char>('a', c => throw new NotImplementedException()));
            Assert.AreEqual(1, callbackHandler.GetCallbackCount('a'));

            try
            {
                callbackHandler.Invoke('a', 'b');
                Assert.Fail("Didn't catch expected exception");
            }
            catch (NotImplementedException)
            {
                //successfully caught expected exception
            }
        }

        [Test]
        public void TestWrongCallbackType()
        {
            CallbackHandler<Type> callbackHandler = new CallbackHandler<Type>();

            //add callback expecting string
            Assert.IsTrue(callbackHandler.AddCallback<string>(typeof(Callback), (_ => { })));
            
            //invoke callback with int
            try
            {
                callbackHandler.Invoke(typeof(Callback), 42);
                Assert.Fail("Didn't catch expected exception");
            }
            catch (ArgumentException)
            {
                //successfully caught expected exception
            }
        }
    }
}