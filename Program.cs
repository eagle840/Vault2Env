using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace KeyVaultDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Load configuration from appsettings.json
            var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var config = new ConfigurationBuilder()
                .SetBasePath(homeDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var vaultName = config["KeyVault:VaultName"];
            if (string.IsNullOrWhiteSpace(vaultName))
            {
                Console.WriteLine("Vault name not found in configuration.");
                return;
            }
            var keyVaultUrl = $"https://{vaultName}.vault.azure.net/";

            // Handle command-line arguments
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  dotnet run login|logout|list");
                Console.WriteLine("  dotnet run <secretName>");
                return; // Exit if no arguments are provided.
            }

            // At this point, args.Length is greater than 0.
            var firstArg = args[0].ToLower();

            // Check if the argument is an Azure CLI command
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
            else if (firstArg == "version" || firstArg == "--version" || firstArg == "-v")
            {
                PrintVersionInfo();
                return;
            }

            // If the argument is not an Azure CLI command, assume it's a secret name.
            // If more than one argument is provided, issue a warning but proceed with the first one.
            if (args.Length > 1)
            {
                Console.WriteLine("Warning: Only the first argument is used as a secret name. Additional arguments will be ignored.");
            }

            Console.WriteLine($"Attempting to pull secret: {args[0]}");
            var secretName = args[0]; // Use the provided argument as the secret name

            // DefaultAzureCredential works by trying multiple credential providers.
            var credential = new DefaultAzureCredential();
            var client = new SecretClient(new Uri(keyVaultUrl), credential);

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

        static void PrintVersionInfo()
        {
            // Fetch version from assembly
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "unknown";
            Console.WriteLine($"KeyVaultDemo Version: {version}");
        }
    }
}
