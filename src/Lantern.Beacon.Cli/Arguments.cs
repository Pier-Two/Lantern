using Lantern.Beacon.Cli.Networks;
using Lantern.Beacon.Sync.Types;
using Microsoft.Extensions.Logging;
using SszSharp;

namespace Lantern.Beacon.Cli;

public static class Arguments
{
    public static BeaconClientOptions Parse(string[] args)
    {
        var argsList = args.ToList();
        var options = new BeaconClientOptions();
        var networkType = NetworkType.Mainnet;
        
        for (var i = 0; i < argsList.Count; i++)
        {
            var arg = argsList[i].ToLowerInvariant();

            if (arg != "--network")
                continue;

            if (i + 1 < argsList.Count)
            {
                networkType = GetNetworkType(argsList[++i]);
                options.SyncProtocolOptions.Network = networkType;
                
                switch (networkType)
                {
                    case NetworkType.Mainnet:
                        options.SyncProtocolOptions.GenesisTime = MainnetConfig.GenesisTime;
                        options.SyncProtocolOptions.GenesisValidatorsRoot = MainnetConfig.GenesisValidatorsRoot;
                        options.SyncProtocolOptions.Preset = MainnetConfig.Preset;
                        break;

                    case NetworkType.Holesky:
                        options.SyncProtocolOptions.GenesisTime = HoleskyConfig.GenesisTime;
                        options.SyncProtocolOptions.GenesisValidatorsRoot = HoleskyConfig.GenesisValidatorsRoot;
                        options.SyncProtocolOptions.Preset = HoleskyConfig.Preset;
                        break;

                    case NetworkType.Custom:
                        // For custom network, the user must provide genesis-time, genesis-validators-root, and preset
                        break;

                    default:
                        throw new ArgumentException($"Unsupported network type: {networkType}");
                }
            }
            else
            {
                throw new ArgumentException("Missing value for --network");
            }
            break;
        }
        
        for (var i = 0; i < argsList.Count; i++)
        {
            var arg = argsList[i].ToLowerInvariant();

            switch (arg)
            {
                case "--network":
                    i++;
                    break;

                case "--log-level":
                    if (i + 1 < args.Length)
                    {
                        var levelString = args[++i];
                        if (Enum.TryParse<LogLevel>(levelString, true, out var logLevel))
                        {
                            options.LogLevel = logLevel;
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid log level: {levelString}");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Missing value for --log-level");
                    }
                    break;

                case "--genesis-time":
                    if (networkType != NetworkType.Custom)
                    {
                        throw new ArgumentException("--genesis-time can only be used with --network custom");
                    }
                    if (i + 1 < argsList.Count && ulong.TryParse(argsList[++i], out var genesisTime))
                    {
                        options.SyncProtocolOptions.GenesisTime = genesisTime;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --genesis-time");
                    }
                    break;

                case "--genesis-validators-root":
                    if (networkType != NetworkType.Custom)
                    {
                        throw new ArgumentException("--genesis-validators-root can only be used with --network custom");
                    }
                    if (i + 1 < argsList.Count)
                    {
                        options.SyncProtocolOptions.GenesisValidatorsRoot = GetTrustedBlockRootBytes(argsList[++i]);
                    }
                    else
                    {
                        throw new ArgumentException("Missing value for --genesis-validators-root");
                    }
                    break;

                case "--preset":
                    if (networkType != NetworkType.Custom)
                    {
                        throw new ArgumentException("--preset can only be used with --network custom");
                    }
                    if (i + 1 < argsList.Count)
                    {
                        options.SyncProtocolOptions.Preset = GetPreset(argsList[++i]);
                    }
                    else
                    {
                        throw new ArgumentException("Missing value for --preset");
                    }
                    break;
                
                case "--block-root":
                    if (i + 1 < argsList.Count)
                    {
                        options.SyncProtocolOptions.TrustedBlockRoot = GetTrustedBlockRootBytes(argsList[++i]);
                    }
                    else
                    {
                        throw new ArgumentException("Missing value for --block-root");
                    }
                    break;

                case "--datadir":
                    if (i + 1 < argsList.Count)
                    {
                        var providedPath = argsList[++i];
                        options.DataDirectoryPath = Path.Combine(providedPath, "lantern", "lantern.db");
                    }
                    else
                    {
                        throw new ArgumentException("Missing value for --datadir");
                    }
                    break;

                case "--peer-count":
                    if (i + 1 < argsList.Count && int.TryParse(argsList[++i], out int peerCount))
                    {
                        options.TargetPeerCount = peerCount;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --peer-count");
                    }
                    break;

                case "--discovery-peer-count":
                    if (i + 1 < argsList.Count && int.TryParse(argsList[++i], out var discoveryPeerCount))
                    {
                        options.TargetNodesToFind = discoveryPeerCount;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --discovery-peer-count");
                    }
                    break;

                case "--dial-parallelism":
                    if (i + 1 < argsList.Count && int.TryParse(argsList[++i], out var dialParallelism))
                    {
                        options.MaxParallelDials = dialParallelism;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --dial-parallelism");
                    }
                    break;

                case "--dial-timeout":
                    if (i + 1 < argsList.Count && int.TryParse(argsList[++i], out var dialTimeout))
                    {
                        options.DialTimeoutSeconds = dialTimeout;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --dial-timeout");
                    }
                    break;

                case "--tcp-port":
                    if (i + 1 < argsList.Count && int.TryParse(argsList[++i], out var tcpPort))
                    {
                        options.TcpPort = tcpPort;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --tcp-port");
                    }
                    break;

                case "--http-port":
                    if (i + 1 < argsList.Count && int.TryParse(argsList[++i], out var httpPort))
                    {
                        options.HttpPort = httpPort;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --http-port");
                    }
                    break;

                case "--gossip-sub-enabled":
                    if (i + 1 < argsList.Count && bool.TryParse(argsList[++i], out var gossipEnabled))
                    {
                        options.GossipSubEnabled = gossipEnabled;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --gossip-sub-enabled");
                    }
                    break;

                case "--bootnodes":
                    while (i + 1 < argsList.Count && !argsList[i + 1].StartsWith("--"))
                    {
                        options.Bootnodes.Add(argsList[++i]);
                    }
                    break;

                case "--enable-discovery":
                    if (i + 1 < argsList.Count && bool.TryParse(argsList[++i], out var enableDiscovery))
                    {
                        options.EnableDiscovery = enableDiscovery;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --enable-discovery");
                    }
                    break;

                default:
                    throw new ArgumentException($"Unknown argument: {argsList[i]}");
            }
        }

        return options;
    }

    private static SizePreset GetPreset(string preset)
    {
        return preset.ToLower() switch
        {
            "mainnet" => MainnetConfig.Preset,
            "holesky" => HoleskyConfig.Preset,
            "custom" => SizePreset.MinimalPreset,
            _ => throw new ArgumentException($"Unsupported preset: {preset}")
        };
    }

    private static NetworkType GetNetworkType(string network)
    {
        return network.ToLower() switch
        {
            "mainnet" => MainnetConfig.NetworkType,
            "holesky" => HoleskyConfig.NetworkType,
            "custom" => NetworkType.Custom,
            _ => throw new ArgumentException($"Unsupported network type: {network}")
        };
    }

    private static byte[] GetTrustedBlockRootBytes(string trustedBlockRoot)
    {
        try
        {
            if (trustedBlockRoot.StartsWith("0x"))
            {
                trustedBlockRoot = trustedBlockRoot[2..];
            }

            return Convert.FromHexString(trustedBlockRoot);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid hex string for --block-root");
        }
    }
}