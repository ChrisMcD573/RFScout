using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RFScout.Core.Domain;

namespace RFScout.Core.Caching
{
    /// <summary>
    /// This consolidates all the state we want to track for each device in one place. It will be updated by the cache logic and can be used by the mediator to make decisions about what to feed back to the scanner or how to prioritize devices.
    /// </summary> 
    public class DeviceState
    {
        private const int BUFFER_SIZE = 10;
        public readonly string Address;
        public readonly string Name;
        public readonly SourceType Type;
        private readonly RssiSample[] _buffer = new RssiSample[BUFFER_SIZE];
        private int _index = 0;
        private int _count = 0;

        public int CurrentRssi => _count > 0
    ? CurrentSample.Rssi
    : 0;

        public int PreviousRssi => _count > 1
            ? PreviousSample.Rssi
            : CurrentRssi;


        // public Proximity CurrentProximity { get; private set; }
        // public Proximity PreviousProximity { get; private set; }

        public DateTime LastSeen { get; private set; }
        public DateTime PreviousSeen { get; private set; }

        public int RssiBufferCount => _count;

        // potential upgrades: Signal Stability, Signal Trend, Device Presence score - detects intermittent devices vs beacons, 
        public DeviceState(DeviceSignal signal)
        {
            Address = signal.Address;
            AddSample(new RssiSample(signal.Rssi, signal.Timestamp));
            LastSeen = signal.Timestamp;
            Name = signal.Name;
            Type = signal.Type;
        }

        // This will be filled in by the cache update logic later
        public void UpdateFromSnapshot(DeviceSignal snapshot)
        {
            AddSample(new RssiSample(snapshot.Rssi, snapshot.Timestamp));

            // --- Update timestamps ---
            PreviousSeen = LastSeen;
            LastSeen = snapshot.Timestamp;
        }
        internal readonly struct RssiSample
        {
            public int Rssi { get; }
            public DateTime Timestamp { get; }

            public RssiSample(int rssi, DateTime timestamp)
            {
                Rssi = rssi;
                Timestamp = timestamp;
            }
        }

        private void AddSample(RssiSample sample)
        {
            _buffer[_index] = sample;
            _index = (_index + 1) % BUFFER_SIZE;

            if (_count < BUFFER_SIZE)
                _count++;
        }

        private RssiSample CurrentSample =>
    _buffer[(_index - 1 + BUFFER_SIZE) % BUFFER_SIZE];

        private RssiSample PreviousSample =>
            _buffer[(_index - 2 + BUFFER_SIZE) % BUFFER_SIZE];

    public double SmoothedRssi()
        {
            if (_count == 0)
                return 0;

            double sum = 0;

            for (int i = 0; i < _count; i++)
                sum += _buffer[i].Rssi;

            return sum / _count;
        }
        public double SmoothedVelocity
        {
            get
            {
                if (_count < 2)
                    return 0;

                // Compute means
                double sumT = 0;
                double sumR = 0;

                for (int i = 0; i < _count; i++)
                {
                    sumT += _buffer[i].Timestamp.Ticks;
                    sumR += _buffer[i].Rssi;
                }

                double meanT = sumT / _count;
                double meanR = sumR / _count;

                // Compute numerator and denominator
                double num = 0;
                double den = 0;

                for (int i = 0; i < _count; i++)
                {
                    double dt = _buffer[i].Timestamp.Ticks - meanT;
                    double dr = _buffer[i].Rssi - meanR;

                    num += dt * dr;
                    den += dt * dt;
                }

                if (den == 0)
                    return 0;

                // slope in RSSI per tick
                double slopePerTick = num / den;

                // convert ticks to seconds
                double slopePerSecond = slopePerTick * TimeSpan.TicksPerSecond;

                return slopePerSecond; // positive = closer, negative = farther
            }
        }
    }
    }
