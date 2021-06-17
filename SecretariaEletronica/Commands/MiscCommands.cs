using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace SecretariaEletronica.Commands
{
    public class MiscCommands : BaseCommandModule
    {
        [Command("ping"), Description("Get bot latency")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync($"Ping: {ctx.Client.Ping}ms");
        }

        [Command("botinfo"), Description("bot information")]
        public async Task BotInfo(CommandContext ctx)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "BotInfo",
                Description = $"Servers: {ctx.Client.Guilds.Count}\n" +
                              $"Commands: {ctx.Client.GetCommandsNext().RegisteredCommands.Count}\n" +
                              $"Latency: {ctx.Client.Ping}ms\n",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "ⓘ Essa mensagem está disponível apenas para usuários do 𝓓𝓲𝓼𝓬𝓸𝓻𝓭 2 from 𝕄𝕚𝕔𝕣𝕠𝕤𝕠𝕗𝕥",
                    IconUrl = "https://cdn.discordapp.com/attachments/816739970161180692/825816505341837342/discordmicrosoft.png"
                }
            };
            
            await ctx.RespondAsync(embedBuilder.Build());
        }
    }
}