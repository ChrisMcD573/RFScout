// See https://aka.ms/new-console-template for more information
using System;
using System.Threading.Tasks;
using RFScout.Caching;
// using RFScout.Scanning; // Ensure this points to where IDeviceScanner lives (e.g., RFScout.Infrastructure.Scanning)
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RFScout.Logging;
using System.Runtime.InteropServices;
using RFScout.Infrastructure.Scanning; 

namespace RFScout
{
    /// <summary>
    /// Main entry point for the scanner application.
    /// This class sets up the host, configures logging, and initializes services.
    /// </summary>
    public class Program
    {
        /*
         * Configuration and logging setup:
         */
        private const string LOG_FILE_PATH   = "logs/app.log";
        private const bool CATEGORY_IN_LOGS = false; // Set to true to include category in log entries, false to exclude for cleaner logs
        private const LogLevel CONSOLE_LOG_LEVEL = LogLevel.Debug;
        private const LogLevel FILE_LOG_LEVEL = LogLevel.Trace;
        private const LogLevel MINIMUM_LOG_LEVEL = LogLevel.Trace; // Set the minimum log level for the entire application
        
        static async Task Main()
        {
            bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var builder = Host.CreateApplicationBuilder();
            
            // Allow all log levels to be captured, filtering will be done by individual providers
            builder.Logging.SetMinimumLevel(MINIMUM_LOG_LEVEL);

            // 1. Add console logging and filter for debug minimum
            builder.Logging.AddConsole();
            builder.Logging.AddFilter("Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider", CONSOLE_LOG_LEVEL);

            // 2. Add file logging and filter for trace minimum
            builder.Logging.AddFileLogger(options =>
            {
                options.FilePath = LOG_FILE_PATH;
                options.IncludeCategory = CATEGORY_IN_LOGS;
                options.MinimumLevel = FILE_LOG_LEVEL;
            });

            // 3. Add services
            builder.Services.AddSingleton<IDeviceCache, DeviceCache>();
            
            // Dynamically inject the OS-specific Bluetooth scanner instead of using the Factory
            if (isLinux)
            {
                builder.Services.AddSingleton<IDeviceScanner, RFScout.Infrastructure.Linux.LinuxBlueZScanner>();
            }
            else if (isWindows)
            {
                builder.Services.AddSingleton<IDeviceScanner, RFScout.Infrastructure.Windows.BluetoothELAdScanner>();
            }
            else
            {
                throw new PlatformNotSupportedException("This OS is not supported for RF scanning.");
            }

            // 4. Add the main application class
            builder.Services.AddSingleton<App>();

            var app = builder.Build().Services.GetRequiredService<App>();

            await app.RunAsync();
        }
    }
}