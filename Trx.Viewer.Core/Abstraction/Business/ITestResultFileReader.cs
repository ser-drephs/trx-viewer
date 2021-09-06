using System.Collections.Generic;

namespace Trx.Viewer.Core.Abstraction.Business
{
    public interface ITestResultFileReader<T> where T : struct
    {
        Dictionary<string, List<T>> Read(string inputUri);

        Dictionary<string, List<T>> Read(string[] inputUris);

        bool Validate(string inputUri);

        Dictionary<string, bool> Validate(string[] inputUris);
    }
}
