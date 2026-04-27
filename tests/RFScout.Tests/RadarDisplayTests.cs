using Xunit;
using Spectre.Console;
using System.Collections.Concurrent;
using RFScout.Core.Domain;

namespace RFScout.Tests;

public class RadarDisplayTests
{
    [Fact]
    public async Task RunLiveSimulation()
    {
        AnsiConsole.Clear();
        var latestSignals = new ConcurrentDictionary<string, DeviceSignal>();
        // The file is copied to the output dir, so we can just reference it by name
        var scanner = new CsvMockScanner("stalker_sim.csv");

        scanner.DeviceFound += signal => latestSignals[signal.Address] = signal;
        scanner.Start();

        var table = new Table().RoundedBorder().Title("[bold blue] RFScout: Test Lab Radar[/]");
        table.AddColumn("Device");
        table.AddColumn("MAC Address");
        table.AddColumn(new TableColumn("RSSI").Centered());

        // Run the UI for 60 seconds or until you kill the process
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        await AnsiConsole.Live(table)
        .AutoClear(false) 
        .StartAsync(async ctx =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                AnsiConsole.Profile.Capabilities.Ansi = true; 
                AnsiConsole.Profile.Capabilities.Interactive = true;
                AnsiConsole.Clear();

                var sorted = latestSignals.Values.OrderByDescending(s => s.Rssi);

                foreach (var signal in sorted)
                {
                    string color = signal.Rssi > -60 ? "green" : (signal.Rssi > -80 ? "yellow" : "red");
                    table.AddRow(signal.Name, $"[dim]{signal.Address}[/]", $"[{color}]{signal.Rssi} dBm[/]");
                }

                ctx.Refresh();
                await Task.Delay(250);
            }
        });
    }
}