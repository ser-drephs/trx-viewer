using System;
using System.IO;

namespace Trx.Viewer.Core.UnitTests
{
    public static class TestHelper
    {
        public static string GetTestDataDirectory()
        {
            return Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Data");
        }
    }
}
