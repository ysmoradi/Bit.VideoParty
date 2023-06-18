using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Bit.VideoParty;

public partial class MainWindow
{
    const string vlcMediaUrl = "http://localhost:8080";
    const string serverUrl =
#if DEBUG
        "https://localhost:7036";
#else
        "https://bit-video-party.azurewebsites.net";
#endif
    const string vlcPassword = "P@ssw0rd";
    private readonly HubConnection connection;
    private readonly PeriodicTimer timer;

    public MainWindow()
    {
        InitializeComponent();

        Help.Text = @"Getting started:
1- Press `Ctrl + P` in VLC Player
2- In `Show Settings` choose `All`
3- Seach for `Lua` and select it in left pane
4- Set `Lua HTTP Password` as `P@ssw0rd` (Not Lua Telnet Password)
5- Select `Main Interfaces` in left pane and check the `web` checkbox
6- Restart VLC (These changes are required for the 1st time only)
6- Write group name down here and tap on Connect!
7- Press toggle and enjoy! ;D";

        connection = new HubConnectionBuilder()
                .WithUrl($"{serverUrl}/signalr/video-party")
                .Build();

        timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        connection.Closed += async (error) =>
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                Group.Foreground = Brushes.Red;

                while (await timer.WaitForNextTickAsync())
                {
                    if (connection.State is HubConnectionState.Connected)
                        break;

                    if (connection.State is HubConnectionState.Disconnected)
                    {
                        try
                        {
                            await DoConnect();
                        }
                        catch (Exception exp)
                        {
                            Clipboard.SetText(exp.ToString());
                        }
                    }
                }
            });
        };

        connection.On("Toggle", async () =>
        {
            await Dispatcher.InvokeAsync(DoToggle);
        });
    }

    private async void Connect_Click(object sender, RoutedEventArgs e)
    {
        await DoConnect();
    }

    private async Task DoConnect()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Group.Text))
                throw new InvalidOperationException("Group name may not be empty!");

            Group.IsReadOnly = true;

            Connect.IsEnabled = false;

            Group.Foreground = Brushes.Yellow;

            try
            {
                using CancellationTokenSource stopCts = new(TimeSpan.FromSeconds(10));

                await connection.StopAsync(stopCts.Token);
            }
            catch { }

            using CancellationTokenSource startCts = new(TimeSpan.FromSeconds(10));

            await connection.StartAsync(startCts.Token);

            await connection.InvokeAsync("AddToGroup", Group.Text);

            Group.Foreground = Brushes.Green;
        }
        catch
        {
            Group.Foreground = Brushes.Red;
            throw;
        }
        finally
        {
            Group.IsReadOnly = false;
            Connect.IsEnabled = true;
        }
    }

    private async void Toggle_Click(object sender, RoutedEventArgs e)
    {
        await DoToggle();

        using HttpClient client = new();
        (await client.PostAsync($"{serverUrl}/api/toggle?group={Group.Text}&senderConnectionId={connection.ConnectionId}", null)).EnsureSuccessStatusCode();
    }

    private async Task DoToggle()
    {
        try
        {
            Toggle.IsEnabled = false;

            using HttpClient client = new();
            byte[] password = Encoding.ASCII.GetBytes($":{vlcPassword}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(password));
            (await client.GetAsync($"{vlcMediaUrl}/requests/status.xml?command=pl_pause")).EnsureSuccessStatusCode();
        }
        finally
        {
            Toggle.IsEnabled = true;
        }
    }
}