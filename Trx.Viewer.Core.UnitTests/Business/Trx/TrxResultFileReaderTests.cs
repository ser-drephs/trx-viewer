using FluentAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Trx.Viewer.Core.Domain;
using Trx.Viewer.Core.UnitTests;

namespace Trx.Viewer.Core.Business.Trx.UnitTests
{
    [TestClass]
    public class TrxResultFileReaderTests
    {
        private TrxResultFileReader sut;

        public TrxResultFileReaderTests()
        {
            var mock = new Mock<IStringLocalizer<TrxResultFileReader>>();
            sut = new TrxResultFileReader(new FileSystem(), new NullLogger<TrxResultFileReader>(), mock.Object);
        }

        #region Single Result File Tests

        [TestMethod]
        public void Read_Flat_Results_From_File_Should_Read_Elements_With_Error_Info()
        {
            var testData = Path.Join(TestHelper.GetTestDataDirectory(), "FlatResult.trx");
            Read_Flat_Result(testData);
        }

        [TestMethod]
        public void Read_Flat_Results_Wihtout_Namespace_From_File_Should_Read_Elements_With_Error_Info()
        {
            var testData = Path.Join(TestHelper.GetTestDataDirectory(), "FlatResultNoNamespace.trx");
            Read_Flat_Result(testData);
        }

        private void Read_Flat_Result(string inputUri)
        {
            var testResults = sut.Read(inputUri);
            testResults.Should().NotBeNull()
                .And.NotBeEmpty();
            testResults.First().Key.Should().Be(inputUri);
            testResults.First().Value.Should().NotBeNull()
                .And.HaveCount(2)
                .And.Contain(t => t.Outcome == TestOutcome.NotExecuted)
                .Which.ErrorInfo.Should().NotBeNull("because at leat one test failed")
                .And.Match<TestErrorInfo>(t => t.Message.Contains("Inconclusive") && !string.IsNullOrWhiteSpace(t.StackTrace), "because the test was inconclusive and a stacktrace is always provided");
            var firstResult = testResults.First().Value.First();
            firstResult.
        }

        [TestMethod]
        public void Read_Failed_DataDriven_Results_From_File_Should_Contain_Parent_With_Children_And_Error_Info()
        {
            var testData = Path.Join(TestHelper.GetTestDataDirectory(), "FailedDataDrivenResult.trx");
            var testResults = sut.Read(testData);
            testResults.First().Value.Should().NotBeNull()
                .And.HaveCount(1, "because one parent")
                .And.BeOfType<List<TrxTestResult>>()
                .Which.First()
                .Children.Should().HaveCount(6, "this parent has 6 child results");
            testResults.First().Value.First().Children.Where(t => t.ErrorInfo.HasValue).First().ErrorInfo.Value.Message.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public void Read_DataDriven_With_Wrong_Parent_Results_From_File_Should_Not_Contain_Children()
        {
            var testData = Path.Join(TestHelper.GetTestDataDirectory(), "FailedDataDrivenResultWrongParent.trx");
            var testResults = sut.Read(testData);
            testResults.Should().NotBeNull()
                .And.HaveCount(1, "because parent of inner results was wrong therefore only the parent is returned");
        }

        [TestMethod]
        public void Read_DataDriven_Results_From_File_Parent_Should_Contain_4_Children()
        {
            var testData = Path.Join(TestHelper.GetTestDataDirectory(), "DataDrivenResult.trx");
            var testResults = sut.Read(testData);
            testResults.Should().NotBeNull()
                .And.BeOfType<Dictionary<string, List<TrxTestResult>>>();

            var firstResult = testResults.First().Value;

            firstResult.Should().NotBeNull()
                .And.HaveCount(1, "because one parent")
                .And.BeOfType<List<TrxTestResult>>();
            firstResult.First()
                .ResultType.Should().Be(TestResultType.DataDrivenTest);
            firstResult.First()
                .Children.Should().HaveCount(4, "this parent has 4 child results")
                .And.BeOfType<List<TrxTestResult>>()
                .Which.TrueForAll(d => d.ResultType == TestResultType.DataDrivenDataRow && d.DataRowInfo > 0);
        }

        [TestMethod]
        public void Read_Empty_Result_From_File_Should_Not_Be_Null()
        {
            var testData = Path.Join(TestHelper.GetTestDataDirectory(), "EmptyResult.trx");
            var testResults = sut.Read(testData);
            testResults.Should().NotBeNull().And.BeOfType<Dictionary<string, List<TrxTestResult>>>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Empty_InputUri_Provided_Throws_Exception()
        {
            sut.Read("");
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Not_Existing_File_InputUri_Provided_Throws_Exception()
        {
            sut.Read(@"sample.trx");
        }

        #endregion

        #region Multiple Result Files

        [TestMethod]
        public void Read_Multiple_Files_Should_Return_Results_For_All_Files_In_Flat_List()
        {
            string[] testData = new []{ Path.Join(TestHelper.GetTestDataDirectory(), "DataDrivenResult.trx"),
                                    Path.Join(TestHelper.GetTestDataDirectory(), "FailedDataDrivenResult.trx"), 
                                    Path.Join(TestHelper.GetTestDataDirectory(), "FlatResultNoNamespace.trx"),
                                    Path.Join(TestHelper.GetTestDataDirectory(), "FlatResult.trx")};
            var results = sut.Read(testData);
            results.Should().NotBeNull().And.NotBeEmpty();
            results.Should().BeOfType<Dictionary<string, List<TrxTestResult>>>().And.HaveCount(4);
        }

        [TestMethod]
        public void Empty_InputUris_Provided_Not_Throws_Exception()
        {
            sut.Read("", "");
        }

        [TestMethod]
        public void Empty_InputUris_Array_Provided_Not_Throws_Exception()
        {
            sut.Read(new []{ "", ""});
        }

        [TestMethod]
        public void One_Of_Some_Not_Existing_File_InputUri_Provided_Not_Throws_Exception()
        {
            var testData = Path.Join(TestHelper.GetTestDataDirectory(), "EmptyResult.trx");
            var result = sut.Read(testData, @"sample.trx");
            result.Should().NotBeNull().And.NotBeEmpty();            
        }

        #endregion


    }
}