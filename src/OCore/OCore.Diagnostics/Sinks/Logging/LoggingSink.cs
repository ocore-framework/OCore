﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OCore.Diagnostics.Filters;
using Orleans;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCore.Diagnostics.Sinks.Logging
{
    public class LoggingSinkOptions
    {
        public bool Enabled { get; set; } = true;

        public bool LogArguments { get; set; } = false;
    }

    public class LoggingSink : IDiagnosticsSink
    {
        public bool IsPaused { get; set; } = false;

        public bool EnableOCoreInternal { get; set; } = false;

        ILogger logger;
        LoggingSinkOptions options;
        public LoggingSink(ILogger<LoggingSink> logger,
            IOptions<LoggingSinkOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;
        }

        public Task Request(DiagnosticsPayload request, IGrainCallContext grainCallContext)
        {
            if (CheckWhetherToLog(grainCallContext) == false) return Task.CompletedTask;

            logger.LogInformation("> " + request.ToString());

            return Task.CompletedTask;
        }

        private bool CheckWhetherToLog(IGrainCallContext grainCallContext)
        {
            if (IsPaused == true || options.Enabled == false) return false;

            var fullName = grainCallContext?.Grain?.GetType()?.FullName;

            if (fullName == null) return false;

            // Do not log internal calls
            if (fullName.StartsWith("Orleans.")) return false;
            if (EnableOCoreInternal == false && fullName.StartsWith("OCore.")) return false;
            return true;
        }

        public Task Complete(DiagnosticsPayload request, IGrainCallContext grainCallContext)
        {
            try
            {
                if (CheckWhetherToLog(grainCallContext) == false) return Task.CompletedTask;

                logger.LogInformation("< " + request.ToString());

                if (request.HopCount == 0)
                {
                    logger.LogInformation($"Request completed in {(DateTimeOffset.UtcNow - request.CreatedAt).TotalMilliseconds}ms");
                }

                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task Fail(DiagnosticsPayload request, IGrainCallContext grainCallContext, Exception ex)
        {
            if (CheckWhetherToLog(grainCallContext) == false) return Task.CompletedTask;
            if (options.LogArguments == true)
            {
                var list = new List<string>();
                for (int i = 0; i < grainCallContext.Request.GetArgumentCount(); i++)
                {
                    list.Add(JsonConvert.SerializeObject(grainCallContext.Request.GetArgument(i)));
                }
                
                logger.LogError(ex,  string.Join(", ", list.ToArray()));
            }
            return Task.CompletedTask;
        }
    }
}
