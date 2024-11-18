# Lantern

Lantern is a light client for Ethereum that can track its Proof of Stake consensus in a fully peer-to-peer manner. This project has received funding from the Ethereum Foundation for improving censorship resistance and decentralising access to Ethereum.

## Prerequisites

Before you can build and run `Lantern.Beacon.Cli`, ensure that you have the following installed on your system:

1. **.NET SDK (version 8.0)**  
   The project is built using .NET, so you will need the .NET SDK installed. You can download it from the official [.NET website](https://dotnet.microsoft.com/download).

   To verify that .NET is installed, run the following command:
   ```bash
   dotnet --version
   ```

2. **Git**  
   Git is required to clone the repository. You can download Git from the official [Git website](https://git-scm.com/).

   To verify that Git is installed, run the following command:
   ```bash
   git --version
   ```

## Build From Source

To build this project from source, follow these detailed steps:

1. **Ensure Prerequisites are Met**  
   Install the required dependencies:  
   - [Download and install .NET SDK](https://dotnet.microsoft.com/download) (version 8.0 or higher).  
   - Install [Git](https://git-scm.com/) to manage the repository.

2. **Clone the Repository**  
   Start by cloning the repository into a local directory:  
   ```bash
   git clone https://github.com/Pier-Two/Lantern.git --recursive
   ```

3. **Navigate to Source Directory**  
   Navigate into the root directory of the project:
   ```bash
   cd Lantern.Beacon.Cli
   ```

4. **Build the Project**  
   You can use the .NET CLI to restore dependencies and build the project:
   ```bash
   dotnet restore
   dotnet build
   ```

5. **Run the Application**  
   Once the build completes successfully, you can run the CLI using the following command:
   ```bash
   cd src/Lantern.Beacon.Cli
   dotnet run -- <your-cli-arguments>
   ```
   Replace `<your-cli-arguments>` with the desired options for running the client (e.g. `--network mainnet`, etc.).

## Example Usage

### Running the Beacon Client

To run the CLI, you need to provide several required arguments such as the network and a recent block root from the Beacon chain. Below is an example of how to run the client for the Ethereum mainnet:

```
dotnet run -- --network mainnet --block-root 0xb170fd52257200a0bc86f896ee9b688e9022f93e70810aa90e779a7bc1683a7f
```

## Releases

You can find pre-built releases of `Lantern.Beacon.Cli` on the [Releases page](https://github.com/Pier-Two/Lantern.Beacon.Cli/releases). Each release is tagged and includes a changelog describing the updates, bug fixes, and new features included. 

## Contributing

We welcome contributions to `Lantern.Beacon.Cli`! If you'd like to contribute, please follow these steps:

1. Fork the repository.
2. Create a feature branch (`git checkout -b your-feature-name`).
3. Commit your changes (`git commit -m 'Add a new feature'`).
4. Push the branch (`git push origin your-feature-name`).
5. Create a pull request on the main repository.


## License

`Lantern.Beacon.Cli` is licensed under the MIT License. See the [LICENSE](./LICENSE) file for more details.

## Early Version Disclaimer

This is an early version of `Lantern.Beacon.Cli`. While it is functional, there may still be bugs, missing features, or incomplete documentation. We are actively working on improving the tool, and we welcome feedback and contributions from the community to help make it better.
