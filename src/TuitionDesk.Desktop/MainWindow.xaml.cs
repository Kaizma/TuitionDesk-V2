using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using TuitionDesk.Infrastructure.Persistence;
using TuitionDesk.Server.Hosting;

namespace TuitionDesk.Desktop;

public partial class MainWindow : Window
{
    private readonly ServerInstance _server;

    public MainWindow(ServerInstance server)
    {
        _server = server;
        InitializeComponent();

        var workArea = SystemParameters.WorkArea;
        Width = Math.Clamp(workArea.Width - 48, MinWidth, 1280);
        Height = Math.Clamp(workArea.Height - 48, MinHeight, 800);

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        StartupDiagnostics.Write("MainWindow loaded.");
        try
        {
            var userDataFolder = DataPaths.WebViewDataDirectory(DataPaths.ResolveRoot());
            Directory.CreateDirectory(userDataFolder);
            StartupDiagnostics.Write($"WebView2 user data folder: {userDataFolder}.");

            var environment = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: userDataFolder,
#if DEBUG
                // Debug-only DevTools endpoint used for local UI verification.
                options: new CoreWebView2EnvironmentOptions("--remote-debugging-port=9222"));
#else
                options: null);
#endif
            StartupDiagnostics.Write($"WebView2 runtime: {environment.BrowserVersionString}.");
            await WebView.EnsureCoreWebView2Async(environment);
            StartupDiagnostics.Write("WebView2 initialised.");

            var core = WebView.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.IsStatusBarEnabled = false;
            core.Settings.AreBrowserAcceleratorKeysEnabled = false;
            core.Settings.IsZoomControlEnabled = false;
            core.Settings.IsPasswordAutosaveEnabled = false;
            core.Settings.IsGeneralAutofillEnabled = false;

#if DEBUG
            core.Settings.AreDevToolsEnabled = true;
#else
            core.Settings.AreDevToolsEnabled = false;
#endif

            var config = JsonSerializer.Serialize(new
            {
                apiBaseUrl = _server.Url.TrimEnd('/'),
                token = _server.Token,
                appVersion = typeof(MainWindow).Assembly.GetName().Version?.ToString() ?? "0.1.0",
            });

            await core.AddScriptToExecuteOnDocumentCreatedAsync(
                $"window.__TUITIONDESK__ = {config};");

            core.NewWindowRequested += (_, args) => args.Handled = true;
            core.NavigationStarting += (_, args) =>
            {
                if (!args.Uri.StartsWith(_server.Url, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(args.Uri, "about:blank", StringComparison.OrdinalIgnoreCase))
                {
                    args.Cancel = true;
                }
            };
            core.NavigationCompleted += async (_, args) =>
            {
                StartupDiagnostics.Write(
                    $"Navigation completed. Success={args.IsSuccess}, status={args.WebErrorStatus}.");

                if (args.IsSuccess)
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;

#if DEBUG
                    try
                    {
                        var probe = await core.ExecuteScriptAsync(
                            "JSON.stringify({ title: document.title, text: (document.body?.innerText ?? '').slice(0, 160) })");
                        StartupDiagnostics.Write($"UI probe: {probe}");
                    }
                    catch (Exception probeException)
                    {
                        StartupDiagnostics.Write($"UI probe failed: {probeException.Message}");
                    }
#endif
                }
                else
                {
                    ShowStartupError(
                        $"The interface failed to load (0x{args.WebErrorStatus:X}). Check the log folder and try again.");
                }
            };
            core.ProcessFailed += (_, args) =>
            {
                StartupDiagnostics.Write($"WebView2 process failed: {args.ProcessFailedKind}.");
                ShowStartupError(
                    $"The interface process stopped ({args.ProcessFailedKind}). Restart TuitionDesk.");
            };

            StartupDiagnostics.Write($"Navigating to {_server.Url}.");
            WebView.Source = new Uri(_server.Url);
        }
        catch (Exception exception)
        {
            StartupDiagnostics.Write($"WebView2 startup failed: {exception}");
            ShowStartupError(
                $"The interface could not be initialised.\n\n{exception.Message}\n\nLogs: {DataPaths.LogDirectory(DataPaths.ResolveRoot())}");
        }
    }

    private void ShowStartupError(string message)
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        LoadingStatus.Text = message;
    }
}
