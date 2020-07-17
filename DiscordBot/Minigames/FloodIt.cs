using System;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Gideon.Minigames
{
    class FloodIt
    {

        private static string red = "<:1_:698269871728754770>";
        private static string blue = "<:3_:698269871741599744>";
        private static string green = "<:2_:698269871460450345>";
        private static string yellow = "<:4_:698269871821160639>";
        private static string purple = "<:6_:698269871838068807>";
        private static string orange = "<:5_:698269871560982582>";

        private readonly string[] colors = { red, blue, green, yellow, purple, orange };

        private string[,] board = new string[9, 9];
        private bool[,] doIChange = new bool[9, 9];

        private Random random = new Random();

        private string GetBoard()
        {
            var boardText = new StringBuilder();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    boardText.Append(board[i, j]);
                }

                boardText.Append("\n");
            }

            return boardText.ToString();
        }

        public async Task Initialize(ISocketMessageChannel channel)
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    board[i, j] = colors[random.Next(0, 6)];
                    doIChange[i, j] = false;
                }

            doIChange[0, 0] = true;

            await channel.SendMessageAsync(GetBoard());
        }

        public async Task PlayGreen(ISocketMessageChannel channel)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (doIChange[i, j])
                    {
                        board[i, j] = green;
                        for (int k = 0; k < 9; k++)
                        {
                            for (int l = 0; l < 9; l++)
                            {
                                if (!doIChange[k, l] && isAdjacent(i, j, k, l) && board[k, l] == green)
                                {
                                    doIChange[k, l] = true;
                                }
                            }
                        }
                    }
                }
            }
            await channel.SendMessageAsync(GetBoard());
        }

        private bool isAdjacent(int x1, int y1, int x2, int y2)
        {
            return ((Math.Abs(x1 - x2) == 1 && Math.Abs(y1 - y2) == 0) || (Math.Abs(x1 - x2) == 0 && Math.Abs(y1 - y2) == 1));
        }
    }
}
