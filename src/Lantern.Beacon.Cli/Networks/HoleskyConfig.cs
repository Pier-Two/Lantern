using Lantern.Beacon.Sync.Types;
using SszSharp;

namespace Lantern.Beacon.Cli.Networks;

public static class HoleskyConfig
{
    public static readonly NetworkType NetworkType = NetworkType.Holesky;
    public const ulong GenesisTime = 1695902400;
    public static readonly byte[] GenesisValidatorsRoot = Convert.FromHexString("9143aa7c615a7f7115e2b6aac319c03529df8242ae705fba9df39b79c59fa8b1");
    public static readonly SizePreset Preset = SizePreset.MainnetPreset;
}