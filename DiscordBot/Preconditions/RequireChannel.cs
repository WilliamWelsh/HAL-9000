using System;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DiscordBot
{
    /// <summary>
    /// Require the command to be a specific channel
    /// </summary>
    public class RequireChannel : PreconditionAttribute
    {
        // The required channel's ID
        private readonly ulong channelID;

        // Constructor
        public RequireChannel(ulong ChannelID) => channelID = ChannelID;

        // Override the CheckPermissions method
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            // Check if this user is a Guild User, which is the only context where roles exist
            if (context.User is SocketGuildUser gUser)
            {
                // If this command was executed by a user with the appropriate role, return a success
                if (context.Channel.Id == channelID)
                    return await Task.FromResult(PreconditionResult.FromSuccess());
                else
                {
                    await Utilities.PrintError((ISocketMessageChannel)context.Channel, $"Please use the {(await context.Guild.GetTextChannelAsync(channelID)).Mention} chat for that, {context.User.Mention}.");
                    return await Task.FromResult(PreconditionResult.FromError(""));
                }
            }
            else
                return await Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
        }
    }
}
