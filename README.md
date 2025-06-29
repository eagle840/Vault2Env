# KeyVaultDemo

KeyVaultDemo is a .NET 8 C\# console application designed to securely retrieve secrets from Azure Key Vault and manage Azure CLI authentication. It offers a convenient way to interact with your Azure Key Vault, fetch specific secrets, and execute common Azure CLI commands directly from the application.

-----

## Features

  * **Secure Secret Retrieval**: Fetches secrets from Azure Key Vault using `DefaultAzureCredential` for secure and flexible authentication (supports Azure CLI login, Managed Identities, etc.).
  * **Environment Variable Integration**: Automatically sets retrieved secrets as environment variables for the current process, making them accessible to other parts of your application or subsequent commands.
  * **Azure CLI Integration**: Provides shortcuts to perform `az login`, `az logout`, and `az account list` commands.
  * **Version Information**: Displays the application's version.
  * **Configuration Management**: Loads Key Vault name from `appsettings.json`.

-----

## Prerequisites

Before running this application, make sure you have the following installed:

  * **.NET 8 SDK**: Download and install the .NET 8 SDK from the official Microsoft website.
  * **Azure CLI**: Install the Azure Command-Line Interface (CLI).
  * **Azure Key Vault**: You need an existing Azure Key Vault with secrets configured.
  * **Permissions**: Your Azure identity (the one you'll use to log in via Azure CLI or a Managed Identity) must have **"Get Secret"** permissions on the Key Vault.

-----

## Setup

1.  **Clone the repository (or copy the code):**

    ```bash
    git clone https://github.com/your-repo/KeyVaultDemo.git
    cd KeyVaultDemo
    ```

2.  **Create `appsettings.json`:**
    In the root directory of the project, create a file named `appsettings.json` and add your Key Vault name:

    ```json
    {
      "KeyVault": {
        "VaultName": "YourKeyVaultName" 
      }
    }
    ```

    Replace `"YourKeyVaultName"` with the actual name of your Azure Key Vault.

3.  **Restore NuGet Packages:**

    ```bash
    dotnet restore
    ```

-----

## Usage

Run the application using `dotnet run` followed by the desired command or secret name.

### Commands

  * **Log in to Azure CLI:**

    ```bash
    dotnet run login
    ```

    This will open a browser window for you to log in to your Azure account.

  * **Log out from Azure CLI:**

    ```bash
    dotnet run logout
    ```

  * **List Azure Accounts:**

    ```bash
    dotnet run list
    ```

    This displays a table of your configured Azure subscriptions.

  * **Get application version:**

    ```bash
    dotnet run version
    # or
    dotnet run --version
    # or
    dotnet run -v
    ```

### Retrieving a Secret

To retrieve a secret, provide the secret name as a command-line argument:

```bash
dotnet run MySecretName
```

Replace `MySecretName` with the actual name of the secret stored in your Azure Key Vault.

**Example Output:**

```
Attempting to pull secret: MySecretName
Secret 'MySecretName' value retrieved from Key Vault:
MySuperSecretValue123!
Secret 'MySecretName' has been set as environment variable.
```

If you provide multiple arguments when trying to retrieve a secret, only the first argument will be used as the secret name.

-----

## Error Handling

The application includes basic error handling for Key Vault operations and Azure CLI commands. If a secret cannot be retrieved, it will provide helpful suggestions, such as checking the secret name, permissions, and Azure login status.

-----

## Contributing

Feel free to open issues or submit pull requests to improve this utility.

-----

## License

This project is open-source and available under the [MIT License](https://www.google.com/search?q=LICENSE).

## AI generated.

Initial AI generated Readme.
