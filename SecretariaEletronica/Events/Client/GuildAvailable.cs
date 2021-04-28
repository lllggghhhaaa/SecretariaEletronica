using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace SecretariaEletronica.Events.Client
{
    public class GuildAvailable
    {
        private readonly DiscordShardedClient _client;

        public GuildAvailable(DiscordShardedClient client)
        {
            _client = client;
        }
        
        public Task Client_GuildAvailable(DiscordClient client, GuildCreateEventArgs e)
        {
            _client.Logger.LogInformation(EventIdent.BotEventId, $"Guild available: {e.Guild.Name}");
            
            return Task.CompletedTask;
        }
    }
}