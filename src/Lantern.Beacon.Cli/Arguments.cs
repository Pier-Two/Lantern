using Lantern.Beacon.Sync.Types;

namespace Lantern.Beacon.Console;

public class Arguments
{
    // SyncProtocolOptions properties
    public string Network { get; set; } = "mainnet"; 
    public string TrustedBlockRoot { get; set; } = "b170fd52257200a0bc86f896ee9b688e9022f93e70810aa90e779a7bc1683a7f";

    // BeaconClientOptions properties with default values
    public string DataDirectoryPath { get; set; } =
        Path.Combine(
            Environment.OSVersion.Platform == PlatformID.Win32NT
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lantern", "lantern.db")
                : Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? string.Empty, ".lantern", "lantern.db")
        );

    public int TargetPeerCount { get; set; } = 1;
    public int TargetNodesToFind { get; set; } = 100;
    public int MaxParallelDials { get; set; } = 1;
    public int DialTimeoutSeconds { get; set; } = 5;
    public int TcpPort { get; set; } = 9001;
    public bool GossipSubEnabled { get; set; } = true;
    public List<string> Bootnodes { get; set; } = new();
    public bool EnableDiscovery { get; set; } = true;

    /// <summary>
    /// Parses command-line arguments and returns an Arguments instance.
    /// </summary>
    /// <param name="args">Array of command-line arguments.</param>
    /// <returns>Populated Arguments instance.</returns>
    public static Arguments Parse(string[] args)
    {
        var arguments = new Arguments();
        var argsList = args.ToList();

        for (int i = 0; i < argsList.Count; i++)
        {
            var arg = argsList[i].ToLowerInvariant();
            switch (arg)
            {
                case "--network":
                    if (i + 1 < argsList.Count)
                    {
                        arguments.Network = argsList[++i];
                    }
                    else
                    {
                        throw new ArgumentException("Missing value for --network");
                    }
                    break;

                case "--block-root":
                    if (i + 1 < argsList.Count)
                    {
                        arguments.TrustedBlockRoot = argsList[++i];
                    }
                    else
                    {
                        throw new ArgumentException("Missing value for --block-root");
                    }
                    break;

                case "--datadir":
                    if (i + 1 < argsList.Count)
                    {
                        arguments.DataDirectoryPath = argsList[++i];
                    }
                    else
                    {
                        throw new ArgumentException("Missing value for --datadir");
                    }
                    break;

                case "--peer-count":
                    if (i + 1 < argsList.Count && int.TryParse(argsList[++i], out int peerCount))
                    {
                        arguments.TargetPeerCount = peerCount;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --peer-count");
                    }
                    break;

                case "--discovery-peer-count":
                    if (i + 1 < argsList.Count && int.TryParse(argsList[++i], out int discoveryPeerCount))
                    {
                        arguments.TargetNodesToFind = discoveryPeerCount;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --discovery-peer-count");
                    }
                    break;

                case "--dial-parallelism":
                    if (i + 1 < argsList.Count && int.TryParse(argsList[++i], out int dialParallelism))
                    {
                        arguments.MaxParallelDials = dialParallelism;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --dial-parallelism");
                    }
                    break;

                case "--dial-timeout":
                    if (i + 1 < argsList.Count && int.TryParse(argsList[++i], out int dialTimeout))
                    {
                        arguments.DialTimeoutSeconds = dialTimeout;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --dial-timeout");
                    }
                    break;

                case "--tcp-port":
                    if (i + 1 < argsList.Count && int.TryParse(argsList[++i], out int tcpPort))
                    {
                        arguments.TcpPort = tcpPort;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --tcp-port");
                    }
                    break;

                case "--gossip-sub-enabled":
                    if (i + 1 < argsList.Count && bool.TryParse(argsList[++i], out bool gossipEnabled))
                    {
                        arguments.GossipSubEnabled = gossipEnabled;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid or missing value for --gossip-sub-enabled");
                    }
                    break;

                case "--bootnodes":
                    // Collect all bootnodes until the next argument starting with --
                    while (i + 1 < argsList.Count && !argsList[i + 1].StartsWith("--"))
                    {
                        arguments.Bootnodes.Add(argsList[++i]);
                    }
                    break;

                case "--enable-discovery":
                    if (i + 1 < argsList.Count && bool.TryParse(argsList[++i], out bool enableDiscovery))
                    {
                        arguments.EnableDiscovery = enableDiscovery;
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

        return arguments;
    }

    /// <summary>
    /// Converts the Network string to the corresponding NetworkType enum.
    /// </summary>
    /// <returns>NetworkType enum value.</returns>
    public NetworkType GetNetworkType()
    {
        return Network.ToLower() switch
        {
            "mainnet" => NetworkType.Mainnet,
            "holesky" => NetworkType.Holesky,
            _ => throw new ArgumentException($"Unsupported network type: {Network}")
        };
    }

    /// <summary>
    /// Converts the TrustedBlockRoot hex string to a byte array.
    /// </summary>
    /// <returns>Byte array of TrustedBlockRoot.</returns>
    public byte[] GetTrustedBlockRootBytes()
    {
        try
        {
            return Convert.FromHexString(TrustedBlockRoot);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid hex string for --block-root");
        }
    }
}


