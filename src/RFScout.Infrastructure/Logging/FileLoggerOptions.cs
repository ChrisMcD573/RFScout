using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RFScout.Infrastructure.Logging
{
    public class FileLoggerOptions
    {
        public string FilePath { get; set; } = "logs/app.log";
        public bool IncludeCategory { get; set; } = false;
        public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;
    }
}
