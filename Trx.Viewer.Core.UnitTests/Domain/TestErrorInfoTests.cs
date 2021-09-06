using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Trx.Viewer.Core.Domain.UnitTests
{
    [TestClass]
    public class TestErrorInfoTests
    {
        [TestMethod]
        public void Init_Test()
        {
            var errorinfo = new TestErrorInfo("Assert.Fail failed. Isso",@"   at Tracker.Core.Business.WorkTasks.IntegrationTests.WorkTaskCommandTests.CreateTest() in C:\source\Tracker\Tracker.Core.IntegrationTests\Business\WorkTasks\WorkTaskCommandTests.cs:line 16&#xD;");
            errorinfo.Message.Should().NotBeNullOrWhiteSpace();
            errorinfo.StackTrace.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Init_Test_Null_Message()
        {
            new TestErrorInfo(null, "Stack Trace");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Init_Test_Null_Stacktrace()
        {
            new TestErrorInfo("Message", null);
        }
    }
}