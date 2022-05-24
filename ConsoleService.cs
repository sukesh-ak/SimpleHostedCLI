namespace SimpleHostedCLI;

public class ConsoleService : IHostedService
{
    private readonly ILogger<ConsoleService> _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private int? _exitCode;

    public ConsoleService(ILogger<ConsoleService> logger, IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _appLifetime = appLifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        AppInfo.Show();

        _appLifetime.ApplicationStarted.Register(async () =>
        {
            await Task.Run(async () =>
            {
                try
                {
                    await CreateCommandPrompt();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled Exception");
                }
                finally
                {
                    _appLifetime.StopApplication();
                }
            });
        });

        return Task.CompletedTask;
    }

    private async Task CreateCommandPrompt()
    {
        Console.WriteLine("Welcome to Interactive Simple CLI");
        string breadcrumps = ">";
        string? cmd = String.Empty;

        using (var cts = new CancellationTokenSource())
        {
            while (!cts.IsCancellationRequested)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{breadcrumps} ");
                Console.ResetColor();

                cmd = Console.ReadLine();
                ArgumentNullException.ThrowIfNull(cmd); // If null check

                switch (cmd.ToUpperInvariant())
                {
                    case "QUIT":
                        _exitCode = 0;
                        cts.Cancel(true);
                        break;
                    case "DOWNLOAD":
                        Console.Write("Downloading");
                        for (int i = 0; i < 10; i++)
                        {
                            Console.Write(".");
                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }
                        Console.WriteLine();
                        break;
                    case "COUNTDOWN":
                        for (int i = 0; i < 10; i++)
                        {
                            Console.WriteLine($"Countdown: {i}");
                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }
                        break;
                    default:
                        if (String.IsNullOrEmpty(cmd)) break;

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Invalid command: {cmd}");
                        Console.ResetColor();
                        break;
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Exiting with return code: {exitCode}", _exitCode);

        // Exit code may be null if the user cancelled via Ctrl+C/SIGTERM
        Environment.ExitCode = _exitCode.GetValueOrDefault(-1);
        return Task.CompletedTask;
    }
}
