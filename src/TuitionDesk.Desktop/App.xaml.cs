using System.IO;
using System.Threading;
using System.Windows;
using TuitionDesk.Infrastructure.Persistence;
using TuitionDesk.Server.Hosting;

namespace TuitionDesk.Desktop;

public partial class App : System.Windows.Application
{
    private Mutex? _singleInstance;
    private ServerInstance? _server;

    public App()
    {
        StartupDiagnostics.Write("App constructed.");
        DispatcherUnhandledException += (_, args) =>
        {
            StartupDiagnostics.Write($"Dispatcher unhandled exception: {args.Exception}");
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            StartupDiagnostics.Write($"AppDomain unhandled exception: {args.ExceptionObject}");
        };
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        StartupDiagnostics.Write($"OnStartup begin. ShutdownMode={ShutdownMode}.");

        _singleInstance = new Mutex(initiallyOwned: true, @"Local\TuitionDesk", out var createdNew);
        if (!createdNew)
        {
            StartupDiagnostics.Write("Another instance is already running.");
            MessageBox.Show(
                "TuitionDesk is already running.",
                "TuitionDesk",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        try
        {
            var dataDirectory = DataPaths.ResolveRoot();
            StartupDiagnostics.Write($"Data directory: {dataDirectory}.");
            Directory.CreateDirectory(dataDirectory);

            _server = await ServerHost.StartAsync(new ServerOptions
            {
                DataDirectory = dataDirectory,
                Urls = ["http://127.0.0.1:0"],
                RequireToken = true,
                ContentRootPath = AppContext.BaseDirectory,
                ServeSpa = true,
                EnvironmentName = "Production",
            });

            StartupDiagnostics.Write($"Server started at {_server.Url}.");

            var window = new MainWindow(_server);
            window.Closed += (_, _) =>
            {
                StartupDiagnostics.Write("Main window closed; shutting down.");
                Shutdown();
            };
            MainWindow = window;
            window.Show();
            StartupDiagnostics.Write("Main window shown.");
        }
        catch (Exception exception)
        {
            StartupDiagnostics.Write($"Startup failed: {exception}");
            MessageBox.Show(
                $"TuitionDesk could not start.\n\n{exception.Message}\n\nLogs: {DataPaths.LogDirectory(DataPaths.ResolveRoot())}",
                "TuitionDesk",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        StartupDiagnostics.Write($"OnExit begin. ExitCode={e.ApplicationExitCode}.");
        if (_server is not null)
        {
            try
            {
                Task.Run(() => _server.DisposeAsync().AsTask()).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                // The process is exiting; nothing useful to do with a shutdown failure.
            }
        }

        if (_singleInstance is not null)
        {
            try
            {
                _singleInstance.ReleaseMutex();
            }
            catch (ApplicationException)
            {
                // Mutex was not owned by this thread.
            }

            _singleInstance.Dispose();
        }

        StartupDiagnostics.Write("OnExit complete.");
        base.OnExit(e);
    }
}
