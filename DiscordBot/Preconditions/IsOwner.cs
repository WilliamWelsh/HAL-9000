using System;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DiscordBot
{
    /// <summary>
    /// Require a role to perform a command.
    /// </summary>
    public class IsOwner : PreconditionAttribute
    {
        // Override the CheckPermissions method
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            // Check if this user is a Guild User, which is the only context where roles exist
            if (context.User is SocketGuildUser)
            {
                // If this command was executed by a user with the appropriate role, return a success
                if (context.Guild.OwnerId == context.User.Id)
                    return await Task.FromResult(PreconditionResult.FromSuccess()).ConfigureAwait(false);
                else
                {
                    await Utilities.PrintError((ISocketMessageChannel)context.Channel, $"You do not have permission to perform this command.");
                    return await Task.FromResult(PreconditionResult.FromError($"You do not have permission to perform this command.")).ConfigureAwait(false);
                }
            }
            else
                return await Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command.")).ConfigureAwait(false);
        }
    }
}
