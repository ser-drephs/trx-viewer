using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Trx.Viewer.Core.Domain.UnitTests
{
    [TestClass]
    public class TrxTestResultTests
    {
        [TestMethod]
        public void HasChidren_NotInitialized_Test()
        {
            var resultEntry = new TrxTestResult();
            resultEntry.HasChildren().Should().BeFalse();
        }

        [TestMethod]
        public void HasChidren_Initialized_NoElements_Test()
        {
            var resultEntry = new TrxTestResult();
            resultEntry.Children = new List<TrxTestResult>();
            resultEntry.HasChildren().Should().BeFalse();
        }

        [TestMethod]
        public void HasChidren_Initialized_WithElements_Test()
        {
            var resultEntry = new TrxTestResult();
            resultEntry.Children = new List<TrxTestResult>();
            var childResultEntry = new TrxTestResult();
            resultEntry.Children.Add(childResultEntry);
            resultEntry.HasChildren().Should().BeTrue();
        }
    }
}