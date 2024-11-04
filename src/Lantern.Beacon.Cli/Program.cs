using Lantern.Beacon.Sync;
using Lantern.Discv5.Enr;
using Lantern.Discv5.Enr.Entries;
using Lantern.Discv5.WireProtocol.Connection;
using Lantern.Discv5.WireProtocol.Session;
using Lantern.Discv5.WireProtocol.Table;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SszSharp;
using System;

namespace Lantern.Beacon.Console;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        // Parse command-line arguments
        BeaconClientOptions beaconClientOptions;

        try
        {
            beaconClientOptions = BeaconClientOptions.Parse(args);
        
            // Validate required arguments
            EnsureRequiredArguments(args);
        
        }
        catch (ArgumentException ex)
        {
            System.Console.WriteLine($"Error parsing arguments: {ex.Message}");
            DisplayUsage();
            return;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
            DisplayUsage();
            return;
        }

        var stopTokenSource = new CancellationTokenSource();
        var stopToken = stopTokenSource.Token;
        System.Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            System.Console.WriteLine("Ctrl+C pressed. Stopping the beacon client...");
            stopTokenSource.Cancel();
        };

        // Discv5 options
        var discoveryBootstrapEnrs = new[]
        {
            "enr:-Ku4QImhMc1z8yCiNJ1TyUxdcfNucje3BGwEHzodEZUan8PherEo4sF7pPHPSIB1NNuSg5fZy7qFsjmUKs2ea1Whi0EBh2F0dG5ldHOIAAAAAAAAAACEZXRoMpD1pf1CAAAAAP__________gmlkgnY0gmlwhBLf22SJc2VjcDI1NmsxoQOVphkDqal4QzPMksc5wnpuC3gvSC8AfbFOnZY_On34wIN1ZHCCIyg", 
            "enr:-Le4QPUXJS2BTORXxyx2Ia-9ae4YqA_JWX3ssj4E_J-3z1A-HmFGrU8BpvpqhNabayXeOZ2Nq_sbeDgtzMJpLLnXFgAChGV0aDKQtTA_KgEAAAAAIgEAAAAAAIJpZIJ2NIJpcISsaa0Zg2lwNpAkAIkHAAAAAPA8kv_-awoTiXNlY3AyNTZrMaEDHAD2JKYevx89W0CcFJFiskdcEzkH_Wdv9iW42qLK79ODdWRwgiMohHVkcDaCI4I"
        };
        var connectionOptions = new ConnectionOptions();
        var sessionOptions = SessionOptions.Default;
        var tableOptions = new TableOptions(discoveryBootstrapEnrs);
        var enr = new EnrBuilder()
            .WithIdentityScheme(sessionOptions.Verifier, sessionOptions.Signer)
            .WithEntry(EnrEntryKey.Id, new EntryId("v4"))
            .WithEntry(EnrEntryKey.Secp256K1, new EntrySecp256K1(sessionOptions.Signer.PublicKey));
        var discv5LoggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.None));
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Nethermind.Libp2p.Core.ChannelFactory", LogLevel.None)
                .SetMinimumLevel(LogLevel.Information)
                .AddProvider(new CustomConsoleLoggerProvider(
                    config => config.EventId == 0,
                    new CustomConsoleLogger.CustomConsoleLoggerConfiguration
                    {
                        EventId = 0,
                        TimestampPrefix = "[HH:mm:ss]"
                    }));
        });

        var services = new ServiceCollection();

        // Setup services
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
            // Graceful shutdown
            await beaconClient.StopAsync();
        }
        finally
        {
            System.Console.WriteLine("Beacon client stopped.");
        }
    }
    
    private static void EnsureRequiredArguments(string[] args)
    {
        var requiredArgs = new Dictionary<string, string>()
        {
            { "--network", "Network type (e.g., mainnet, holesky)" },
            { "--genesis-time", "Genesis time in UNIX timestamp for slot 0" },
            { "--genesis-validators-root", "Trusted genesis validators root in hex format" },
            { "--preset", "Preset type (e.g., mainnet, holesky)" }
        };

        foreach (var requiredArg in requiredArgs)
        {
            if (!args.Any(arg => arg.StartsWith(requiredArg.Key, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception($"Missing required argument: {requiredArg.Key} - {requiredArg.Value}");
            }
        }
    }

    private static void DisplayUsage()
    {
        System.Console.WriteLine("Usage: Lantern.Beacon.Console [options]");
        System.Console.WriteLine("Options:");
        System.Console.WriteLine("  --network <type>                 Network type (e.g., mainnet, holesky)");
        System.Console.WriteLine("  --genesis-time <timestamp>       Genesis time in UNIX timestamp for slot 0");
        System.Console.WriteLine("  --genesis-validators-root <hex>  Trusted genesis validators root in hex format");
        System.Console.WriteLine("  --preset <type>                  Preset type (e.g., mainnet, holesky)");
        System.Console.WriteLine("  --block-root <hex>               Trusted block root in hex format");
        System.Console.WriteLine("  --datadir <path>                 Data directory path (OPTIONAL)");
        System.Console.WriteLine("  --peer-count <number>            Target number of peers to connect (OPTIONAL)");
        System.Console.WriteLine("  --discovery-peer-count <number>  Target nodes to find for discovery (OPTIONAL)");
        System.Console.WriteLine("  --dial-parallelism <number>      Maximum parallel dials (OPTIONAL)");
        System.Console.WriteLine("  --dial-timeout <seconds>         Dial timeout in seconds (OPTIONAL)");
        System.Console.WriteLine("  --tcp-port <port>                TCP port to use (OPTIONAL)");
        System.Console.WriteLine("  --gossip-sub-enabled <true|false> Enable or disable GossipSub (OPTIONAL)");
        System.Console.WriteLine("  --bootnodes <libp2p-multiaddress1> <libp2p-multiaddress2> ...    List of bootnode Libp2p multiaddresses (OPTIONAL)");
        System.Console.WriteLine("  --enable-discovery <bool>  Enable or disable discovery (OPTIONAL)");
    }
}

