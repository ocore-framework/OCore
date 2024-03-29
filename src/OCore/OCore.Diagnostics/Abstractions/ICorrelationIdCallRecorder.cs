﻿using OCore.Core;
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
        Task Request(string? from, string to, string parameters, int hopCount);

        [Internal]
        Task Complete(string? from, string to, string? result, int hopCount);

        [Internal]
        Task Fail(string methodName, string previousMethodName, string exceptionType, string message, int hopCount);

        Task<PlainText> ToMermaid();
    }
}
