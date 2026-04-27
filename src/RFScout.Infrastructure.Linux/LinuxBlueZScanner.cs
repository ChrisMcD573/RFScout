using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RFScout.Core.Domain;
using RFScout.Infrastructure.Scanning;
using Tmds.DBus;

namespace RFScout.Infrastructure.Linux.Scanning
{
    public class LinuxBlueZScanner : IDeviceScanner
    {
        private int _minRssi;

        private event Action<DeviceSignal>? _deviceFound;
        private Connection _connection;
        private IDisposable? _watcher;

        public event Action<DeviceSignal> DeviceFound
        {
            add => _deviceFound += value;
            remove => _deviceFound -= value;
        }
        public LinuxBlueZScanner()
        {
            _connection = new Connection(Address.System);
        }

        public async void Start()
        {
            await _connection.ConnectAsync();

            var manager = _connection.CreateProxy<IObjectManager>("org.bluez", "/");
            _watcher = await manager.WatchInterfacesAddedAsync(HandleInterfacesAdded);

            var adapter = _connection.CreateProxy<IAdapter1>("org.bluez", "/org/bluez/hci0");
            await adapter.StartDiscoveryAsync();

        }

        public void Stop()
        {
            // Stop the process or DBus subscription
        }

        // public void SetSensitivity(Proximity ring)
        // {
        //     _minRssi = ring switch
        //     {
        //         Proximity.Very_Close => -40,
        //         Proximity.Near => -60,
        //         Proximity.Far => -80,
        //         Proximity.Edge => -100,
        //         _ => -100
        //     };

        // }
        private void HandleInterfacesAdded((ObjectPath path, IDictionary<string, IDictionary<string, object>> interfaces) args)
        {
            if (!args.interfaces.TryGetValue("org.bluez.Device1", out var props))
                return;

            if (!props.TryGetValue("RSSI", out var rssiObj))
                return;

            int rssi = (short)rssiObj;
            if (rssi < _minRssi)
                return;

            string address = props["Address"] as string ?? "";
            string name = props.ContainsKey("Name") ? props["Name"] as string : "";

            var signal = new DeviceSignal(
                Address: address,
                Name: name ?? "Uknown Device",
                Rssi: rssi,
                Type: SourceType.Bluetooth,
                Timestamp: DateTime.UtcNow
            );

            _deviceFound?.Invoke(signal);
        }


        [DBusInterface("org.freedesktop.DBus.ObjectManager")]
        interface IObjectManager : IDBusObject
        {
            Task<IDictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>>> GetManagedObjectsAsync();

            Task<IDisposable> WatchInterfacesAddedAsync(
                Action<(ObjectPath path, IDictionary<string, IDictionary<string, object>> interfaces)> handler);
        }

        [DBusInterface("org.bluez.Adapter1")]
        interface IAdapter1 : IDBusObject
        {
            Task StartDiscoveryAsync();
            Task StopDiscoveryAsync();
        }
    }
}
