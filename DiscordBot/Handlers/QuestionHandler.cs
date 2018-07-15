using Discord.Commands;
using System.Threading.Tasks;

namespace Gideon.Handlers
{
    class QuestionHandler
    {
        public async Task InterpretQuestion(SocketCommandContext context, string input)
        {
            foreach(string s in Config.botQuestions.WhenIsNextVideo)
            {
                if(input.Contains(s))
                {
                    if (Config.botResources.nextVideoDate == "n/a")
                        await context.Channel.SendMessageAsync("There is no set date for the next update video.");
                    else
                        await context.Channel.SendMessageAsync($"The next video comes out {Config.botResources.nextVideoDate}.");
                    return;
                }
            }

            foreach (string s in Config.botQuestions.HowMuchDoesGameCost)
            {
                if (input.Contains(s))
                {
                    await context.Channel.SendMessageAsync("The game will be free. This is because we cannot financially profit off of this project without encountering legal issues.");
                    return;
                }
            }

            foreach (string s in Config.botQuestions.WhichPlatform)
            {
                if (input.Contains(s))
                {
                    await context.Channel.SendMessageAsync("This game will **ONLY** be on PC.");
                    return;
                }
            }

            foreach (string s in Config.botQuestions.IsBlackLightningInGame)
            {
                if (input.Contains(s))
                {
                    await context.Channel.SendMessageAsync("Black Lightning will not be featured in the game because he is not a part of the Arrowverse.");
                    return;
                }
            }

            foreach (string s in Config.botQuestions.IsBatmanInGame)
            {
                if (input.Contains(s))
                {
                    await context.Channel.SendMessageAsync("Batman will not be added to the game. This is final.");
                    return;
                }
            }

            foreach (string s in Config.botQuestions.IsBatwomanInGame)
            {
                if (input.Contains(s))
                {
                    await context.Channel.SendMessageAsync("We will not be adding Batwoman to the game.");
                    return;
                }
            }

            foreach (string s in Config.botQuestions.WhereIsDownload)
            {
                if (input.Contains(s))
                {
                    await context.Channel.SendMessageAsync("The game is not yet available for download.");
                    return;
                }
            }

            foreach (string s in Config.botQuestions.WhenDoesGameComeOut)
            {
                if (input.Contains(s))
                {
                    await context.Channel.SendMessageAsync("There is no set release date. However, we are hoping for a 2019 release.");
                    return;
                }
            }

            return;
        }
    }
}