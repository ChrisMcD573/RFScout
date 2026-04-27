using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RFScout.Core.Caching;

namespace RFScout.Infrastructure.Scanning
{
    public class ScanMediator
    {
        private readonly IDeviceScanner _scanner;
        private readonly IDeviceCache _cache;
        private readonly Timer _timer;
        // private readonly ILogger<ScanMediator> _logger;
        public ScanMediator(IDeviceScanner scanner, IDeviceCache cache)
        {
            _scanner = scanner;
            _cache = cache;
            // _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ScanMediator>();
            _timer = new Timer(AdjustScanningStrategy(Proximity.Edge), null, 0, 500);
        }

        public TimerCallback AdjustScanningStrategy(Proximity proximity)
        {
            // Always scan perimeter
            // _scanner.SetSensitivity(proximity);

            foreach (var device in _cache.GetAllDevices())
            {
                if (Math.Abs(device.SmoothedVelocity) > 0.5)
                {
                    // Moving device → scan its ring
                    // _scanner.SetSensitivity(device.CurrentProximity);
                }
            }
            return _ => AdjustScanningStrategy(proximity); // reschedule the timer
        }

        public void Start() {
            _timer.Change(0, 500); // adjust scanning every 500ms
        }
    }
}
