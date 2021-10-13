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
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace SecretariaEletronica.Events.Commands;

public class CommandErrored
{
    public static async Task Commands_CommandErrored(CommandsNextExtension commandsNext, CommandErrorEventArgs e)
    {
        e.Context.Client.Logger.LogError(EventIdent.BotEventId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

        if (e.Exception is ChecksFailedException)
        {
            DiscordEmoji emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Access denied",
                Description = $"{emoji} You do not have the permissions required to execute this command.",
                Color = new DiscordColor(0xFF0000)
            };
            await e.Context.RespondAsync(embed.Build());
        }
        else if(e.Exception is CommandNotFoundException)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "CommandNotFound",
                Description = "This command doesnt exist",
                Color = new DiscordColor(0xFF0000)
            };
            await e.Context.RespondAsync(embed.Build());
        }
        else if(e.Exception is DuplicateCommandException)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "CommandAlreadyExist",
                Description = "This command already exists, please, change the attachment name",
                Color = new DiscordColor(0xFF0000)
            };
            await e.Context.RespondAsync(embed.Build());
        }
        else
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Error",
                Description = e.Exception.ToString(),
                Color = new DiscordColor(0xFF0000)
            };
            await e.Context.RespondAsync(embed.Build());
        }
    }
}