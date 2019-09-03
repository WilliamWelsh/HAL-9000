using System;
using System.Drawing;
using Discord.Commands;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace DiscordBot.Handlers
{
    [RequireContext(ContextType.Guild)]
    public class EmojiCommands : ModuleBase<SocketCommandContext>
    {
        // This commands turns emoji input into image
        // The image will be the same way the emojis are inputted
        // For example, if someone builds a minecraft emoji house,
        // The image will be in the exact same shape
        [Command("image")]
        public async Task EmojisToImage([Remainder]string input)
        {
            // Split emojis into an array
            var rows = input.Split('\n');

            // Make a list of emojis that are in each row
            var allEmojis = new List<string[]>(rows.Length);

            // Split the emojis into separate items within each row
            foreach (var row in rows)
                allEmojis.Add(row.Split('>'));

            // The height of the image is the amount of emoji rows there are
            int imageRows = rows.Length;

            // The width of the image is the longest row of emoji's amount of emojis
            int imageColumns = 1;
            // Find the row with the most amount of emojis
            foreach (var row in allEmojis)
                if (row.Length > imageColumns)
                    imageColumns = row.Length;

            imageColumns--; // Subtract one because arrays start at 0

            // The images will be 128x128 (so they're all the same size)
            int imageWidth = 128 * imageColumns;
            int imageHeight = 128 * imageRows;

            Bitmap image = new Bitmap(imageWidth, imageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(image))
            {
                // Some settings to help with resizing
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                // Start drawing the emojis
                // Draw the image row by row
                for (int row = 0; row < allEmojis.Count; row++)
                {
                    // Get each emoji in the current row
                    for (int emoji = 0; emoji < allEmojis[row].Length; emoji++)
                    {
                        // If the emoji is an actual emoji then continue
                        // (the last element is just ">" as a result from splitting by ">" into an array)
                        if (allEmojis[row][emoji].Length > 1)
                        {
                            // Get the URL of the emoji and download it as an image
                            Image emojiImage = Utilities.DownloadImage(GetEmojiImageURL(allEmojis[row][emoji] + ">"));

                            // Draw the image at the column and row at 128x128
                            graphics.DrawImage(emojiImage,
                                new Rectangle(new Point(emoji * 128, row * 128), new Size(128, 128)),
                                new Rectangle(new Point(), emojiImage.Size),
                                GraphicsUnit.Pixel);
                        }
                    }
                }
            }

            // Save the image and send it
            image.Save("hi.png");
            await Context.Channel.SendFileAsync("hi.png");
        }

        // Makes an emoji big or prints an image of all the emojis
        [Command("url")]
        [Alias("jumbo")]
        public async Task PrintEmojis([Remainder]string input)
        {
            // Split emojis into an array
            var emojis = input.Split('>');

            // If there's just one emoji then print it
            if (emojis.Length == 2) // 2 because there's a leftover ">"
                await PrintEmoji(emojis[0] + ">").ConfigureAwait(false);
            else // Print the emojis in one big picture
                await PrintBigEmojiPicture(emojis, Context).ConfigureAwait(false);
        }

        private async Task PrintEmoji(string emoji)
        {
            string url = GetEmojiImageURL(emoji);
            await Context.Channel.SendMessageAsync("", false, Utilities.ImageEmbed("", "", Utilities.DomColorFromURL(url), "", url));
        }

        // Get the URL of an emoji
        private string GetEmojiImageURL(string emoji) => $"https://cdn.discordapp.com/emojis/{Regex.Replace(emoji, "[^0-9.]", "")}.{(emoji.StartsWith("<a:") ? "gif" : "png")}";

        private async Task PrintBigEmojiPicture(string[] emojis, SocketCommandContext Context)
        {
            // The rows and columns is the square root of the amount of emojis rounded up
            int rows = Convert.ToInt32(Math.Ceiling(Math.Sqrt(emojis.Length - 1)));
            int columns = rows;

            var emojiImages = new List<Image>();

            foreach (var emoji in emojis)
            {
                if (emoji.Length > 1)
                {
                    // Add ">" to the emoji cause it got removed when we split the emojis into an array
                    // Download the image and add it to the list
                    emojiImages.Add(Utilities.DownloadImage(GetEmojiImageURL($"{emoji}>")));
                }

            }

            int imageWidth = 128 * rows;
            int imageHeight = 128 * columns;

            Bitmap image = new Bitmap(imageWidth, imageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(image))
            {
                // Some settings to help with resizing
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                // Draw the rest of the images
                int i = 0;
                for (int row = 0; row < rows; row++)
                {
                    for (int column = 0; column < columns; column++)
                    {
                        i++;
                        if (i > emojiImages.Count)
                            break; // If we run out of images then stop
                        graphics.DrawImage(emojiImages[i - 1],
                            new Rectangle(new Point(column * 128, row * 128), new Size(128, 128)),
                            new Rectangle(new Point(), emojiImages[i - 1].Size),
                            GraphicsUnit.Pixel);
                    }
                }
            }

            // Save the image and send it
            image.Save("hi.png");
            await Context.Channel.SendFileAsync("hi.png");
        }
    }
}
