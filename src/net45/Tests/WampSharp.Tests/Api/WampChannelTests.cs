﻿using System;
using NUnit.Framework;
using WampSharp.Tests.TestHelpers;
using WampSharp.V1;

namespace WampSharp.Tests.Api
{
    public class WampChannelTests
    {
        [Test]
        public void OpenAsyncWillNotBlockOnConnectionLost()
        {
            var wampChannelFactory = new WampChannelFactory<MockRaw>(new MockRawFormatter());
            var mockConnection = new MockConnection<MockRaw>();
            var mockControlledWampConnection = mockConnection.SideAToSideB;
            
            var wampChannel = wampChannelFactory.CreateChannel(mockControlledWampConnection);
            var openAsync = wampChannel.OpenAsync();

            Assert.IsFalse(openAsync.IsCompleted);
            mockConnection.SideBToSideA.Dispose();
            Assert.IsTrue(openAsync.IsCompleted);
        }

        [Test]
        public void OpenAsyncWillThrowAndExpcetionIfAnErrorOccurs()
        {
            var wampChannelFactory = new WampChannelFactory<MockRaw>(new MockRawFormatter());
            var mockConnection = new MockConnection<MockRaw>();

            var wampChannel = wampChannelFactory.CreateChannel(mockConnection.SideAToSideB);
            var openAsyncTask = wampChannel.OpenAsync();

            Exception myException = new Exception("My exception");

            mockConnection.SideBToSideA.SendError(myException);

            Assert.IsNotNull(openAsyncTask.Exception);

            Assert.That(openAsyncTask.Exception.InnerException, Is.EqualTo(myException));
        }
    }
}