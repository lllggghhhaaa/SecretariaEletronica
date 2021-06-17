using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace SecretariaEletronica.Commands
{
    public class ModeratorCommands : BaseCommandModule
    {
        [Command("ban"), Description("Ban member"), RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers)]
        public async Task BanMember(CommandContext ctx, DiscordMember user, [RemainingText]string reason = "no reason")
        {
            await user.Guild.BanMemberAsync(user, 0, reason);
            await ctx.RespondAsync(user.Username + " banned!");
        }

        [Command("eval"), RequireOwner, Aliases("evaluate", "e")]
        public async Task Evaluate(CommandContext ctx, [RemainingText] string code)
        {
            object response = CSharpScript.EvaluateAsync(code, ScriptOptions.Default).Result;

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "Terminal >_"
            };

            embedBuilder.AddField("Input", $"`{code}`", true);
            embedBuilder.AddField("Output", $"`{response}`", true);
            await ctx.RespondAsync(embedBuilder.Build());
        }
    }
}