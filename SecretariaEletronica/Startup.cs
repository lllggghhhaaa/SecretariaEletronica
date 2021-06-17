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
        public static ConfigJson Configuration { get; private set; }
        
        public async Task RunBotAsync()
        {
            string json;
            await using (FileStream fs = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "config.json")))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            Configuration = JsonConvert.DeserializeObject<ConfigJson>(json);
            
            DiscordConfiguration cfg = new DiscordConfiguration
            {
                Token = Configuration.Token,
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

            List<Type> typesToRegister = new List<Type>();
            
            if(!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "CustomCommands")))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "CustomCommands");
            }
            else
            {
                string[] assemblieList = Directory.GetFiles(
                    Path.Combine(Directory.GetCurrentDirectory(), "CustomCommands"),
                    "*.dll", SearchOption.AllDirectories);

                foreach (string assemblyPath in assemblieList)
                {
                    Assembly assembly = Assembly.LoadFile(assemblyPath);
                    Type type = assembly.GetType("SecretariaEletronica.CustomCommands.Main");
                    typesToRegister.Add(type);
                }
            }

            CommandsNextConfiguration commandcfg = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { Configuration.CommandPrefix },
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
                cmdNext.RegisterCommands<WaxCommands>();
                
                foreach (Type type in typesToRegister)
                {
                    cmdNext.RegisterCommands(type);
                }
            }

            ConnectionEndpoint endpoint = new ConnectionEndpoint
            {
                Hostname = Configuration.LavaLinkIp,
                Port = Configuration.LavaLinkPort
            };

            LavalinkConfiguration lavalinkConfig = new LavalinkConfiguration
            {
                Password = Configuration.LavaLinkPass,
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
        [JsonProperty("token")] public string Token;

        [JsonProperty("prefix")] public string CommandPrefix;

        [JsonProperty("lavalink-ip")] public string LavaLinkIp;

        [JsonProperty("lavalink-port")] public int LavaLinkPort;

        [JsonProperty("lavalink-pass")] public string LavaLinkPass;

        [JsonProperty("rapid-api-key")] public string RapidApiKey;
    }
}