
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFScout.Caching;
using RFScout.Scanning;
using Microsoft.Extensions.Logging;

namespace RFScout
{
    /// <summary>
    /// Main application class that orchestrates the device scanning and caching. It listens for device signals from the scanner and updates the cache accordingly, while periodically displaying the list of recently seen devices.
    /// </summary>
    internal class App
    {
        private readonly ILogger<App> _logger;
        private readonly IDeviceCache _cache;
        private readonly IDeviceScanner _scanner;
        private readonly ScanMediator _mediator;

        public App(ILogger<App> logger, IDeviceCache deviceCache, IDeviceScanner scanner)
        {
            _logger = logger;
            _cache = deviceCache;
            _scanner = scanner;
            _scanner.DeviceFound += signal => _cache.AddOrUpdateDevice(signal);

            _mediator = new ScanMediator(scanner, (DeviceCache)deviceCache);
        }

        public async Task RunAsync()
        {
            _scanner.Start();
            _mediator.Start();

            _logger.LogInformation("Scanning for devices...");
            while (true)
            {
                var newDevices = _cache.GetAllMovingDevices();// TODO: consider adding a timestamp to the device state and only show devices seen in the last N seconds

                //Console.Clear();
                _logger.LogDebug("Recently seen devices:");
                foreach (var d in newDevices)
                {
                    _logger.LogDebug(_logger.IsEnabled(LogLevel.Debug) ? $"Device: {d.Name} ({d.Address}), RSSI={d.CurrentRssi}, Velocity={d.SmoothedVelocity:F2}" : "Device: {Name} ({Address})",
                        d.Name, d.Address);
                }
                await Task.Delay(2000);
            }
        }
    }
}