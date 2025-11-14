# CleanArchitectureIdentityTemplate

## Introduction

This is a template solution for creating new .NET solutions that already use
a Clean Architecture structure. The template also contains ASP.NET Core Identity
scaffolding with roles. Along this, a service for authentication is included
with a REST API that has endpoints for login, register, and refresh.
The authentication is done with JWT.

## Requirements

- [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- SQL Server

## Usage

Example structure when done with the commands:

```
Projects\
    CleanArchitectureIdentityTemplate\
    CleanIdentityTest\
```

First you need to clone the repository with

```
git clone https://github.com/paavkar/CleanArchitectureIdentityTemplate.git
```

When you have the solution cloned, you need to install the template with the
command

```
dotnet new install .\CleanArchitectureIdentityTemplate\
```

You can check that the install was successful with the command

```
dotnet new list
```

You should see a new template with the name `Clean Architecture with Identity`.

After which you can create new solutions with

```
dotnet new clean-identity -n CleanIdentityTest --force
```

`-n` denotes the name for the solution (and folder). As is in the example
a new solution of name `CleanIdentityTest` was created with this command.
`--force` is required as the command needs to rename the namespace used.

## New solution user secrets

The program needs these values to be added to either user secrets
or app settings:

```
{
  "Kestrel:Certificates:Development:Password": "<GUID>",
  "Jwt:Key": "<256_BIT_VALUE>",
  "Jwt:Issuer": "<YOUR_APPLICATION_NAME>",
  "Jwt:Audience": "<YOUR_APPLICATION_NAME>",
  "ConnectionStrings:DefaultConnection": "<YOUR_DB_CONNECTION>"
}
```