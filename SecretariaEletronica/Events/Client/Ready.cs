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
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace SecretariaEletronica.Events.Client;

public class Ready
{
    public static async Task Client_Ready(DiscordClient client, ReadyEventArgs e)
    {
        client.Logger.LogInformation(EventIdent.BotEventId, "Client is ready to process events");

        DiscordChannel channel = client.GetChannelAsync(Startup.Configuration.LogReady).Result;

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
        {
            Title = "Ready",
            Description = $"Ready at {client.Guilds.Values.Count()} guilds",
            Timestamp = DateTimeOffset.Now,
            Color = DiscordColor.Green
        };
            
        await channel.SendMessageAsync(embed.Build());
    }
}