using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.IO; // Added for Directory.GetCurrentDirectory() and File.Exists()

namespace KeyVaultDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Determine the base path for configuration
            string configFilePath = "";
            string currentDirectory = Directory.GetCurrentDirectory();
            string userProfileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // Prioritize appsettings.json in the current working directory
            if (File.Exists(Path.Combine(currentDirectory, "appsettings.json")))
            {
                configFilePath = Path.Combine(currentDirectory, "appsettings.json");
                Console.WriteLine($"Using appsettings.json from current directory: {configFilePath}");
            }
            else
            {
                configFilePath = Path.Combine(userProfileDirectory, "appsettings.json");
                Console.WriteLine($"Using appsettings.json from user profile directory: {configFilePath}");
            }

            // Build configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(configFilePath)) // Set base path to where the found appsettings.json is
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var vaultName = config["KeyVault:VaultName"];
            if (string.IsNullOrWhiteSpace(vaultName))
            {
                Console.WriteLine("Vault name not found in configuration. Please ensure 'KeyVault:VaultName' is set in appsettings.json.");
                return;
            }
            var keyVaultUrl = $"https://{vaultName}.vault.azure.net/";

            // Handle command-line arguments
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  dotnet run login|logout|list|list-secrets|version");
                Console.WriteLine("  dotnet run <secretName>");
                return; // Exit if no arguments are provided.
            }

            var firstArg = args[0].ToLower();

            // DefaultAzureCredential works by trying multiple credential providers.
            DefaultAzureCredential credential;
            try
            {
                credential = new DefaultAzureCredential();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Azure credentials: {ex.Message}");
                Console.WriteLine("Please ensure you are logged into Azure (e.g., via 'az login').");
                return;
            }

            var client = new SecretClient(new Uri(keyVaultUrl), credential);


            // Check if the argument is an Azure CLI command or a new internal command
            if (firstArg == "login")
            {
                ExecuteAzCommand("az login");
                return;
            }
            else if (firstArg == "logout")
            {
                ExecuteAzCommand("az logout");
                return;
            }
            else if (firstArg == "list")
            {
                ExecuteAzCommand("az account list --output table");
                return;
            }
            else if (firstArg == "list-secrets")
            {
                await ListAllSecrets(client, vaultName);
                return;
            }
            else if (firstArg == "version" || firstArg == "--version" || firstArg == "-v")
            {
                PrintVersionInfo();
                return;
            }

            // If the argument is not an Azure CLI command or internal command, assume it's a secret name.
            if (args.Length > 1)
            {
                Console.WriteLine("Warning: Only the first argument is used as a secret name. Additional arguments will be ignored.");
            }

            Console.WriteLine($"Attempting to pull secret: {args[0]}");
            var secretName = args[0]; // Use the provided argument as the secret name

            try
            {
                KeyVaultSecret secret = await client.GetSecretAsync(secretName);
                Console.WriteLine($"Secret '{secretName}' value retrieved from Key Vault:");
                Console.WriteLine(secret.Value);

                // Store the secret in an environment variable for the current process.
                Environment.SetEnvironmentVariable(secretName, secret.Value);
                Console.WriteLine($"Secret '{secretName}' has been set as environment variable.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the secret '{secretName}':");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Please ensure:");
                Console.WriteLine("1. The secret name is correct.");
                Console.WriteLine("2. You have 'Get Secret' permissions on the Key Vault.");
                Console.WriteLine("3. You are logged into Azure (e.g., via 'az login').");
            }
        }

        /// <summary>
        /// Executes an Azure CLI command in the system shell.
        /// </summary>
        /// <param name="command">The Azure CLI command to execute.</param>
        static void ExecuteAzCommand(string command)
        {
            try
            {
                // Determine the shell based on the OS platform.
                string shell, shellArgs;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    shell = "cmd.exe";
                    shellArgs = $"/c {command}";
                }
                else
                {
                    shell = "/bin/bash";
                    shellArgs = $"-c \"{command}\"";
                }

                var psi = new ProcessStartInfo
                {
                    FileName = shell,
                    Arguments = shellArgs,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Console.WriteLine($"Executing command: {command}");
                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    Console.WriteLine(output);
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        Console.WriteLine("Error: " + error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to execute command:");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Please ensure Azure CLI is installed and accessible in your system's PATH.");
            }
        }

        /// <summary>
        /// Prints the application version information.
        /// </summary>
        static void PrintVersionInfo()
        {
            // Fetch version from assembly
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "unknown";
            Console.WriteLine($"KeyVaultDemo Version: {version}");
        }

        /// <summary>
        /// Lists all secret names in the specified Azure Key Vault.
        /// </summary>
        /// <param name="client">The SecretClient instance for the Key Vault.</param>
        /// <param name="vaultName">The name of the Key Vault.</param>
        static async Task ListAllSecrets(SecretClient client, string vaultName)
        {
            Console.WriteLine($"Listing all secret names in Key Vault: {vaultName}");
            try
            {
                await foreach (SecretProperties secretProperties in client.GetPropertiesOfSecretsAsync())
                {
                    Console.WriteLine($"- {secretProperties.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while listing secrets in Key Vault '{vaultName}':");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Please ensure:");
                Console.WriteLine("1. The Key Vault name is correct in appsettings.json.");
                Console.WriteLine("2. You have 'List Secrets' permissions on the Key Vault.");
                Console.WriteLine("3. You are logged into Azure (e.g., via 'az login').");
            }
        }
    }
}
