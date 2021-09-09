//   Copyright 2022 lllggghhhaaa
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//       You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//       distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//       See the License for the specific language governing permissions and
//   limitations under the License.

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
            if (user.Id == 597926883069394996)
            {
                await ctx.RespondAsync(user.Username + " banned!");
                return;
            }
        
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