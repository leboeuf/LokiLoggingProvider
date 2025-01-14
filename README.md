# Loki Logging Provider

Send logs directly to [Grafana Loki](https://grafana.com/loki) from your .NET application. The Loki Logging Provider is a simple library that plugs into the logging framework provided in .NET.

For more information about logging, please checkout [_Logging in .NET_](https://docs.microsoft.com/dotnet/core/extensions/logging) and [_Logging in .NET Core and ASP.NET Core_](https://docs.microsoft.com/aspnet/core/fundamentals/logging).

## Usage

### Step 1. Install the Package

The Loki logging provider is available from [Nuget.org](https://www.nuget.org/packages/LokiLoggingProvider). Add it to your project with the command;

```sh
dotnet add package LokiLoggingProvider --version <version>
```

### Step 2. Add the Logging Provider

For host based apps, add the Loki logging provider by calling the `AddLoki()` extension method on the `ILoggingBuilder` instance when configuring your host.

```C#
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logBuilder =>
            {
                logBuilder.AddLoki();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
```

For non-host based apps, add the Loki logging provider by calling the `AddLoki()` extension method when creating your `ILoggerFactory`.

```C#
using Microsoft.Extensions.Logging;

public class Program
{
    public static void Main()
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddLoki();
        });

        ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("Hello from my Console App!");
    }
}
```

### Step 3. Configure Logging

By default, the Loki logging provider does nothing. You must specify the type of push client (`Grpc` or `Http`) to use before any logs will be sent to Loki. Configure the Loki logging provider by modifying `appsettings.json` or its variants. An example is below.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    },
    "Loki": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      },
      "Client": "Grpc",
      "Grpc": {
        "Address": "http://localhost:9095"
      },
      "StaticLabels": {
        "JobName": "Example.WebApp"
      }
    }
  }
}
```

For a complete list of all the options you can configure and their defaults, please checkout the [`LokiLoggerOptions`](./Sources/LokiLoggingProvider/Options/LokiLoggerOptions.cs) class and its related classes. The Loki section in `appsettings.json` basically binds to this class.

Alternatively, you can configure the Loki logging provider by passing an `Action` delegate that configures an instance of [`LokiLoggerOptions`](./Sources/LokiLoggingProvider/Options/LokiLoggerOptions.cs) to the `AddLoki()` extension method.

## Examples

Included in this repository are two example projects that use the Loki logging provider which you can use as a reference.

- [Example.ConsoleApp](./Sources/Example.ConsoleApp)
- [Example.WebApp](./Sources/Example.WebApp)

There is also [docker-compose.yml](./docker-compose.yml) file to help you spin up instances of Grafana and Loki in [Docker](https://www.docker.com) for local development purposes.
