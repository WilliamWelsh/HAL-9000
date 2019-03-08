﻿using System;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon
{
    /// <summary>
    /// Require a role to perform a command.
    /// </summary>
    public class RequireStone : PreconditionAttribute
    {
        // The required role's name
        private readonly string stoneName;

        // Constructor
        public RequireStone(string name) => stoneName = name;

        // Override the CheckPermissions method
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            // Check if this user is a Guild User, which is the only context where roles exist
            if (context.User is SocketGuildUser gUser)
            {
                // If this command was executed by a user with the appropriate role, return a success
                if (gUser.Roles.Any(r => r.Name == stoneName))
                    return await Task.FromResult(PreconditionResult.FromSuccess());
                else
                {
                    await Utilities.PrintError((ISocketMessageChannel)context.Channel, $"You must have the {stoneName} stone to run this command.");
                    return await Task.FromResult(PreconditionResult.FromError(""));
                }
            }
            else
                return await Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
        }
    }
}
