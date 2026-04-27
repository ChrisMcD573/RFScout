using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFScout.Core.Domain;

namespace RFScout.Infrastructure.Scanning
{
    public interface IDeviceScanner
    {
        event Action<DeviceSignal> DeviceFound;
        void Start();
        void Stop();
        // void SetSensitivity(Proximity ringThreshold);
        
    }

    public enum Proximity
    {
        Very_Close = 1,
        Near = 2,
        Far = 3,
        Edge = 4
    }
}
