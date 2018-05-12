using System;
using Discord;
using System.Linq;
using Gideon.Minigames;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;

namespace Gideon
{
    class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _service;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += HandleCommandAsync;
        }

        public string PayAttentionToUser = "";
        public bool isPayingAttentionToUser = false;
        public bool PayAttentionToUserDone = false;

        Trivia Trivia = new Trivia();
        NumberGuess NGGame = new NumberGuess();
        TicTacToe TTT = new TicTacToe();
        RussianRoulette RR = new RussianRoulette();

        private static readonly Random getrandom = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            lock (getrandom) { return getrandom.Next(min, max); }
        }

        private string TellTime(int h, string p)
        {
            DateTime t = DateTime.Now.AddHours(h);
            return "It's " + t.ToString("h:mm tt") + " for " + p + ".\n" + t.ToString("dddd, MMMM d.");
        }

        bool isDonePayingAttention(string user)
        {
            if (user == PayAttentionToUser && isPayingAttentionToUser && !PayAttentionToUserDone)
                return true;
            return false;
        }

        public void StopPayingAttention() => PayAttentionToUserDone = true;

        bool isRespectedPlus(SocketCommandContext c, SocketGuildUser u)
        {
            var Respected = c.Guild.Roles.FirstOrDefault(x => x.Name == "Respected");
            var Helper = c.Guild.Roles.FirstOrDefault(x => x.Name == "Helpers");
            var Developer = c.Guild.Roles.FirstOrDefault(x => x.Name == "Developer");
            var Lead = c.Guild.Roles.FirstOrDefault(x => x.Name == "Lead");
            var Director = c.Guild.Roles.FirstOrDefault(x => x.Name == "Director");
            if (u.Roles.Contains(Respected) ||
                u.Roles.Contains(Helper) ||
                u.Roles.Contains(Developer) ||
                u.Roles.Contains(Lead) ||
                u.Roles.Contains(Director))
                return true;
            return false;
        }

        private string DevTime(int h, string p)
        {
            DateTime t = DateTime.Now.AddHours(h);
            return p + ":" + t.ToString(" h:mm tt, dddd, MMMM d.");
        }

        Embed embed(string desc)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Time Teller");
            embed.WithDescription(desc);
            embed.WithColor(new Color(127, 166, 208));
            return embed;
        }

        Embed embedYT(string ID, string name)
        {
            MediaFetchHandler mediaFH = new MediaFetchHandler();
            MediaFetchHandler.YTChannel channel;
            channel = mediaFH.FetchYTChannel(ID);
            var embed = new EmbedBuilder();
            embed.WithTitle($":desktop: {name}'s YT Stats");
            embed.WithColor(new Color(227, 37, 39));
            embed.AddField("Views", Int32.Parse(channel.items[0].statistics.viewCount).ToString("#,##0"));
            embed.AddField("Subscribers", Int32.Parse(channel.items[0].statistics.subscriberCount).ToString("#,##0"));
            embed.AddField("Videos", Int32.Parse(channel.items[0].statistics.videoCount).ToString("#,##0"));
            if (name == "Teco")
                embed.WithThumbnailUrl("https://yt3.ggpht.com/-DdcoySXTxN8/AAAAAAAAAAI/AAAAAAAAAAA/UZKCiCRP15o/s288-mo-c-c0xffffffff-rj-k-no/photo.jpg");
            if (name == "Renz")
                embed.WithThumbnailUrl("https://yt3.ggpht.com/-ZcC98kd5iIY/AAAAAAAAAAI/AAAAAAAAAAA/O2v690B6I90/s288-mo-c-c0xffffffff-rj-k-no/photo.jpg");
            if (name == "Captain Slander")
                embed.WithThumbnailUrl("https://yt3.ggpht.com/-_1kzQCWabiE/AAAAAAAAAAI/AAAAAAAAAAA/ffrQ76O46M8/s288-mo-c-c0xffffffff-rj-k-no/photo.jpg");
            if (name == "Jonathan TRG")
                embed.WithThumbnailUrl("https://yt3.ggpht.com/-1jVFeeQWsj8/AAAAAAAAAAI/AAAAAAAAAAA/ffJ4bHhtQoo/s288-mo-c-c0xffffffff-rj-k-no/photo.jpg");
            if (name == "R/Slurpy")
                embed.WithThumbnailUrl("https://yt3.ggpht.com/-2wPz3cZResM/AAAAAAAAAAI/AAAAAAAAAAA/v3BNxqIoMhI/s288-mo-c-c0xffffffff-rj-k-no/photo.jpg");
            return embed;
        }

        Embed embedDev()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Current Time for the Developers");
            embed.WithDescription("");
            embed.WithColor(new Color(255, 0, 0));
            embed.AddField("--- Director ---", DevTime(18, "Teco"));
            embed.AddField("--- Lead ---",
                    DevTime(4, "Kara") + "\n" +
                    DevTime(4, "Galva") + "\n" +
                    DevTime(18, "P-Zoom"));
            embed.AddField("--- Developer ---",
                    DevTime(0, "Reverse") + "\n" +
                    DevTime(10, "Jonathan") + "\n" +
                    DevTime(18, "Cottage") + "\n" +
                    DevTime(4, "Noah") + "\n" +
                    DevTime(4, "Renz"));
            embed.AddField("--- Helper --- ",
                DevTime(1, "Gannon")
                );
            return embed;
        }

        ImageFetcher imageFetcher = new ImageFetcher();

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;
            if (msg.Author.ToString() == "Gideon#8386") return;

            var context = new SocketCommandContext(_client, msg);

            if (PayAttentionToUserDone)
            {
                isPayingAttentionToUser = false;
                PayAttentionToUser = "";
                PayAttentionToUserDone = false;
            }

            int argPos = 0;
            if (msg.HasStringPrefix("!", ref argPos) || isPayingAttentionToUser && msg.Author.ToString() == PayAttentionToUser)
            {
                var result = await _service.ExecuteAsync(context, argPos);
                if (result.IsSuccess)
                {
                    if (msg.Author.ToString() == PayAttentionToUser && isPayingAttentionToUser)
                    {
                        isPayingAttentionToUser = false;
                        PayAttentionToUser = "";
                        PayAttentionToUserDone = false;
                    }
                }
            }

            string m = msg.Content.ToLower();

            if (msg.Author.ToString() == PayAttentionToUser && isPayingAttentionToUser)
            {
                m = "!" + m;
            }
            /*
            if(msg.Author.ToString() == "Username#2720")
            {
                var messages = await context.Channel.GetMessagesAsync(1).Flatten();
                await context.Channel.DeleteMessagesAsync(messages);
                char[] letters = msg.Content.ToCharArray();
                for (int n = 0; n < letters.Length; n += 2)
                {
                    letters[n] = char.ToUpper(letters[n]);
                }
                string new_s = new string(letters);
                await msg.Channel.SendMessageAsync("McSlandas: " + new_s);
            }*/

            if (m == "!reset")
            {
                if (!isRespectedPlus(context, (SocketGuildUser)msg.Author)) return;
                isPayingAttentionToUser = false;
                PayAttentionToUser = "";
                PayAttentionToUserDone = false;
                return;
            }

            if (m == "!reset trivia")
            {
                if (!isRespectedPlus(context, (SocketGuildUser)msg.Author)) return;
                await msg.Channel.SendMessageAsync($"{context.User.Mention} has reset Trivia.");
                Trivia.ResetTrivia();
                return;
            }

            if (m == "!reset rr")
            {
                if (!isRespectedPlus(context, (SocketGuildUser)msg.Author)) return;
                await msg.Channel.SendMessageAsync($"{context.User.Mention} has reset Russian Roulette.");
                Config.RR.Reset();
                return;
            }

            if (m == "!reset ttt")
            {
                if (!isRespectedPlus(context, (SocketGuildUser)msg.Author)) return;
                TTT.Reset();
                await msg.Channel.SendMessageAsync($"{msg.Author.Mention} has reset Tic-Tac-Toe.");
                return;
            }

            if (m == "!reset ng")
            {
                NGGame.Reset();
                return;
            }

            if (m.Contains("batman"))
            {
                if (m.Contains("is") || m.Contains("will"))
                    await msg.Channel.SendMessageAsync("Batman is not going to be in the game.");
            }

            if (m.Contains("black lightning"))
            {
                if (m.Contains("is") || m.Contains("will"))
                    await msg.Channel.SendMessageAsync("Black Lightning is not going to be in the game because he is not in the Arrowverse.");
            }

            if (m.Contains("video") && m.Contains("when") || m == "update video soon?")
            {
                await msg.Channel.SendMessageAsync("There is no set date for the next update video.");
            }

            if(m.Contains("much") && m.Contains("game") && m.Contains("cost"))
            {
                await msg.Channel.SendMessageAsync("This game will be free.");
            }

            if (m.Contains("is") && m.Contains("game") && m.Contains("free") && m.Contains("this"))
            {
                await msg.Channel.SendMessageAsync("This game will be free.");
            }

            if (m.Contains("much") && m.Contains("game") && m.Contains("money") && m.Contains("be"))
            {
                await msg.Channel.SendMessageAsync("This game will be free.");
            }

            if (m.Contains("cottage > teco"))
            {
                await msg.Channel.SendMessageAsync("https://i.imgur.com/ZPkm0nc.png");
                return;
            }





            if(m.StartsWith("!put"))
            {
                await TTT.PutLetter(context, m);
                return;
            }

            if (m.StartsWith("!ttt"))
            {
                await TTT.TryToStartGame(context, m);
                return;
            }

            if (m == "!join ttt")
            {
                await TTT.TryToJoinGame(context);
                return;
            }



            if(m.StartsWith("!rr"))
            {
                await Config.RR.TryToStartGame(context, m);
                return;
            }

            if (m == "!join rr")
            {
                await Config.RR.TryToJoin(context);
                return;
            }

            if (m == "!pt")
            {
                await Config.RR.PullTrigger(context);
                return;
            }






            if (m.StartsWith("!play ng"))
            {
                await NGGame.TryToStartGame(GetRandomNumber(1, 100), (SocketGuildUser)msg.Author, context, m);
            }

            if (m == "!join ng")
            {
                await NGGame.JoinGame((SocketGuildUser)msg.Author, context);
            }

            if (m.StartsWith("!g"))
            {
                await NGGame.TryToGuess((SocketGuildUser)msg.Author, context, m);
            }




            if(m.StartsWith("!trivia"))
                await Trivia.TryToStartTrivia((SocketGuildUser)msg.Author, context, m);

            if (m == "a" || m == "b" || m == "c" || m == "d")
                await Trivia.AnswerTrivia((SocketGuildUser)msg.Author, context, m);



















            if (m.Contains("teco > cottage"))
            {
                await msg.Channel.SendMessageAsync("https://i.imgur.com/oghnB1D.png");
                return;
            }

            if (msg.Author.ToString() == "Username#2720")
            {
                if (m == "!slander approves" || m == "!siander approves")
                {
                    await msg.Channel.SendMessageAsync("https://i.imgur.com/QSL86jU.png");
                    return;
                }
            }

            if (msg.Content == "Gideon!" && !isPayingAttentionToUser)
            {
                isPayingAttentionToUser = true;
                PayAttentionToUser = msg.Author.ToString();
                if (msg.Author.ToString() == "Tecosaurus#6343")
                {
                    await msg.Channel.SendMessageAsync("Yes, Director Teco?");
                    return;
                }
                string user = (msg.Author as SocketGuildUser).Nickname != null ? (msg.Author as SocketGuildUser).Nickname : msg.Author.Username;
                await msg.Channel.SendMessageAsync($"Yes, {user}?");
                return;
            }

            if (msg.Content.Contains(":SummonTeco:"))
            {
                var role = context.Guild.Roles.FirstOrDefault(x => x.Name == "Director");
                foreach (SocketGuildUser u in context.Guild.Users.ToArray())
                {
                    if (u.Roles.Contains(role))
                    {
                        string user = (msg.Author as SocketGuildUser).Nickname != null ? (msg.Author as SocketGuildUser).Nickname : msg.Author.ToString();
                        await u.SendMessageAsync($"[SUMMON ALERT] {user} has summoned you in #{msg.Channel}!");
                        return;
                    }
                }
            }

            if (msg.Content.Contains(":SlandaApproves:"))
            {
                if (context.User.ToString() == "Username#2720")
                    return;
                var messages = await context.Channel.GetMessagesAsync(1).Flatten();
                await context.Channel.DeleteMessagesAsync(messages);
            }

            if (m.Contains("lennyface"))
            {
                await context.Channel.SendMessageAsync("( ͡° ͜ʖ ͡°)");
            }

            var Ats = msg.Attachments.ToArray();

            if (m == "!michael")
            {
                string pic = imageFetcher.GetRandomPic(ref imageFetcher.Michael);
                var embedW = new EmbedBuilder();

                embedW.WithTitle("Michael :heart_eyes:");
                embedW.WithColor(new Utilities().DomColorFromURL(pic));
                embedW.WithImageUrl(pic);

                await context.Channel.SendMessageAsync("", false, embedW);
                return;
            }

            if (m == "!constantine")
            {
                string pic = imageFetcher.GetRandomPic(ref imageFetcher.Constantine);
                var embedW = new EmbedBuilder();

                embedW.WithTitle("Constantine :heart_eyes:");
                embedW.WithColor(new Utilities().DomColorFromURL(pic));
                embedW.WithImageUrl(pic);

                await context.Channel.SendMessageAsync("", false, embedW);
                return;
            }

            if (m == "!alani")
            {
                string pic = imageFetcher.GetRandomPic(ref imageFetcher.Alani);
                var embedW = new EmbedBuilder();

                embedW.WithTitle("Alani :heart_eyes:");
                embedW.WithColor(new Utilities().DomColorFromURL(pic));
                embedW.WithImageUrl(pic);

                await context.Channel.SendMessageAsync("", false, embedW);
                return;
            }

            if (Ats.Length > 0)
            {
                if (Config.bot.allowedChannels.Contains(context.Channel.Name)) return;
                if (isRespectedPlus(context, (SocketGuildUser)msg.Author)) return;

                var messages = await context.Channel.GetMessagesAsync(1).Flatten();
                await context.Channel.DeleteMessagesAsync(messages);
                await msg.Channel.SendMessageAsync($"Please post images in #off-topic only, {msg.Author.Mention}");
                return;
            }

            if (m.Contains("http://") || m.Contains("www.") || m.Contains(".com") || m.Contains("https://"))
            {
                if (Config.bot.allowedChannels.Contains(context.Channel.Name)) return;
                if (isRespectedPlus(context, (SocketGuildUser)msg.Author)) return;

                var messages = await context.Channel.GetMessagesAsync(1).Flatten();
                await context.Channel.DeleteMessagesAsync(messages);
                await msg.Channel.SendMessageAsync($"Please post links in #off-topic only, {msg.Author.Mention}");
                return;
            }

            string[] timePeople = { "Reverse", "Teco", "Cottage", "Zoom", "PZoom", "Noah", "Jon", "Boot", "Slander", "Bob", "ZX", "Gannon", "Matthias", "R", "Retarded R", "Waffle", "Vaddy",
        "poison", "poisonbreak"};
            int[] timeHourDif = { 0, 18, 18, 18, 18, 4, 10, 17, 18, 10, 20, 1, 16, 16, 16, 18, 4, 1, 1 };

            for (int i = 0; i < timePeople.Length; i++)
            {
                if (m == "!time " + timePeople[i].ToLower() || m.StartsWith("!" + timePeople[i].ToLower()) && (m.Substring(msg.Content.Length - 4) == "time"))
                {
                    await msg.Channel.SendMessageAsync("", false, embed(TellTime(timeHourDif[i], timePeople[i])));
                    PayAttentionToUserDone = isDonePayingAttention(msg.Author.ToString());
                    return;
                }
            }

            string[] spellingMistakes = { "should of", "would of", "wouldnt of", "wouldn't of", "would not of", "couldnt of", "couldn't of", "could not of", "better of", "shouldnt of", "shouldn't of", "should not of", "alot" };
            string[] spellingFix = { "should have", "would have", "wouldn't have", "wouldn't have", "would not have", "couldn't have", "couldn't have", "could not have", "better have", "shouldn't have", "shouldn't have", "should not have", "a lot" };

            for (int i = 0; i < spellingMistakes.Length; i++)
            {
                if (m.Contains(spellingMistakes[i]))
                {
                    await msg.Channel.SendMessageAsync(spellingFix[i] + "*");
                }
            }

            if (m == "!dev time")
            {
                PayAttentionToUserDone = isDonePayingAttention(msg.Author.ToString());
                await msg.Channel.SendMessageAsync("", false, embedDev());
                return;
            }

            if (m == "!yt" || m == "!subs" || m == "!youtube" || m == "!videos" || m == "!subscribers" ||
                m == "!yt teco" || m == "!tecosubs" || m == "!tecoyoutube" || m == "!tecovideos" || m == "!tecosubscribers")
            {
                PayAttentionToUserDone = isDonePayingAttention(msg.Author.ToString());
                await msg.Channel.SendMessageAsync("", false, embedYT("UCQ49yiS-PGeJTkyUqI5L62g", "Teco"));
                return;
            }

            if (m == "!ytrenz" || m == "!yt renz")
            {
                PayAttentionToUserDone = isDonePayingAttention(msg.Author.ToString());
                await msg.Channel.SendMessageAsync("", false, embedYT("UCL_kJSc23j9wgCo3cJjaiZg", "Renz"));
                return;
            }

            if (m == "!ytslander" || m == "!yt slander")
            {
                PayAttentionToUserDone = isDonePayingAttention(msg.Author.ToString());
                await msg.Channel.SendMessageAsync("", false, embedYT("UCm7JHwUjiXv1V5O15z6X4og", "Captain Slander"));
                return;
            }

            if (m == "!ytjon" || m == "!yt jon")
            {
                PayAttentionToUserDone = isDonePayingAttention(msg.Author.ToString());
                await msg.Channel.SendMessageAsync("", false, embedYT("UCJA6t3uRK-Bgs-IaDbNv_Dw", "Jonathan TRG"));
                return;
            }

            if (m == "!ytr" || m == "!yt r" || m == "!ytslurpy" || m == "!yt slurpy")
            {
                PayAttentionToUserDone = isDonePayingAttention(msg.Author.ToString());
                await msg.Channel.SendMessageAsync("", false, embedYT("UC8ittCidmMQ-025HJujT1Pw", "R/Slurpy"));
                return;
            }

            if (m.Contains("nigger"))
            {
                var messages = await context.Channel.GetMessagesAsync(1).Flatten();
                await context.Channel.DeleteMessagesAsync(messages);
            }

            if (m.StartsWith("!!!") && m.Length >= 4 && context.Channel.ToString() != "off-topic" && !m.StartsWith("!!!!"))
            {
                await new Utilities().AutoWarn(context, (SocketGuildUser)msg.Author, "using the Jukebox outside of #off-topic");
            }

            if (m.Contains("@everyone"))
            {
                await new Utilities().AutoWarn(context, (SocketGuildUser)msg.Author, "using @ everyone.");
            }
        }
    }
}