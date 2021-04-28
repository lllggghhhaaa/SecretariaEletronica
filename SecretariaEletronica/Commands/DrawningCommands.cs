using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace SecretariaEletronica.Commands
{
    public class DrawningCommands : BaseCommandModule
    {
        [Command("invert")]
        public async Task Invert(CommandContext ctx)
        {
            if (ctx.Message.Attachments.Count != 1)
            {
                await ctx.RespondAsync("Send the image");
                return;
            }

            using (HttpClient client = new HttpClient())
            {
                using (Stream stream = await client.GetStreamAsync(ctx.Message.Attachments[0].Url))
                {
                    Bitmap reversed = new Bitmap(stream);

                    for (int y = 0; (y <= (reversed.Height - 1)); y++)
                    {
                        for (int x = 0; (x <= (reversed.Width - 1)); x++)
                        {
                            Color inv = reversed.GetPixel(x, y);
                            inv = Color.FromArgb(255, (255 - inv.R), (255 - inv.G), (255 - inv.B));
                            reversed.SetPixel(x, y, inv);
                        }
                    }

                    reversed.Save("image.png");

                    DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();
                    messageBuilder.WithFile("image.png");

                    await ctx.RespondAsync(messageBuilder);
                }
            }
        }

        [Command("mirror")]
        public async Task Mirror(CommandContext ctx)
        {
            using (HttpClient client = new HttpClient())
            {
                using (Stream stream = await client.GetStreamAsync(ctx.Message.Attachments[0].Url))
                {
                    Bitmap image = new Bitmap(stream);

                    int width = image.Width;
                    int height = image.Height;
                    
                    Bitmap mimg = new Bitmap(width*2, height);

                    for (int y = 0; y < height; y++)
                    {
                        for (int lx = 0, rx = width * 2 - 1; lx < width; lx++, rx--)
                        {
                            Color p = image.GetPixel(lx, y);

                            mimg.SetPixel(lx, y, p);
                            mimg.SetPixel(rx, y, p);
                        }
                    }

                    mimg.Save("image.png");
                    
                    DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();
                    messageBuilder.WithFile("image.png");

                    await ctx.RespondAsync(messageBuilder);
                }
            }
        }
    }
}