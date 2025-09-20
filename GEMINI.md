# GEMINI.md

## Project Overview

This project, `Vault2Env`, is a .NET 8 console application named `v2e`. Its primary purpose is to securely retrieve secrets from Azure Key Vault and set them as environment variables for the current process. It also provides a convenient wrapper around common Azure CLI authentication commands. The application is configured via an `appsettings.json` file.

The core technologies used are:
- .NET 8
- C#
- Azure Identity SDK (`Azure.Identity`)
- Azure Key Vault Secrets SDK (`Azure.Security.KeyVault.Secrets`)

## Building and Running

### Prerequisites

- .NET 8 SDK
- Azure CLI

### Configuration

The application requires an `appsettings.json` file in either the current working directory or the user's profile directory. This file should contain the name of the Azure Key Vault to use.

**Example `appsettings.json`:**
```json
{
  "KeyVault": {
    "VaultName": "YourKeyVaultName"
  }
}
```

### Building the Project

To build the application, run the following command from the project root:

```sh
dotnet build --configuration Release
```

### Publishing the Project

To create a self-contained, single-file executable for Windows or Linux, use the `dotnet publish` command. The output will be placed in the `publish_output` directory.

**For Windows (x64):**
```sh
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish_output
```

**For Linux (x64):**
```sh
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish_output
```

The executable will be named `v2e.exe` on Windows and `v2e` on Linux.

### Running the Application

You can run the application using `dotnet run` or by executing the published binary.

**Using `dotnet run`:**

- **Login to Azure:** `dotnet run login`
- **Logout from Azure:** `dotnet run logout`
- **List Azure accounts:** `dotnet run list`
- **List secrets in the vault:** `dotnet run list-secrets`
- **Get application version:** `dotnet run version`
- **Retrieve a secret:** `dotnet run <secret-name>`

**Using the published executable:**

Replace `dotnet run` with the path to the executable (e.g., `./publish_output/v2e`).

- **Login to Azure:** `v2e login`
- **Retrieve a secret:** `v2e <secret-name>`

## Development Conventions

- **Configuration:** Application configuration is handled by `appsettings.json`. The application looks for this file first in the current working directory, and then in the user's profile directory.
- **Command-Line Interface:** The application uses a simple command-line argument parser to determine the action to perform. The first argument is treated as the command.
- **Error Handling:** `try/catch` blocks are used to handle potential exceptions during Azure SDK operations, providing user-friendly error messages.
- **Asynchronous Operations:** The application uses `async/await` for non-blocking I/O operations when interacting with Azure Key Vault.
- **Cross-Platform:** The application is designed to be cross-platform, with logic to execute shell commands on both Windows and Unix-like systems.
- **CI/CD:** GitHub Actions are used for building and publishing the application for both Windows and Linux. The workflows are defined in `.github/workflows/`.
