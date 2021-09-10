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

#define LAVALINK

#if LAVALINK

using DSharpPlus.Net;
using System.Threading;
using System.Diagnostics;

#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
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
        public static DiscordShardedClient Client { get; private set; }
        public static IMongoDatabase Database { get; private set; }
        public static IReadOnlyDictionary<int, CommandsNextExtension> Commands { get; private set; }
        public static IReadOnlyDictionary<int, VoiceNextExtension> Voice { get; private set; }
        public static IReadOnlyDictionary<int, LavalinkExtension> LavaLink { get; private set; }
        public static ConfigJson Configuration { get; private set; }
        
        private static IMongoClient _mongoClient;

        public async Task RunBotAsync()
        {
            // Run LavaLink

#if LAVALINK

            Process llProcess = new Process()
            {
                StartInfo = new ProcessStartInfo("java",
                    $"-jar {Path.Combine(Directory.GetCurrentDirectory(), "Lavalink.jar")}")
                {
                    RedirectStandardOutput = true
                }
            };
            llProcess.Start();
            
            Thread.Sleep(5000);
            
#endif

            // Load configuration.
            
            string json;
            await using (FileStream fs = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "config.json")))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            Configuration = JsonConvert.DeserializeObject<ConfigJson>(json);
            
            // Setup client.
            
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

            Client.Ready += Ready.Client_Ready;
            Client.GuildAvailable += GuildAvailable.Client_GuildAvailable;
            Client.GuildCreated += GuildCreated.Client_GuildCreated;
            Client.GuildDeleted += GuildDeleted.Client_GuildDeleted;
            Client.ClientErrored += ClientErrored.Client_ClientErrored;

            // Load custom commands.
            
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
                    if (type?.BaseType == typeof(BaseCommandModule)) typesToRegister.Add(type);

                    MethodInfo methodInfo = type?.GetMethod("Load");
                    object o = Activator.CreateInstance(type);

                    methodInfo?.Invoke(o, new []{ Client });
                }
            }
            
            // Setup CommandsNext.

            CommandsNextConfiguration commandCfg = new CommandsNextConfiguration
            {
                StringPrefixes = Configuration.CommandPrefix,
                EnableDms = true,
                EnableMentionPrefix = true
            };

            Commands = await Client.UseCommandsNextAsync(commandCfg);

            foreach (CommandsNextExtension cmdNext in Commands.Values)
            {
                cmdNext.CommandExecuted += CommandExecuted.Commands_CommandExecuted;
                cmdNext.CommandErrored += CommandErrored.Commands_CommandErrored;
                
                cmdNext.RegisterCommands<CustomCommands>();
                cmdNext.RegisterCommands<DrawningCommands>();
                cmdNext.RegisterCommands<MiscCommands>();
                cmdNext.RegisterCommands<ModeratorCommands>();
                cmdNext.RegisterCommands<WaxCommands>();

#if LAVALINK
                cmdNext.RegisterCommands<VoiceCommands>();
                cmdNext.RegisterCommands<LavaLinkCommands>();
#endif
                
                foreach (Type type in typesToRegister)
                {
                    cmdNext.RegisterCommands(type);
                }
            }
            
            await Client.StartAsync();

#if LAVALINK
            
            // Setup LavaLink.

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

            foreach (LavalinkExtension lava in LavaLink.Values)
            {
                await lava.ConnectAsync(lavalinkConfig);
            }

#endif
            
            // Setup MongoDB.

            _mongoClient = new MongoClient(Configuration.MongoUrl);
            Database = _mongoClient.GetDatabase("SecretariaEletronica");
            
            await Task.Delay(-1);
        }
    }

    public struct ConfigJson
    {
        [JsonProperty("token")] public string Token;
        [JsonProperty("prefix")] public string[] CommandPrefix;
        [JsonProperty("log-ready")] public ulong LogReady;
        [JsonProperty("log-guild")] public ulong LogGuild;
        [JsonProperty("log-commands")] public ulong LogCommands;
        [JsonProperty("lavalink-ip")] public string LavaLinkIp;
        [JsonProperty("lavalink-port")] public int LavaLinkPort;
        [JsonProperty("lavalink-pass")] public string LavaLinkPass;
        [JsonProperty("rapid-api-key")] public string RapidApiKey;
        [JsonProperty("mongo-url")] public string MongoUrl;
    }
}