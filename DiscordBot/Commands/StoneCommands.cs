using Discord;
using System.Linq;
using Discord.Commands;
using System.Threading.Tasks;

namespace Gideon.Handlers
{
    [RequireContext(ContextType.Guild)]
    public class StoneCommands : ModuleBase<SocketCommandContext>
    {
        // Chat bomb
        [RequireStone("Space")]
        [Command("space clear")]
        public async Task ClearMessage()
        {
            await Context.Channel.SendMessageAsync($"||{new string('\n', 1996)}||");
        }

        [RequireStone("Space")]
        [RequireRole("Avenger")]
        [Command("soul")]
        [Alias("soulstone", "soul stone")]
        public async Task AttemptToGetSoulStone()
        {
            await Utilities.SendEmbed(Context.Channel, "Soul Stone", "You must sacrifice all of your level xp.\n\nIf you are sure, type `!soul get`.", new Color(230, 126, 34), "", "");
        }

        [RequireStone("Space")]
        [RequireRole("Avenger")]
        [Command("soul get")]
        public async Task GetSoulStone()
        {
            var account = UserAccounts.GetAccount(Context.User);
            account.xp = 0;
            account.level = 0;
            UserAccounts.SaveAccounts();
            await (Context.User as IGuildUser).RemoveRoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Avenger"));
            await (Context.User as IGuildUser).AddRoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Soul"));
            await Utilities.SendEmbed(Context.Channel, "Soul Stone", $"{Context.User.Mention} has acquired the Soul Stone.", new Color(230, 126, 34), "", "");
        }
    }
}