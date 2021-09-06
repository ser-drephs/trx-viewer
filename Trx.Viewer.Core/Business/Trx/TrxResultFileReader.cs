using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Trx.Viewer.Core.Abstraction.Business;
using Trx.Viewer.Core.Domain;

namespace Trx.Viewer.Core.Business.Trx
{
    public class TrxResultFileReader : ITestResultFileReader<TrxTestResult>
    {
        private const string XmlNamespace = "TeamTest";

        private bool valid = true;
        private XmlNamespaceManager nsmgr;

        public TrxResultFileReader(IFileSystem fileSystem, ILogger<TrxResultFileReader> logger, IStringLocalizer<TrxResultFileReader> localizer)
        {
            FileSystem = fileSystem;
            Logger = logger;
            Localizer = localizer;
            XmlDocument = new XmlDocument();
        }

        public IFileSystem FileSystem { get; }
        public ILogger<TrxResultFileReader> Logger { get; }
        public IStringLocalizer<TrxResultFileReader> Localizer { get; }
        public XmlDocument XmlDocument { get; private set; }

        public Dictionary<string, List<TrxTestResult>> Read(string inputUri)
        {
            return new Dictionary<string, List<TrxTestResult>>() { { inputUri, ReadFile(inputUri) } };
        }

        public Dictionary<string, List<TrxTestResult>> Read(params string[] inputUris)
        {
            var results = new Dictionary<string, List<TrxTestResult>>();
            foreach (var inputUri in inputUris)
            {
                try
                {
                    results.Add(inputUri, ReadFile(inputUri));
                } catch(System.Exception ex)
                {
                    Logger.LogError(ex.Message);
                }
            }
            return results;
        }

        public bool Validate(string inputUri)
        {
            var schemaSet = new XmlSchemaSet();
            schemaSet.Add("http://microsoft.com/schemas/VisualStudio/TeamTest/2010", Path.Join(Assembly.GetAssembly(typeof(TrxResultFileReader)).Location, "..", "Schema", "TrxSchema.xsd"));
            var settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = schemaSet;
            settings.ValidationEventHandler += ValidationError;
            var reader = XmlReader.Create(inputUri, settings);
            while (reader.Read()) ;
            if (valid)
            {
                Logger.LogDebug(Localizer["Validation Successfull"]);
            }
            reader.Close();
            return valid;
        }

        public Dictionary<string, bool> Validate(string[] inputUris)
        {
            var results = new Dictionary<string, bool>();
            foreach (var inputUri in inputUris)
            {
                results.Add(inputUri, Validate(inputUri));
            }
            return results;
        }

        private void ValidationError(object sender, ValidationEventArgs e)
        {
            Logger.LogError(Localizer["Validation Failed"]);
            Logger.LogError(e.Message);
            valid = false;
        }

        private void SetXmlNamespaceManager()
        {
            if (XmlDocument.DocumentElement.Attributes["xmlns"] != null)
            {
                string xmlns = XmlDocument.DocumentElement.Attributes["xmlns"].Value;
                nsmgr = new XmlNamespaceManager(XmlDocument.NameTable);

                nsmgr.AddNamespace(XmlNamespace, xmlns);
                Logger.LogDebug("Namespace found, using: {0}", xmlns);
            }
        }

        private List<TrxTestResult> ReadFile(string inputUri)
        {
            if (string.IsNullOrWhiteSpace(inputUri)) throw new ArgumentNullException(nameof(inputUri));
            if (!FileSystem.File.Exists(inputUri)) throw new FileNotFoundException(inputUri);
            // todo validation not working with the provided schema
            //if (!Validate(inputUri)) throw new TrxSchemaValidationException(inputUri);

            XmlDocument.Load(inputUri);
            SetXmlNamespaceManager();
            List<TrxTestResult> parsedTrxTestResults = new List<TrxTestResult>();

            var unitTestResults = GetUnitTestResultsFromXmlDocument();
            foreach (XmlNode unitTestResult in unitTestResults)
            {
                parsedTrxTestResults.Add(GetTrxTestResultFromXmlNode(unitTestResult));
            }

            List<TrxTestResult> result = new List<TrxTestResult>();
            // Get all Test Results with a parent id set
            var trxTestResultsWithParent = parsedTrxTestResults.Where(t => t.ParentExecutionId != null && !t.ParentExecutionId.Equals(Guid.Empty));
            if (trxTestResultsWithParent.Any())
            {
                // Get all Test Results without a parent id set
                var parentTrxTestResultsSelection = parsedTrxTestResults.Where(t => t.ExecutionId != null && t.ParentExecutionId == null || t.ParentExecutionId.Equals(Guid.Empty));
                var parentTrxTestResults = new List<TrxTestResult>();

                // Add all Test Results with a parent id set to their parent as children
                parentTrxTestResultsSelection.ToList().ForEach(p => {
                    p.Children = new List<TrxTestResult>(trxTestResultsWithParent.Where(ch => ch.ParentExecutionId == p.ExecutionId));
                    parentTrxTestResults.Add(p);
                });
                Logger.LogDebug("Assigned child nodes to their parents");
                result = parentTrxTestResults;
            }
            else
            {
                result = parsedTrxTestResults;
            }
            Logger.LogInformation(Localizer["Results Found"], parsedTrxTestResults.Count);
            return result;
        }

