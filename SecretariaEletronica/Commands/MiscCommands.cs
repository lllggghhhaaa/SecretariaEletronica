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

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace SecretariaEletronica.Commands;

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
                Text = "โ Essa mensagem estรก disponรญvel apenas para usuรกrios do ๐๐ฒ๐ผ๐ฌ๐ธ๐ป๐ญ 2 from ๐๐๐๐ฃ๐ ๐ค๐ ๐๐ฅ",
                IconUrl = "https://cdn.discordapp.com/attachments/816739970161180692/825816505341837342/discordmicrosoft.png"
            }
        };
            
        await ctx.RespondAsync(embedBuilder.Build());
    }
}