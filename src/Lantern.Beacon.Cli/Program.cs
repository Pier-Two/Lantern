using Lantern.Beacon.Cli.Logging;
using Lantern.Beacon.Sync;
using Lantern.Discv5.Enr;
using Lantern.Discv5.Enr.Entries;
using Lantern.Discv5.WireProtocol.Connection;
using Lantern.Discv5.WireProtocol.Session;
using Lantern.Discv5.WireProtocol.Table;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lantern.Beacon.Cli;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        BeaconClientOptions beaconClientOptions;

        try
        {
            beaconClientOptions = Arguments.Parse(args);
            Utilities.EnsureRequiredArguments(args);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error parsing arguments: {ex.Message}");
            Utilities.DisplayUsage();
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Utilities.DisplayUsage();
            return;
        }

        var stopTokenSource = new CancellationTokenSource();
        var stopToken = stopTokenSource.Token;
        
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("Ctrl+C pressed. Stopping Lantern...");
            stopTokenSource.Cancel();
        };
        
        var discoveryBootnodes = new[]
        {
            "enr:-Ku4QHqVeJ8PPICcWk1vSn_XcSkjOkNiTg6Fmii5j6vUQgvzMc9L1goFnLKgXqBJspJjIsB91LTOleFmyWWrFVATGngBh2F0dG5ldHOIAAAAAAAAAACEZXRoMpC1MD8qAAAAAP__________gmlkgnY0gmlwhAMRHkWJc2VjcDI1NmsxoQKLVXFOhp2uX6jeT0DvvDpPcU8FWMjQdR4wMuORMhpX24N1ZHCCIyg",
            "enr:-Ku4QG-2_Md3sZIAUebGYT6g0SMskIml77l6yR-M_JXc-UdNHCmHQeOiMLbylPejyJsdAPsTHJyjJB2sYGDLe0dn8uYBh2F0dG5ldHOIAAAAAAAAAACEZXRoMpC1MD8qAAAAAP__________gmlkgnY0gmlwhBLY-NyJc2VjcDI1NmsxoQORcM6e19T1T9gi7jxEZjk_sjVLGFscUNqAY9obgZaxbIN1ZHCCIyg",
            "enr:-Ku4QImhMc1z8yCiNJ1TyUxdcfNucje3BGwEHzodEZUan8PherEo4sF7pPHPSIB1NNuSg5fZy7qFsjmUKs2ea1Whi0EBh2F0dG5ldHOIAAAAAAAAAACEZXRoMpD1pf1CAAAAAP__________gmlkgnY0gmlwhBLf22SJc2VjcDI1NmsxoQOVphkDqal4QzPMksc5wnpuC3gvSC8AfbFOnZY_On34wIN1ZHCCIyg", 
            "enr:-Ku4QEWzdnVtXc2Q0ZVigfCGggOVB2Vc1ZCPEc6j21NIFLODSJbvNaef1g4PxhPwl_3kax86YPheFUSLXPRs98vvYsoBh2F0dG5ldHOIAAAAAAAAAACEZXRoMpC1MD8qAAAAAP__________gmlkgnY0gmlwhDZBrP2Jc2VjcDI1NmsxoQM6jr8Rb1ktLEsVcKAPa08wCsKUmvoQ8khiOl_SLozf9IN1ZHCCIyg",
            "enr:-Le4QPUXJS2BTORXxyx2Ia-9ae4YqA_JWX3ssj4E_J-3z1A-HmFGrU8BpvpqhNabayXeOZ2Nq_sbeDgtzMJpLLnXFgAChGV0aDKQtTA_KgEAAAAAIgEAAAAAAIJpZIJ2NIJpcISsaa0Zg2lwNpAkAIkHAAAAAPA8kv_-awoTiXNlY3AyNTZrMaEDHAD2JKYevx89W0CcFJFiskdcEzkH_Wdv9iW42qLK79ODdWRwgiMohHVkcDaCI4I"
        };
        var connectionOptions = new ConnectionOptions();
        var sessionOptions = SessionOptions.Default;
        var tableOptions = new TableOptions(discoveryBootnodes);
        var enr = new EnrBuilder()
            .WithIdentityScheme(sessionOptions.Verifier, sessionOptions.Signer)
            .WithEntry(EnrEntryKey.Id, new EntryId("v4"))
            .WithEntry(EnrEntryKey.Secp256K1, new EntrySecp256K1(sessionOptions.Signer.PublicKey));
        var discv5LoggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.None));
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Nethermind.Libp2p.Core.ChannelFactory", LogLevel.None)
                .SetMinimumLevel(beaconClientOptions.LogLevel)
                .AddProvider(new CustomConsoleLoggerProvider(
                    config => config.EventId == 0,
                    new CustomConsoleLogger.CustomConsoleLoggerConfiguration
                    {
                        EventId = 0,
                        TimestampPrefix = "[HH:mm:ss]"
                    }));
        });

        var services = new ServiceCollection();
        services.AddBeaconClient(beaconClientBuilder =>
        {
            beaconClientBuilder.AddDiscoveryProtocol(discv5Builder =>
            {
                discv5Builder
                    .WithConnectionOptions(connectionOptions)
                    .WithTableOptions(tableOptions)
                    .WithEnrBuilder(enr)
                    .WithSessionOptions(sessionOptions)
                    .WithLoggerFactory(discv5LoggerFactory);
            });
            beaconClientBuilder.WithBeaconClientOptions(beaconClientOptions);
            beaconClientBuilder.AddLibp2pProtocol(libp2PBuilder => libp2PBuilder);
            beaconClientBuilder.WithLoggerFactory(loggerFactory);
        });

        var serviceProvider = services.BuildServiceProvider();
        var beaconClient = serviceProvider.GetRequiredService<IBeaconClient>();
   
        try
        {
            await beaconClient.InitAsync();
            await beaconClient.StartAsync(stopToken);
            
            await Task.Delay(-1, stopToken);
        }
        catch (OperationCanceledException)
        {
            await beaconClient.StopAsync();
        }
        finally
        {
            Console.WriteLine("Lantern has been stopped.");
        }
    }
}

