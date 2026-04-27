using System;
using System.IO;
using System.Threading.Tasks;
using RFScout.Core.Domain;
using RFScout.Infrastructure.Scanning;

namespace RFScout.Tests
{
    public class CsvMockScanner : IDeviceScanner
    {
        private readonly string _csvPath;
        private event Action<DeviceSignal>? _deviceFound;

        public event Action<DeviceSignal> DeviceFound
        {
            add => _deviceFound += value;
            remove => _deviceFound -= value;
        }

        public CsvMockScanner(string csvPath)
        {
            _csvPath = csvPath;
        }

        public async void Start()
        {
            // Expected CSV Format: DelayMs,Address,Name,Rssi
            // Example: 500,AA:BB:CC:DD:EE:FF,iPhone,-65
            
            var lines = await File.ReadAllLinesAsync(_csvPath);
            
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var parts = line.Split(',');
                int delayMs = int.Parse(parts[0]);
                string address = parts[1];
                string name = parts[2];
                short rssi = short.Parse(parts[3]);

                await Task.Delay(delayMs); // Simulate the real-time gap between signals

                var signal = new DeviceSignal(
                    Address: address,
                    Name: name,
                    Rssi: rssi,
                    Type: SourceType.Bluetooth,
                    Timestamp: DateTime.UtcNow
                );

                _deviceFound?.Invoke(signal);
            }
        }

        public void Stop() { }
    }
}