using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SecretariaEletronica.Commands
{
    public class CustomCommands : BaseCommandModule
    {
        [Command("upload")]
        public async Task Execute(CommandContext ctx)
        {
            if (ctx.Message.Attachments.Count != 1)
            {
                await ctx.RespondAsync("No File");
                return;
            }

            if (!ctx.Message.Attachments[0].FileName.EndsWith(".dll"))
            {
                await ctx.RespondAsync("File are not an DLL");
                return;
            }
            
            using (HttpClient client = new HttpClient())
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "CustomCommands", "ToAprove",
                    ctx.Message.Attachments[0].FileName);

                if (File.Exists(path))
                {
                    await ctx.RespondAsync("This file already exists, change file name");
                    return;
                }
                
                HttpResponseMessage response = await client.GetAsync(ctx.Message.Attachments[0].Url);
                FileStream fs = File.Create(path);

                await response.Content.CopyToAsync(fs);
                
                await ctx.RespondAsync("Downloaded, wait to host to approve");
            }
        }
    }
}