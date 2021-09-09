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

using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace SecretariaEletronica.Events.Client
{
    public static class GuildCreated
    {
        public static async Task Client_GuildCreated(DiscordClient client, GuildCreateEventArgs args)
        {
            DiscordChannel channel = client.GetChannelAsync(Startup.Configuration.LogGuild).Result;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Guild Joined",
                Description = $"+{args.Guild.MemberCount} {args.Guild.Name}",
                Color = DiscordColor.Green,
                Timestamp = DateTimeOffset.Now
            };

            await channel.SendMessageAsync(embed.Build());
        }
    }
}