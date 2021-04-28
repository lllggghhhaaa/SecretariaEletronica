using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;

namespace SecretariaEletronica.Events.Commands
{
    public class CommandExecuted
    {
        public Task Commands_CommandExecuted(CommandsNextExtension commandsNext, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(EventIdent.BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");
            
            return Task.CompletedTask;
        }
    }
}