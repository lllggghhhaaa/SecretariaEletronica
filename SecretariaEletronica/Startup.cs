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
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using SecretariaEletronica.Commands;
using SecretariaEletronica.Events.Client;
using SecretariaEletronica.Events.Commands;

namespace SecretariaEletronica
{
    public class Startup
    {
        public static DiscordShardedClient Client { get; private set; }
        private static IMongoClient MongoClient;
        public static IMongoDatabase Database { get; private set; }
        public static IReadOnlyDictionary<int, CommandsNextExtension> Commands { get; private set; }
        public static IReadOnlyDictionary<int, VoiceNextExtension> Voice { get; private set; }
        public static IReadOnlyDictionary<int, LavalinkExtension> LavaLink { get; private set; }
        public static ConfigJson Configuration { get; private set; }
        
        public async Task RunBotAsync()
        {
            // read config
            
            string json;
            await using (FileStream fs = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "config.json")))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            Configuration = JsonConvert.DeserializeObject<ConfigJson>(json);
            
            // setup client
            
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

            // load custom commands
            
            List<Type> typesToRegister = new List<Type>();
            
            if(!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "CustomCommands")))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "CustomCommands");
            }
            else
            {
                string[] assemblyList = Directory.GetFiles(
                    Path.Combine(Directory.GetCurrentDirectory(), "CustomCommands"),
                    "*.dll", SearchOption.AllDirectories);

                foreach (string assemblyPath in assemblyList)
                {
                    Assembly assembly = Assembly.LoadFile(assemblyPath);
                    Type type = assembly.GetType("SecretariaEletronica.CustomCommands.Main");
                    typesToRegister.Add(type);
                }
            }
            
            // setup commandsnext

            CommandsNextConfiguration commandCfg = new CommandsNextConfiguration
            {
                StringPrefixes = Configuration.CommandPrefix,
                EnableDms = true,
                EnableMentionPrefix = true
            };

            Commands = await Client.UseCommandsNextAsync(commandCfg);

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
            
            // setup lavalink

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

            await Client.StartAsync();

            foreach (LavalinkExtension lava in LavaLink.Values)
            {
                await lava.ConnectAsync(lavalinkConfig);
            }
            
            // setup mongodb

            MongoClient = new MongoClient(Configuration.MongoUrl);
            Database = MongoClient.GetDatabase("SecretariaEletronica");
            
            await Task.Delay(-1);
        }
    }

    public struct ConfigJson
    {
        [JsonProperty("token")] public string Token;
        [JsonProperty("prefix")] public string[] CommandPrefix;
        [JsonProperty("lavalink-ip")] public string LavaLinkIp;
        [JsonProperty("lavalink-port")] public int LavaLinkPort;
        [JsonProperty("lavalink-pass")] public string LavaLinkPass;
        [JsonProperty("rapid-api-key")] public string RapidApiKey;
        [JsonProperty("mongo-url")] public string MongoUrl;
    }
}