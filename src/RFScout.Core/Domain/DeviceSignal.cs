using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFScout.Core.Domain
{
    /// <summary>
    /// Represents a single signal detected from a device, including its address, name, signal strength (RSSI), source type, and the timestamp of when it was observed.
    /// </summary>
    /// <param name="Address"></param>
    /// <param name="Name"></param>
    /// <param name="Rssi"></param>
    /// <param name="Type"></param>
    /// <param name="Timestamp"></param>
    public record struct DeviceSignal(string Address,
                            string Name,
                            int Rssi,
                            SourceType Type,
                            DateTime Timestamp);
}
