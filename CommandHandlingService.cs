using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace HAL9000
{
    public class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        private InviteTracker _inviteTracker;

        private SocketGuild _reversesGuild;

        private ITextChannel _logChannel;

        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _discord.InteractionCreated += OnInteractionCreated;

            _discord.Ready += OnReadyAsync;

            _discord.UserJoined += OnUserJoined;

            _discord.GuildMemberUpdated += OnGuildMemberUpdated;

            // Invite handler thingy
            _discord.InviteCreated += OnInviteCreated;

            _discord.MessageUpdated += CheckForUnoWin;
        }

        private async Task CheckForUnoWin(Cacheable<IMessage, ulong> arg1, SocketMessage msg, ISocketMessageChannel arg3)
        {
            Console.WriteLine("hi");
            if (msg.Author.Id == 914696129067757608 && msg.Content == "The game is over, I hope you had fun 😊")
            {
                var winner = msg.Embeds.ElementAt(0).Author.Value.Name;

                var rounds = msg.Embeds.ElementAt(0).Description;

                // Cut everything before "after "
                rounds = rounds.Substring(rounds.IndexOf("after ") + 6); // Extra 6 letters for "after "

                // Cut evrything after "!""
                rounds = rounds.Substring(0, rounds.IndexOf("!"));

                await _discord.GetGuild(735263201612005472).GetTextChannel(735263201612005476).SendMessageAsync($"Detected an UNO win by {winner} in {rounds} rounds!");
            }
        }

        // Save the new invite
        private async Task OnInviteCreated(SocketInvite arg) => _inviteTracker.SaveInvites(await _reversesGuild.GetInvitesAsync());

        // Restart one of my bots if they went offline
        private async Task OnGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> before, SocketGuildUser after)
        {
            // If the SocketGuildUser isn't my bot, then we don't care
            if (!Bots.List.Contains(before.Id)) return;

            // If one of my bots went offline, restart it
            if (before.Value.Status == UserStatus.Online && after.Status == UserStatus.Offline)
                Bots.RestartBot(before.Id);
        }

        private async Task OnUserJoined(SocketGuildUser user)
        {
            // Only for my server
            if (user.Guild.Id != Config.ReversesGuildId) return;

            await _logChannel.SendMessageAsync(user.Mention, embed: new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName(user.Username)
                    .WithIconUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()))
                .WithColor(Embeds.Red)
                .WithDescription($"{user.Username} has joined the server.\n\n{_inviteTracker.GetInviterAsync(await _reversesGuild.GetInvitesAsync(), user.Id)}")
                .Build());
        }

        private async Task OnReadyAsync()
        {
            _reversesGuild = _discord.GetGuild(Config.ReversesGuildId);

            _logChannel = _reversesGuild.GetTextChannel(Config.LogChannelId);

            // Initialize the invite tracker
            _inviteTracker = new InviteTracker();
            _inviteTracker.SaveInvites(await _reversesGuild.GetInvitesAsync());
        }

        private async Task RestartBot(SocketInteraction interaction, ulong botId)
        {
            if (interaction.User.Id != Config.ReversesId)
            {
                await interaction.PrintError();
                return;
            }

            else if (!Bots.List.Contains(botId))
            {
                await interaction.PrintError("That can't be restarted");
                return;
            }

            Bots.RestartBot(botId);
            var botAccount = _reversesGuild.GetUser(botId);
            await interaction.RespondAsync(embed: new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName($"Restarted {botAccount.Username}")
                    .WithIconUrl(botAccount.GetAvatarUrl()))
                .WithColor(Embeds.Green)
                .Build());
        }

        private async Task OnInteractionCreated(SocketInteraction interaction)
        {
            switch (interaction)
            {
                // Slash Commands
                case SocketSlashCommand slashCommand:

                    // /restart
                    if (slashCommand.CommandName == "restart")
                    {
                        await RestartBot(interaction, ((SocketGuildUser)slashCommand.Data.Options.ElementAt(0).Value).Id);
                    }

                    // /avatar
                    else if (slashCommand.CommandName == "avatar")
                    {
                        var target = ((SocketGuildUser)slashCommand.Data.Options.ElementAt(0).Value);
                        await slashCommand.RespondAsync(target.GetAvatarUrl() ?? target.GetDefaultAvatarUrl());
                    }

                    break;

                // User Commands (context menut)
                case SocketUserCommand userCommand:

                    // Restart command
                    if (userCommand.CommandName == "Restart")
                        await RestartBot(interaction, userCommand.Data.Member.Id);

                    // Avatar command
                    else if (userCommand.CommandName == "Avatar")
                        await userCommand.RespondAsync(userCommand.Data.Member.GetAvatarUrl());

                    break;

                default:
                    break;
            }
        }

        public async Task InitializeAsync()
        {
            // Register modules that are public and inherit ModuleBase<T>.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
    }
}
