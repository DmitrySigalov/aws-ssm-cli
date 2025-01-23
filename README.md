# aws-ssm-client

[![Build](https://github.com/dmitrysigalov/aws-ssm-cli/workflows/Build/badge.svg)](https://github.com/dmitrysigalov/aws-ssm-cli/actions/workflows/build.yml)
[![License](https://badgen.net/github/license/dmitrysigalov/aws-ssm-cli)](https://github.com/dmitrysigalov/aws-ssm-cli/blob/main/LICENSE)

A dotnet open source which provides aws system manager using tool

## :gift: Features:
- Best practice for the environment variables names according to system parameters configured in AWS system manager store
- Configuration of ssm paths
- Synchronization of environment variables with system parameters values
- View current synchronization state of environment variables

## :sunny: .NET Runtime
This project is built with DotNet 8.0 and is mandatory to install before using.

You can find and install it [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

Verify your dotnet version:

![image](https://user-images.githubusercontent.com/31489258/153608978-cced639e-af42-4485-8c15-5333325b0883.png)

## :gift: Installation

The Installer publishes the code to the machine applications directory and adds it to your system's path.

- ### Windows
    - Compile and run the installer project as an Administrator while being in repo root directory:
```
dotnet run --project src/Aws.Ssm.Cli.Installer/Aws.Ssm.Cli.Installer.csproj
```

- ### macOS
    - Compile and run the installer project while being in repo root directory:
```
sudo dotnet run --project src/Aws.Ssm.Cli.Installer/Aws.Ssm.Cli.Installer.csproj
```

Open terminal / cmd and run:
```
aws-ssm-cli help
```
or
```
ascli help
```
If everything ran smoothly, you should see the list of supported commands

## :tada: Usage

The user should be authenticated (selected role) according to aws environment with access to AWS System Manager.
Recommended external tools:
- [okta-aws-cli](https://github.com/nizanrosh/okta-aws-cli)
- ...

Command line:
```cmd
aws-ssm-cli
```
FYI - The CLI can be executed using the commands `ascli` or `aws-ssm-cli`.

## :clipboard: Profile configuration

First time for the profile configuration recommended to run/select command:
```cmd
aws-ssm-cli config
```
- Set profile name - "default"
- Recommendation to set prefix for the environment variable(s) - "SSM_"
- Add ssm-path 
- You can export already valid profile configuration in the json format (example):
```json
{
  "EnvironmentVariablePrefix": "SSM_",
  "SsmPaths": [
    "/demo1",
    "/demo2"
  ]
}
```
- Complete/exit configuration

## :books: Commands using

### `set-env`
Using to synchronize environment variables with SSM parameters.
#### macOS
For the activation of environment variables required to recreate a process (terminal, Rider, ...)

### `get-env`
Using to view current synchronization status of the environment with SSM parameters

### `view`
Using for the easy configuration of infrastructure parameters (mapping ssm parameters to and environment variable names)



## License

This project is licensed under the [MIT License](https://github.com/dmitrysigalov/aws-ssm-cli/blob/main/LICENSE).
