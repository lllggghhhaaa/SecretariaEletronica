using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using SecretariaEletronica.Commands;
using SecretariaEletronica.Events.Client;
using SecretariaEletronica.Events.Commands;

namespace SecretariaEletronica
{
    public class Startup
    {
        public DiscordShardedClient Client { get; set; }
        public IReadOnlyDictionary<int, CommandsNextExtension> Commands { get; set; }
        public IReadOnlyDictionary<int, VoiceNextExtension> Voice { get; set; }
        public IReadOnlyDictionary<int, LavalinkExtension> LavaLink { get; set; }
        
        public async Task RunBotAsync()
        {
            string json;
            await using (FileStream fs = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "config.json")))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            ConfigJson cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);
            
            DiscordConfiguration cfg = new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,

                Intents = DiscordIntents.GuildMessages
                          | DiscordIntents.Guilds
                          | DiscordIntents.GuildMembers
                          | DiscordIntents.GuildBans
                          | DiscordIntents.GuildVoiceStates,

                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                LogTimestampFormat = "dd MMM yyy - hh:mm:ss"
            };
            
            Client = new DiscordShardedClient(cfg);

            Client.Ready += new Ready(Client).Client_Ready;
            Client.GuildAvailable += new GuildAvailable(Client).Client_GuildAvailable;
            Client.ClientErrored += new ClientErrored(Client).Client_ClientErrored;
            
            string[] assemblieList = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "CustomCommands"),
                "*.dll", SearchOption.AllDirectories);

            List<Type> typesToRegister = new List<Type>();
            
            foreach (string assemblyPath in assemblieList)
            {
                Assembly assembly = Assembly.LoadFile(assemblyPath);
                Type? type = assembly.GetType("SecretariaEletronica.CustomCommands.Main");
                typesToRegister.Add(type);
            }

            CommandsNextConfiguration commandcfg = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { cfgjson.CommandPrefix },
                EnableDms = true,
                EnableMentionPrefix = true
            };

            Commands = await Client.UseCommandsNextAsync(commandcfg);

            foreach (CommandsNextExtension cmdNext in Commands.Values)
            {
                cmdNext.CommandExecuted += new CommandExecuted().Commands_CommandExecuted;
                cmdNext.CommandErrored += new CommandErrored().Commands_CommandErrored;
                
                cmdNext.RegisterCommands<CustomCommands>();
                cmdNext.RegisterCommands<DrawningCommands>();
                cmdNext.RegisterCommands<LavaLinkCommands>();
                cmdNext.RegisterCommands<MiscCommands>();
                cmdNext.RegisterCommands<ModeratorCommands>();
                cmdNext.RegisterCommands<VoiceCommands>();
                
                foreach (Type type in typesToRegister)
                {
                    cmdNext.RegisterCommands(type);
                }
            }

            ConnectionEndpoint endpoint = new ConnectionEndpoint
            {
                Hostname = cfgjson.LavaLinkIp, // From your server configuration.
                Port = cfgjson.LavaLinkPort // From your server configuration
            };

            LavalinkConfiguration lavalinkConfig = new LavalinkConfiguration
            {
                Password = cfgjson.LavaLinkPass, // From your server configuration.
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };
            
            Voice = await Client.UseVoiceNextAsync(new VoiceNextConfiguration());
            LavaLink = await Client.UseLavalinkAsync();

            await this.Client.StartAsync();

            foreach (LavalinkExtension lava in LavaLink.Values)
            {
                await lava.ConnectAsync(lavalinkConfig);
            }
            
            await Task.Delay(-1);
        }
    }

    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }
        
        [JsonProperty("lavalink-ip")]
        public string LavaLinkIp { get; private set; }
        
        [JsonProperty("lavalink-port")]
        public int LavaLinkPort { get; private set; }
        
        [JsonProperty("lavalink-pass")]
        public string LavaLinkPass { get; private set; }
    }
}