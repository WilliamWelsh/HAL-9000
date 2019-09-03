using System;
using Discord;
using System.Text;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Handlers
{
    [RequireContext(ContextType.Guild)]
    public class FunCommands : ModuleBase<SocketCommandContext>
    {
        // Used to turn text upside down
        [Command("australia")]
        [Alias("aussie", "aus")]
        public async Task AussieText([Remainder]string message)
        {
            char[] X = @"¿/˙'\‾¡zʎxʍʌnʇsɹbdouɯlʞɾıɥƃɟǝpɔqɐ".ToCharArray();
            string V = @"?\.,/_!zyxwvutsrqponmlkjihgfedcba";
            string upsideDownText = new string((from char obj in message.ToCharArray()
                                                select (V.IndexOf(obj) != -1) ? X[V.IndexOf(obj)] : obj).Reverse().ToArray()); // thanks stack overflow
            await Utilities.SendEmbed(Context.Channel, "Australian Translator", upsideDownText, new Color(1, 33, 105), "", "");
        }

        // Reverse text
        [Command("reverse")]
        public async Task ReverseText([Remainder]string input)
        {
            char[] chars = input.ToCharArray();
            int len = input.Length - 1;
            for (uint i = 0; i < len; i++, len--)
            {
                chars[i] ^= chars[len];
                chars[len] ^= chars[i];
                chars[i] ^= chars[len];
            }
            await Utilities.SendEmbed(Context.Channel, "Reversed Text", new string(chars), Utilities.ClearColor, "", "");
        }

        // Spongebob Mock Meme
        [Command("mock")]
        public async Task Mock([Remainder]string message)
        {
            char[] letters = message.ToCharArray();
            for (int n = 0; n < letters.Length; n += 2)
                letters[n] = char.ToUpper(letters[n]);
            string name = ((SocketGuildUser)Context.User).Nickname ?? Context.User.Username;
            await Utilities.SendEmbed(Context.Channel, "", new string(letters), Utilities.ClearColor, $"Mocked by {name}", "http://i0.kym-cdn.com/photos/images/masonry/001/255/479/85b.png");
        }

        // Display a random person on the server
        [Command("someone")]
        public async Task GetRandomPerson()
        {
            SocketGuildUser[] s = Context.Guild.Users.ToArray();
            SocketGuildUser randomUser = s[Utilities.GetRandomNumber(0, s.Length)];
            string footer = $"{((1.0m / Context.Guild.MemberCount) * 100).ToString("N3")}% chance of selecting this user.";
            await Utilities.SendEmbed(Context.Channel, "Random User", randomUser.ToString(), Utilities.DomColorFromURL(randomUser.GetAvatarUrl()), footer, randomUser.GetAvatarUrl());
        }

        // See how many tries it would take to randomly get a specific user
        [Command("someone")]
        public async Task GetRandomPerson(SocketGuildUser user)
        {
            SocketGuildUser[] s = Context.Guild.Users.ToArray();
            uint count = 0;
            int arraySize = s.Length;
            bool hasFound = false;
            do
            {
                count++;
                SocketGuildUser randomUser = s[Utilities.GetRandomNumber(0, arraySize)];
                if (randomUser == user) hasFound = true;
            } while (!hasFound);

            string footer = $"{((1.0m / Context.Guild.MemberCount) * 100).ToString("N3")}% chance of selecting this user.";
            await Utilities.SendEmbed(Context.Channel, "Random User", $"It took {count} tries to find {user.Mention}.", Utilities.DomColorFromURL(user.GetAvatarUrl()), footer, user.GetAvatarUrl());
        }

        // Convert ASCII to Binary
        [Command("binary")]
        public async Task ASCIIToBinary([Remainder]string ascii)
        {
            StringBuilder binary = new StringBuilder();
            foreach (char c in ascii)
                binary.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            if (binary.Length > 2000)
            {
                await Utilities.SendEmbed(Context.Channel, "ASCII to Binary Converter", "The resulting binary is too long (over 2000 characters).\nDue to Discord's character count limitation, I am unable to send the message.", Utilities.ClearColor, "", "");
                return;
            }
            await Utilities.SendEmbed(Context.Channel, "ASCII to Binary Converter", binary.ToString(), Utilities.ClearColor, "", "");
        }

        // Convert Binary to ASCII
        [Command("ascii")]
        public async Task BinaryToASCII([Remainder]string input)
        {
            if (input.Length % 8 != 0)
            {
                await Utilities.SendEmbed(Context.Channel, "Binary to ASCII Converter", "Sorry, that cannot be converted to text.\nThe length of the binary must be a multiple of 8.", Utilities.ClearColor, "", "");
                return;
            }
            var list = new List<byte>();
            for (int i = 0; i < input.Length; i += 8)
            {
                string bit = input.Substring(i, 8);
                list.Add(Convert.ToByte(bit, 2));
            }

            await Utilities.SendEmbed(Context.Channel, "Binary to ASCII Converter", Encoding.ASCII.GetString(list.ToArray()), Utilities.ClearColor, "", "");
        }
    }
}