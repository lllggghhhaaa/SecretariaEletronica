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
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace SecretariaEletronica.Events.Commands;

public class CommandExecuted
{
    public static async Task Commands_CommandExecuted(CommandsNextExtension commandsNext, CommandExecutionEventArgs e)
    {
        e.Context.Client.Logger.LogInformation(EventIdent.BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

        DiscordChannel channel = e.Context.Client.GetChannelAsync(Startup.Configuration.LogCommands).Result;
            
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
        {
            Title = "Command executed",
            Description = $"Args: {e.Context.Message.Content}",
            Timestamp = DateTimeOffset.Now,
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                Name = e.Context.User.Username,
                IconUrl = e.Context.User.GetAvatarUrl(ImageFormat.Png)
            }
        };

        if (e.Context.User.Id is not 597926883069394996)
            await channel.SendMessageAsync(embed.Build());
    }
}