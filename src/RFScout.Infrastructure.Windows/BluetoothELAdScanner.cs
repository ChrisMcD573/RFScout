// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection.Emit;
// using System.Text;
// using System.Threading.Tasks;
// using RFScout.Core.Domain;

// namespace RFScout.Infrastructure.Scanning
// {
//     public class BluetoothELAdScanner : IDeviceScanner
//     {
//         private int _minRssi = -100; // Default minimum RSSI threshold for device discovery
//         private readonly BluetoothLEAdvertisementWatcher watcher;
//         private event Action<DeviceSignal> DeviceFound;

//         public BluetoothELAdScanner()
//         {
//             watcher = new BluetoothLEAdvertisementWatcher();
//             watcher.Received += Watcher_Received;
//         }
//         event Action<DeviceSignal> IDeviceScanner.DeviceFound
//         {
//             add
//             {
//                 DeviceFound += value;
//             }

//             remove
//             {
//                 throw new NotImplementedException();
//             }
//         }
//         private void Watcher_Received(BluetoothLEAdvertisementWatcher sender,
//                                   BluetoothLEAdvertisementReceivedEventArgs args)
//         {
//             if (args.RawSignalStrengthInDBm >= _minRssi)
//             {
//                 var signal = new DeviceSignal(
//                     Address: args.BluetoothAddress.ToString("X"),
//                     Name: args.Advertisement.LocalName,
//                     Rssi: args.RawSignalStrengthInDBm,
//                     Type: SourceType.Bluetooth,
//                     Timestamp: DateTime.Now
//                 );

//                 DeviceFound?.Invoke(signal);
//             }
//         }

//         void IDeviceScanner.Start()
//         {
//             if (watcher.Status == BluetoothLEAdvertisementWatcherStatus.Aborted)
//             {
//                 Console.WriteLine("Failed to initialize Bluetooth LE watcher. Please ensure Bluetooth is enabled.");
//                 watcher.Stop();
//             }
//             watcher.ScanningMode = BluetoothLEScanningMode.Active;
//             try
//             {
//                 watcher.Start();
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error starting Bluetooth LE watcher: {ex.Message}");
//                 Console.WriteLine($"ErrorCode: {ex.HResult}");
//                 Console.WriteLine(ex.ToString());
//             }
//         }
//         void IDeviceScanner.Stop()
//         {
//             try
//             {
//                 watcher.Stop();
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error starting Bluetooth LE watcher: {ex.Message}");
//                 Console.WriteLine($"ErrorCode: {ex.HResult}");
//                 Console.WriteLine(ex.ToString());
//             }
//         }

//         public void SetSensitivity(Proximity ringThreshold)
//         {

//             int level = ringThreshold switch
//             {
//                 Proximity.Very_Close => -40,
//                 Proximity.Near => -60,
//                 Proximity.Far => -80,
//                 Proximity.Edge => -100,
//                 _ => -100
//             };
//             _minRssi = level;
//         }
//     }
// }
