using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace SecretariaEletronica.Events.Client
{
    public class ClientErrored
    {
        private readonly DiscordShardedClient _client;

        public ClientErrored(DiscordShardedClient client)
        {
            _client = client;
        }
        
        public Task Client_ClientErrored(DiscordClient client, ClientErrorEventArgs e)
        {
            _client.Logger.LogError(EventIdent.BotEventId, e.Exception, "Exception occured");

            return Task.CompletedTask;
        }
    }
}