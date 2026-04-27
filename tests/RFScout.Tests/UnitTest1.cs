using Xunit;
using Spectre.Console;
using RFScout.Tests;
using RFScout.Core.Domain;
using System.Collections.Concurrent;

namespace RFScout.Tests
{
    public class ManualDevTests
    {
        [Fact] // Change to [Fact(Skip = "Manual only")] later if you want
        public async Task Launch_Live_Radar_Simulation()
        {
            var latestSignals = new ConcurrentDictionary<string, DeviceSignal>();
            var scanner = new CsvMockScanner("stalker_sim.csv");

            scanner.DeviceFound += signal => latestSignals[signal.Address] = signal;
            scanner.Start();

            var table = new Table().RoundedBorder().Title("🛰️ [blue]RFScout: Test Lab[/]");
            table.AddColumn("Device");
            table.AddColumn("RSSI");

            // We use a timeout so the test eventually "passes" instead of running forever
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            await AnsiConsole.Live(table).StartAsync(async ctx =>
            {
                while (!cancellationToken.Token.IsCancellationRequested)
                {
                    table.Rows.Clear();
                    foreach (var signal in latestSignals.Values.OrderByDescending(s => s.Rssi))
                    {
                        table.AddRow(signal.Name, $"{signal.Rssi} dBm");
                    }
                    ctx.Refresh();
                    await Task.Delay(250);
                }
            });
        }
    }
}