# CleanArchitectureIdentityTemplate

## Introduction

This is a template solution for creating new .NET solutions that already use
a Clean Architecture structure. The template also contains ASP.NET Core Identity
scaffolding with roles. Along this, a service for authentication is included
with a REST API that has endpoints for login, register, and refresh. Support
for setting up 2FA for user accounts is also present.
The authentication is done with JWT.

## Requirements

- [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- SQL Server

## Usage

You can install the template using the .nupkg file.
Use the command

```
dotnet new install .\PK.CleanArchitecture.Identity.1.1.0.nupkg
```

You can check that the install was successful with the command

```
dotnet new list
```

You should see a new template with the name `Clean Architecture with Identity`.

After which you can create new projects with the CLI

```
dotnet new clean-identity -n CleanIdentityTest --force
```

`-n` denotes the name for the solution (and folder). As is in the example
a new solution of name `CleanIdentityTest` was created with this command.
`--force` is required as the command needs to rename the namespace used.

or via you IDE of choice.

Using the CLI, you need to also generate a new .sln file in the project root with
the following command

```
dotnet new sln --name CleanIdentityTest
```

The `--name` command takes the name of your project (in this example CleanIdentityTest).
This creates a new .sln file which you need to migrate to .slnx file with the command

```
dotnet sln .\CleanIdentityTest.sln migrate
```

Again, use your project's solution name here.
You can remove the old .sln file with

`rm .\CleanIdentityTest.sln`

After this, you need to add the projects to the solution. Running the command

```
dotnet sln add .\CleanIdentityTest.WebAPI\CleanIdentityTest.WebAPI.csproj
```

should add all the projects to the solution. Again, use your project name here
in place of `CleanIdentityTest`.

You should now have a working solution with all the projects added.

## New project user secrets

The program needs these values to be added to either user secrets
or app settings:

```
{
  "Kestrel:Certificates:Development:Password": "<GUID>",
  "Jwt:Key": "<256_BIT_VALUE>",
  "Jwt:Issuer": "<YOUR_APPLICATION_NAME>",
  "Jwt:Audience": "<YOUR_APPLICATION_NAME>",
  "ConnectionStrings:DefaultConnection": "<YOUR_DB_CONNECTION>",
  "EncryptionKeys": {
  "Current": "<CURRENT_KEY_ID>",
  "Values": {
    "KEY_ID": "<32-CHAR-LONG-VALUE>"
  }
}
}
```

## Updating the database

When you have set up the project with the required secret values, you need to update the
database. This can be done with VS Package Manager Console or with dotnet ef CLI.

Following commands use the example project name of `CleanIdentityTest`, replace that with
your chosen project name.

### VS Package Manager Console

Using the VS Package Manager Console, the following command to create a new migration is

`Add-Migration InitialCreate -Project CleanIdentityTest.Infrastructure -StartupProject CleanIdentityTest.WebAPI`

The migration name using VS PMC is the first argument after `Add-Migration`.

and then the command that updates the database structure

`Update-Database -Project CleanIdentityTest.Infrastructure -StartupProject CleanIdentityTest.WebAPI`

### dotnet ef CLI

When using the dotnet ef CLI, the command to create migrations is

`dotnet ef migrations add InitialCreate --project CleanIdentityTest.Infrastructure --startup-project CleanIdentityTest.WebAPI`

The migration name using the dotnet ef CLI is the argument after `dotnet ef migrations add`.

and to update the database with this migration

`dotnet ef database update --project CleanIdentityTest.Infrastructure --startup-project CleanIdentityTest.WebAPI`
