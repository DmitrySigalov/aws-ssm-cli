# aws-ssm-client

[![Build](https://github.com/dmitrysigalov/aws-ssm-cli/workflows/Build/badge.svg)](https://github.com/dmitrysigalov/aws-ssm-cli/actions/workflows/build.yml)
[![License](https://badgen.net/github/license/dmitrysigalov/aws-ssm-cli)](https://github.com/dmitrysigalov/aws-ssm-cli/blob/main/LICENSE)

A dotnet open source which provides aws system manager using tool

## :sunny: .NET Runtime
This project is built with DotNet 6.0 and is mandatory to install before using.

You can find and install it [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

Verify your dotnet version:

![image](https://user-images.githubusercontent.com/31489258/153608978-cced639e-af42-4485-8c15-5333325b0883.png)

## :gift: Installation

The Installer publishes the code to the machine applications directory and adds it to your system's path.

The installer can be found at the root folder under its own directory.

- ### Windows
    - Run Installer.exe

- ### macOS
    - It will be easier to run the installer correctly with the following command, while in its directory:
```
dotnet Installer.dll
```
or 
```
sudo dotnet Installer.dll
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
External tools:
- [okta-aws-cli](https://github.com/nizanrosh/okta-aws-cli)
- ...

```cmd
aws-ssm-cli
```
Select one of commands

```cmd
aws-ssm-cli <command>
```

FYI - The CLI can be executed using the commands `oacli` or `okta-aws-cli`.

## :clipboard: Profile configurations

First time for the profile configuration recommended to run/select command:
```cmd
aws-ssm-cli config
```
- Profile name - "default"
- Set prefix for the environment variable(s) - "SSM_"
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

## License

This project is licensed under the [MIT License](https://github.com/dmitrysigalov/aws-ssm-cli/blob/main/LICENSE).
