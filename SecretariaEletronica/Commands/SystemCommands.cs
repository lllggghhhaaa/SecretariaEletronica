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

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace SecretariaEletronica.Commands;

public class SystemCommands : BaseCommandModule
{
    [Command("shards"), RequireOwner]
    public async Task Shards(CommandContext ctx)
    {
        IReadOnlyDictionary<int, DiscordClient> shards = Startup.Client.ShardClients;

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
        {
            Title = "Bot Shards",
            Description = $"Shards count: `{shards.Count}`\n"
        };

        foreach (DiscordClient client in shards.Values)
        {
            embed.Description += $"\nShard {client.ShardCount}: `{client.Guilds.Count}` Guilds\n";
            foreach (DiscordGuild guild in client.Guilds.Values)
            {
                embed.Description += $"> `{guild.Name}`\n";
            }
        }

        await ctx.RespondAsync(embed.Build());
    }
}