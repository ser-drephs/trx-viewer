using System;
using System.Collections.Generic;
using System.Linq;
using Trx.Viewer.Core.Abstraction.Domain;

namespace Trx.Viewer.Core.Domain
{
    public struct TrxTestResult : ITestResult
    {
        public Guid ExecutionId { get; set; } // Zuordnung child zu parent test results
        public Guid TestId { get; set; }
        public string TestName { get; set; }
        public string ComputerName { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TestOutcome Outcome { get; set; }
        public Guid TestListId { get; set; } // Zuordnung zu einer TestList wobei nur der Name interessant ist.
        public string TestListName { get; set; }
        public string TestClass { get; set; } // Basierend auf der TestId kann die TestClass ermittelt werden
        public Guid? ParentExecutionId { get; set; }
        public int? DataRowInfo { get; set; }
        public TestResultType ResultType { get; set; }
        public TestErrorInfo? ErrorInfo { get; set; }
        public List<TrxTestResult> Children { get; set; }

        public bool HasChildren()
        {
            return Children != null && Children.Any();
        }
    }
}
