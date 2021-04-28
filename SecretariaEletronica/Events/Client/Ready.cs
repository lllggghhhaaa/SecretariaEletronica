using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace SecretariaEletronica.Events.Client
{
    public class Ready
    {
        private readonly DiscordShardedClient _client;

        public Ready(DiscordShardedClient client)
        {
            this._client = client;
        }
        
        public Task Client_Ready(DiscordClient client, ReadyEventArgs e)
        {
            _client.Logger.LogInformation(EventIdent.BotEventId, "Client is ready to process events.");
            
            return Task.CompletedTask;
        }
    }
}