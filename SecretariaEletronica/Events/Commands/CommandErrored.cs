using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace SecretariaEletronica.Events.Commands
{
    public class CommandErrored
    {
        public async Task Commands_CommandErrored(CommandsNextExtension commandsNext, CommandErrorEventArgs e)
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
}