using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFScout.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RFScout.Infrastructure.Logging
{
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFileLogger(
        this ILoggingBuilder builder,
        Action<FileLoggerOptions> configure)
        {
            builder.Services.Configure(configure);
            builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();

            //var options = new FileLoggerOptions();
            //configure(options);

            //builder.AddProvider(new FileLoggerProvider(options));
            return builder;
        }
    }
}