        #region Xml Nodes Reader

        private TrxTestResult GetTrxTestResultFromXmlNode(XmlNode xmlNode)
        {
            try
            {
                
                var trxTestResult = new TrxTestResult()
                {
                    ExecutionId = Guid.Parse(xmlNode.Attributes.GetNamedItem("executionId").Value),
                    TestId = Guid.Parse(xmlNode.Attributes.GetNamedItem("testId").Value),
                    TestName = xmlNode.Attributes.GetNamedItem("testName").Value,
                    ComputerName = xmlNode.Attributes.GetNamedItem("computerName").Value,
                    Duration = TimeSpan.Parse(xmlNode.Attributes.GetNamedItem("duration").Value),
                    StartTime = DateTime.Parse(xmlNode.Attributes.GetNamedItem("startTime").Value),
                    EndTime = DateTime.Parse(xmlNode.Attributes.GetNamedItem("endTime").Value),
                    Outcome = this.ParseEnumFromXmlNode<TestOutcome>(xmlNode.Attributes, "outcome"),
                    TestListId = Guid.Parse(xmlNode.Attributes.GetNamedItem("testListId").Value)
                };

                trxTestResult.TestListName = GetTestListNameFromXmlDocument(trxTestResult.TestListId);
                trxTestResult.TestClass = GetTestClassNameFromXmlDocument(trxTestResult.TestId);
                trxTestResult.ErrorInfo = GetTestErrorInfoFromXmlNode(xmlNode);

                if (Guid.TryParse(xmlNode.Attributes.GetNamedItem("parentExecutionId")?.Value, out var parentExecutionId))
                {
                    trxTestResult.ParentExecutionId = parentExecutionId;
                    if (int.TryParse(xmlNode.Attributes.GetNamedItem("dataRowInfo")?.Value, out var dataRowInfo))
                        trxTestResult.DataRowInfo = dataRowInfo;
                    if (this.TryParseEnumFromXmlNode<TestResultType>(xmlNode.Attributes, "resultType", out var testResultType))
                        trxTestResult.ResultType = testResultType;
                }
                Logger.LogDebug("Parsed Trx Result with ID '{0}'", trxTestResult.ExecutionId);
                return trxTestResult;
            }
            catch (System.FormatException ex)
            {
                Logger.LogError(Localizer["Trx TestResult Parse Error"], ex.Message);
                return new TrxTestResult();
            }
        }

        private XmlNodeList GetUnitTestResultsFromXmlDocument()
        {
            if (nsmgr != null) { return XmlDocument.DocumentElement.SelectNodes($"//{XmlNamespace}:UnitTestResult", nsmgr); }
            else { return XmlDocument.DocumentElement.SelectNodes("//UnitTestResult"); }
        }

        private string GetTestClassNameFromXmlDocument(Guid id)
        {
            XmlNode xmlNode;
            if (nsmgr != null) { xmlNode = XmlDocument.DocumentElement.SelectSingleNode($"//{XmlNamespace}:TestDefinitions/{XmlNamespace}:UnitTest[@id='{id}']/{XmlNamespace}:TestMethod", nsmgr); }
            else { xmlNode = XmlDocument.DocumentElement.SelectSingleNode($"//TestDefinitions/UnitTest[@id='{id}']/TestMethod"); }
            return xmlNode.Attributes.GetNamedItem("className").Value;
        }

        private string GetTestListNameFromXmlDocument(Guid id)
        {
            XmlNode xmlNode;
            if (nsmgr != null) { xmlNode = XmlDocument.DocumentElement.SelectSingleNode($"//{XmlNamespace}:TestLists/{XmlNamespace}:TestList[@id='{id}']", nsmgr); }
            else { xmlNode = XmlDocument.DocumentElement.SelectSingleNode($"//TestLists/TestList[@id='{id}']"); }
            return xmlNode.Attributes.GetNamedItem("name").Value;
        }

        private TestErrorInfo? GetTestErrorInfoFromXmlNode(XmlNode xmlNode)
        {
            if (!xmlNode.HasChildNodes) return null;

            string errorInfoMessage;

            if (nsmgr != null) { errorInfoMessage = xmlNode.SelectSingleNode($"//{XmlNamespace}:ErrorInfo/{XmlNamespace}:Message", nsmgr)?.InnerText; }
            else { errorInfoMessage = xmlNode.SelectSingleNode("//ErrorInfo/Message").InnerText; }

            string errorInfoStacktrace;

            if (nsmgr != null) { errorInfoStacktrace = xmlNode.SelectSingleNode($"//{XmlNamespace}:ErrorInfo/{XmlNamespace}:StackTrace", nsmgr)?.InnerText; }
            else { errorInfoStacktrace = xmlNode.SelectSingleNode("//ErrorInfo/StackTrace").InnerText; }

            if (errorInfoMessage == null || errorInfoStacktrace == null) return null;

            return new TestErrorInfo(errorInfoMessage, errorInfoStacktrace);
        }

        #endregion
    }
}
