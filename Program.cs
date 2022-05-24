using SimpleHostedCLI;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<ConsoleService>();
    })
    .Build();

await host.RunAsync();
