using System.Collections.Generic;
using System.Net.Http;
using OCore.Core;
using OCore.Diagnostics.Entities;
using OCore.Entities.Data;
using System.Threading.Tasks;
using OCore.Http.DataTypes;

namespace OCore.Diagnostics.Abstractions
{
    [DataEntity("OCore.CorrelationIdRecorder", dataEntityMethods: DataEntityMethods.Read)]
    public interface ICorrelationIdCallRecorder : IDataEntity<CorrelationIdCallRecord>
    {
        [Internal]
        Task Request(string? from, string to, string parameters);

        [Internal]
        Task Complete(string? from, string to, string? result);

        [Internal]
        Task Fail(string methodName, string previousMethodName, string exceptionType, string message);

        Task<PlainText> ToMermaid();
    }
}
