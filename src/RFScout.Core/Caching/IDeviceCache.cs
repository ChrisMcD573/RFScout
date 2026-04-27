using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFScout.Core.Domain;

namespace RFScout.Core.Caching
{
    public interface IDeviceCache
    {
        void AddOrUpdateDevice(DeviceSignal signal);
        IEnumerable<DeviceState> GetAllDevices();

        IEnumerable<DeviceState> GetAllMovingDevices();

        void Cleanup(DateTime cutoff);
        void Clear();
    }

    public enum VelocityCategory
    {
        Stationary,
        SmallDrift,
        Walking,
        FastMovement
    }
}
