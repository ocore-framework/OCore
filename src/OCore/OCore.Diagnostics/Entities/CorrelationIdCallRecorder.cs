using System;
using Microsoft.Extensions.Options;
using OCore.Diagnostics.Abstractions;
using OCore.Diagnostics.Options;
using OCore.Entities.Data;
using Orleans;
using Orleans.Runtime;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OCore.Http.DataTypes;

namespace OCore.Diagnostics.Entities
{
    [GenerateSerializer]
    public class CallEntry
    {
        [Id(0)] public string? From { get; init; }

        [Id(1)] public string? To { get; init; }

        [Id(2)] public string? Parameters { get; init; }

        [Id(3)] public string? Result { get; init; }

        [Id(4)] public string? ExceptionMessage { get; init; }

        [Id(5)] public string? ExceptionType { get; init; }
    }

    [GenerateSerializer]
    public class CorrelationIdCallRecord
    {
        [Id(0)] public List<CallEntry> Entries { get; set; } = new List<CallEntry>();

        [Id(1)] public string? RequestSource { get; set; }
    }

    public class CorrelationIdCallRecorder : DataEntity<CorrelationIdCallRecord>, ICorrelationIdCallRecorder
    {
        readonly DiagnosticsOptions _diagnosticsOptions;

        readonly ICorrelationIdRegistry? _correlationIdRegistry;

        public CorrelationIdCallRecorder(IOptions<DiagnosticsOptions> diagnosticsOptions)
        {
            _diagnosticsOptions = diagnosticsOptions.Value;
            if (_diagnosticsOptions.StoreInCorrelationIdRegistry == true)
            {
                _correlationIdRegistry = Get<ICorrelationIdRegistry>("Registry");
            }
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            if (_diagnosticsOptions.StoreCorrelationIdData == false
                && Created == false)
            {
                Created = true;
            }
        }

        public async Task Complete(string? from, string to, string? result)
        {
            State.Entries.Add(new CallEntry
            {
                From = from,
                To = to,
                Result = result
            });

            if (_diagnosticsOptions.StoreCorrelationIdData == true)
            {
                await WriteStateAsync();
            }

            if (_diagnosticsOptions.StoreInCorrelationIdRegistry == true)
            {
                await RegisterCorrelationId();
            }
        }

        public async Task Fail(string methodName,
            string previousMethodName,
            string exceptionType, string message)
        {
            State.Entries.Add(new CallEntry
            {
                From = methodName,
                To = previousMethodName,
                ExceptionMessage = message,
                ExceptionType = exceptionType
            });

            if (_diagnosticsOptions.StoreCorrelationIdData == true)
            {
                await WriteStateAsync();
            }

            if (_diagnosticsOptions.StoreInCorrelationIdRegistry == true)
            {
                await RegisterCorrelationId();
            }
        }

        private async Task RegisterCorrelationId()
        {
            if (_correlationIdRegistry is not null)
            {
                await _correlationIdRegistry.Register(PrimaryKeyString);
            }
        }

        public async Task Request(string? from, string to, string parameters)
        {
            State.Entries.Add(new CallEntry
            {
                To = to,
                From = from,
                Parameters = parameters
            });

            if (State.RequestSource == null)
            {
                State.RequestSource = RequestContext.Get("D:RequestSource") as string;
            }

            if (_diagnosticsOptions.StoreCorrelationIdData == true)
            {
                await WriteStateAsync();
            }

            if (_diagnosticsOptions.StoreInCorrelationIdRegistry == true)
            {
                await RegisterCorrelationId();
            }
        }

        public Task<PlainText> ToMermaid()
        {
            var sb = new StringBuilder();
            var participants = new HashSet<string>();

            if (State.RequestSource != null)
            {
                participants.Add(State.RequestSource);
            }

            foreach (var entry in State.Entries)
            {
                if (entry.From != null)
                {
                    participants.Add(entry.From);
                }

                if (entry.To != null)
                {
                    participants.Add(entry.To);
                }
            }

            sb.AppendLine("sequenceDiagram");

            foreach (var participant in participants)
            {
                sb.AppendLine($"   participant {participant}");
            }

            foreach (var entry in State.Entries)
            {
                var from = entry.From;
                if (from == null)
                {
                    from = State.RequestSource;
                }

                var to = entry.To;
                if (to == null)
                {
                    to = State.RequestSource;
                }

                if (entry.Parameters != null)
                {
                    sb.AppendLine($"   {from}->>+{to}: {TruncateString(entry.Parameters, 15)}");
                }

                if (entry.Result != null)
                {
                    sb.AppendLine($"   {from}->>-{to}: {entry.Result}");
                }

                if (entry.ExceptionMessage != null)
                {
                    sb.AppendLine($"   {from}-x-{to}: {entry.ExceptionType}: {entry.ExceptionMessage}");
                }
            }

            byte[] utf8Bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var returnString = Encoding.UTF8.GetString(utf8Bytes);
            return Task.FromResult(new PlainText { Text = returnString });
        }

        static string TruncateString(string input, int maxLength)
        {
            if (input.Length <= maxLength)
            {
                return input;
            }
            else
            {
                return input.Substring(0, maxLength) + "...";
            }
        }
    }
}