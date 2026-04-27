using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFScout.Core.Domain;

namespace RFScout.Core.Caching
{
    /// <summary>
    /// Manages the state of detected devices, including their RSSI history and movement velocity.
    /// </summary>
    public class DeviceCache : IDeviceCache
    {
        // private readonly ILogger<DeviceCache> _logger;
        private readonly Dictionary<string, DeviceState> _devices = [];
        private readonly object _lock = new();

        public DeviceCache()
            // ILogger<DeviceCache> logger)
        {
            // _logger = logger;
        }
        /// <summary>
        /// Entry point for adding or updating a device in the cache. 
        /// It checks if the device already exists and updates its state accordingly,
        /// or adds it as a new entry if it's not already present. 
        /// </summary>
        /// <param name="signal">Scanner produced struct</param>
        public void AddOrUpdateDevice(DeviceSignal signal)
        {
            // _logger.LogDebug("Adding/updating device: {Name} ({Address}) RSSI={RSSI} ",
            //     signal.Name,
            //     signal.Address,
            //     signal.Rssi);
            DeviceState currentDeviceState;
            lock (_lock)
            {
                if (_devices.TryGetValue(signal.Address, out var pre))
                {
                    // _logger.LogInformation("existing device {Name} ({Address}) RSSI changed from {PrevRssi} to {CurrRssi}",
                    //     signal.Name, signal.Address, pre.CurrentRssi, signal.Rssi);
                    currentDeviceState = pre;
                    currentDeviceState.UpdateFromSnapshot(signal);
                    // _logger.LogTrace(_logger.IsEnabled(LogLevel.Trace) ? $"Updated device {signal.Name} ({signal.Address}): " +
                    //     $"Prev RSSI={pre.CurrentRssi}, Curr RSSI={currentDeviceState.CurrentRssi}, " +
                    //     $"Velocity={currentDeviceState.SmoothedVelocity:F2}" : "Updated device {Name} ({Address})",
                    //     signal.Name, signal.Address);
                } else
                {
                    // _logger.LogInformation("Adding new device {Name} ({Address}) with RSSI={Rssi}",
                    //     signal.Name, signal.Address, signal.Rssi);
                    currentDeviceState = new DeviceState(signal);
                    // _logger.LogTrace(_logger.IsEnabled(LogLevel.Trace) ? $"Added new device {signal.Name} ({signal.Address}): " +
                        // $"RSSI={currentDeviceState.CurrentRssi}" : "Added new device {Name} ({Address})",
                        // signal.Name, signal.Address);
                }
                _devices[signal.Address] = currentDeviceState;
            }
        }
        public IEnumerable<DeviceState> GetAllDevices() // 
        {
            var list = new List<DeviceState>();

            foreach (var d in _devices.Values)
            {
                //if (d.Timestamp >= cutoff)
                //    list.Add(d);
            }

            list.Sort((a, b) => a.CurrentRssi.CompareTo(b.CurrentRssi));

            return list;
        }
        public void Clear()
        {
            lock (_devices)
            {
                _devices.Clear();
            }
        }

        public IEnumerable<DeviceState> GetAllMovingDevices()
        {
            var movementThreshold = 0.5; // Adjust as needed
            var list = new List<DeviceState>(); // Use List<DeviceSignal> 
            lock (_lock)
            {
                foreach (var d in _devices.Values)
                {
                    if (d.SmoothedVelocity > movementThreshold)
                        list.Add(d);
                }

                list.Sort((a, b) => a.CurrentRssi.CompareTo(b.CurrentRssi));
                return list;
            }
        }

        public void Cleanup(DateTime cutoff)
        {
            var remove = new List<string>();

            foreach (var kv in _devices)
            {
                if (kv.Value.LastSeen< cutoff)
                    remove.Add(kv.Key);
            }

            foreach (var key in remove)
                _devices.Remove(key);
        }
    }

    // implement something to getSnapShot();
}
