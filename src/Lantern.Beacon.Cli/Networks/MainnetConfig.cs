using Lantern.Beacon.Sync.Types;
using SszSharp;

namespace Lantern.Beacon.Cli.Networks;

public static class MainnetConfig
{
    public static readonly NetworkType NetworkType = NetworkType.Mainnet;
    public const ulong GenesisTime = 1606824023;
    public static readonly byte[] GenesisValidatorsRoot = Convert.FromHexString("4b363db94e286120d76eb905340fdd4e54bfe9f06bf33ff6cf5ad27f511bfe95");
    public static readonly SizePreset Preset = SizePreset.MainnetPreset;
}