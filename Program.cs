using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace KeyVaultDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // If any arguments are provided, assume the user wants to perform an Azure CLI command.
            if (args.Length > 0)
            {
                var command = args[0].ToLower();
                if (command == "login")
                {
                    ExecuteAzCommand("az login");
                }
                else if (command == "logout")
                {
                    ExecuteAzCommand("az logout");
                }
                else if (command == "list")
                {
                    ExecuteAzCommand("az account list --output table");
                }
                else
                {
                    Console.WriteLine("Invalid argument. Use 'login', 'logout', or 'list'.");
                }
                return;
            }

            // If no command-line argument is provided, perform the Key Vault secret retrieval.
            var keyVaultUrl = "https://<VAULTNAME>.vault.azure.net/";
            var secretName = "mysecret1";

            // DefaultAzureCredential works by trying multiple credential providers.
            var credential = new DefaultAzureCredential();
            var client = new SecretClient(new Uri(keyVaultUrl), credential);

            try
            {
                KeyVaultSecret secret = await client.GetSecretAsync(secretName);
                Console.WriteLine("Secret value retrieved from Key Vault:");
                Console.WriteLine(secret.Value);

                // Store the secret in an environment variable for the current process.
                Environment.SetEnvironmentVariable(secretName, secret.Value);
                Console.WriteLine($"Secret has been set as environment variable: {secretName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while retrieving the secret:");
                Console.WriteLine(ex.Message);
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
            }
        }
    }
}
