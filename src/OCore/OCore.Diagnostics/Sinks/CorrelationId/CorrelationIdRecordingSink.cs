﻿using Newtonsoft.Json;
using OCore.Diagnostics.Abstractions;
using OCore.Diagnostics.Filters;
using OCore.Entities.Data.Extensions;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Diagnostics.Sinks.CorrelationId
{    
    public class CorrelationIdRecordingSink : IDiagnosticsSink
    {
        public bool IsPaused { get; set; } = false;
        public bool EnableOCoreInternal { get; set; } = false;

        private readonly IGrainFactory grainFactory;

        public CorrelationIdRecordingSink(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task Complete(DiagnosticsPayload request, IGrainCallContext grainCallContext)
        {
            var recorderGrain = grainFactory.GetDataEntity<ICorrelationIdCallRecorder>(request.CorrelationId);

            var result = grainCallContext.Result?.ToString();

            // Flip MethodName and PreviousMethodName here as it is working its way down the call stack
            await recorderGrain.Complete(request.MethodName, 
                request.PreviousMethodName!, 
                result, request.HopCount);
        }

        public async Task Fail(DiagnosticsPayload request, IGrainCallContext grainCallContext, Exception ex)
        {
            var recorderGrain = grainFactory.GetDataEntity<ICorrelationIdCallRecorder>(request.CorrelationId);

            await recorderGrain.Fail(
                request.MethodName!, 
                request.PreviousMethodName!, 
                ex.GetType().ToString(), 
                ex.Message,
                request.HopCount);
        }

        public async Task Request(DiagnosticsPayload request, IGrainCallContext grainCallContext)
        {
            var recorderGrain = grainFactory.GetDataEntity<ICorrelationIdCallRecorder>(request.CorrelationId);

            var list = new List<string>();

            var sb = new StringBuilder();

            sb.Append("(");

            for (int i = 0; i < grainCallContext.Request.GetArgumentCount(); i++)
            {
                list.Add(JsonConvert.SerializeObject(grainCallContext.Request.GetArgument(i)));
            }

            sb.Append(string.Join(", ", list.ToArray()));

            sb.Append(")");

            var parameters = sb.ToString();

            await recorderGrain.Request(
                request.PreviousMethodName, 
                request.MethodName!, 
                parameters,
                request.HopCount);
        }
    }
}
