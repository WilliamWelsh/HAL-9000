using System.Text;
using Discord.Rest;
using System.Timers;
using System.Threading.Tasks;

namespace Gideon
{
    class PewdsVsTSeriesWatcher
    {
        private Timer statusTimer;
        private RestUserMessage StatusMessage;

        private const string URL = "https://realtimeyoutube.com/tseries-overtakes-pewdiepie";

        public async Task SetUp(RestUserMessage statusMessage)
        {
            StatusMessage = statusMessage;
            // Set up the timer
            statusTimer = new Timer()
            {
                Interval = 20000,
                AutoReset = true,
                Enabled = true
            };
            await UpdateCount().ConfigureAwait(false);
            statusTimer.Elapsed += OnStatusTimerTicked;
        }

        private async void OnStatusTimerTicked(object sender, ElapsedEventArgs e) => await UpdateCount().ConfigureAwait(false);

        public async Task UpdateCount()
        {
            string html = Utilities.webClient.DownloadString(URL);

            string tseries = html.Substring(html.IndexOf("MuiTypography-root-250 MuiTypography-display4-251 t-heading-276\">") + 65);
            string pewdiepie = tseries;
            tseries = tseries.Substring(0, tseries.IndexOf("<"));

            pewdiepie = pewdiepie.Substring(pewdiepie.IndexOf("MuiTypography-root-250 MuiTypography-display4-251 t-heading-276\">") + 65);
            pewdiepie = pewdiepie.Substring(0, pewdiepie.IndexOf("<"));

            int pewdCount = int.Parse(pewdiepie);
            int tseriesCount = int.Parse(tseries);
            int difference = pewdCount - tseriesCount;

            StringBuilder description = new StringBuilder()
                .AppendLine($"PewDiePie: `{pewdCount.ToString("#,##0")}`")
                .AppendLine($"TSeries: `{tseriesCount.ToString("#,##0")}`").AppendLine()
                .AppendLine($"Subscriber Difference: `{difference.ToString("#,##0")}`");

            await StatusMessage.ModifyAsync(m => { m.Embed = Utilities.Embed("PewDiePie vs TSeries", description.ToString(), Colors.Blue, "", ""); });

            if (difference <= 0)
            {
                await StatusMessage.Channel.SendMessageAsync(null, false, Utilities.Embed("PewDiePie vs TSeries", $"@everyone PEWDIEPIE WENT BEHIND TSERIES AT {difference} SUBSCRIBERS", Colors.Blue, "", ""));
            }
        }
    }
}
