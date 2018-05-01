using System;
using System.IO;
using Fabric.IdentityProviderSearchService.Configuration;
using Serilog;
using Serilog.Core;

namespace Fabric.IdentityProviderSearchService.Logging
{
    public class LogFactory
    {
        public static ILogger CreateTraceLogger(LoggingLevelSwitch levelSwitch, ApplicationInsights appInsightsConfig)
        {
            var loggerConfiguration = CreateLoggerConfiguration(levelSwitch);

            if (appInsightsConfig != null && appInsightsConfig.Enabled &&
                !string.IsNullOrEmpty(appInsightsConfig.InstrumentationKey))
            {
                loggerConfiguration.WriteTo.ApplicationInsightsTraces(appInsightsConfig.InstrumentationKey);
            }

            return loggerConfiguration.CreateLogger();
        }

        private static LoggerConfiguration CreateLoggerConfiguration(LoggingLevelSwitch levelSwitch)
        {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.RollingFile(Path.Combine(currentDirectory, "logs\\idpsearchservice-{Date}.log"));

        }
    }
}