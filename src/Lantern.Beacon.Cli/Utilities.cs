namespace Lantern.Beacon.Cli;

public static class Utilities
{
    public static void EnsureRequiredArguments(string[] args)
    {
        var requiredArgs = new Dictionary<string, string>
        {
            { "--network", "Specifies the network type (e.g., mainnet, holesky)." },
            { "--genesis-time", "Sets the genesis time as a UNIX timestamp for slot 0." },
            { "--genesis-validators-root", "Trusted genesis validators root in hex format." },
            { "--preset", "Chooses the preset type (e.g., mainnet, holesky)." },
            { "--block-root", "Trusted Beacon block root in hex format" }
        };

        foreach (var requiredArg in requiredArgs.Where(requiredArg =>
                     !args.Any(arg => arg.StartsWith(requiredArg.Key, StringComparison.OrdinalIgnoreCase))))
        {
            throw new Exception($"Missing required argument: {requiredArg.Key} - {requiredArg.Value}");
        }
    }

    public static void DisplayUsage()
    {
        var options = new List<(string Option, string Description)>
        {
            ("--network <type>", "Specifies the network type (e.g., mainnet, holesky)."),
            ("--genesis-time <timestamp>", "Sets the genesis time as a UNIX timestamp for slot 0."),
            ("--genesis-validators-root <hex>", "Defines the trusted genesis validators root in hexadecimal format."),
            ("--preset <type>", "Sets the preset type (e.g., mainnet, holesky)."),
            ("--block-root <hex>", "Sets the trusted Beacon block root in hexadecimal format."),
            ("--datadir <path>", "Specifies the data directory path. (Optional)"),
            ("--peer-count <number>", "Sets the target number of peer connections for the client. (Optional)"),
            ("--discovery-peer-count <number>", "Sets the number of target nodes for discovery. (Optional)"),
            ("--dial-parallelism <number>", "Defines the maximum number of parallel dials. (Optional)"),
            ("--dial-timeout <seconds>", "Sets the dial timeout duration in seconds. (Optional)"),
            ("--tcp-port <port>", "Sets the TCP port for the client to use. (Optional)"),
            ("--http-port <port>", "Sets the HTTP port for the API exposed. (Optional)"),
            ("--gossip-sub-enabled <true|false>", "Enables or disables GossipSub. (Optional)"),
            ("--bootnodes <libp2p-multiaddress1> <libp2p-multiaddress2> ...", "Lists the bootnode Libp2p multiaddresses. (Optional)"),
            ("--enable-discovery <bool>", "Enables or disables discovery. (Optional)")
        };

        try
        {
            var maxOptionLength = options.Max(option => option.Option.Length);
            const int padding = 4;
            
            int terminalWidth;
            try
            {
                terminalWidth = Console.WindowWidth;
            }
            catch
            {
                terminalWidth = 80;
            }

            var descriptionStart = maxOptionLength + padding;

            Console.WriteLine("Usage: Lantern.Beacon.Console [options]");
            Console.WriteLine("Options:");

            foreach (var (option, description) in options)
            {
                var formattedDescription = description;
                
                if (descriptionStart + description.Length > terminalWidth)
                {
                    var maxLength = terminalWidth - descriptionStart - 3; 
                    
                    if (maxLength > 0)
                    {
                        formattedDescription = description[..maxLength] + "...";
                    }
                    else
                    {
                        formattedDescription = "...";
                    }
                }
                
                Console.WriteLine($"{option.PadRight(maxOptionLength + padding)}{formattedDescription}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An error occurred while displaying usage information: {ex.Message}");
        }
    }
}