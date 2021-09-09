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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace SecretariaEletronica.Commands
{
    public class LavaLinkCommands : BaseCommandModule
    {
        private Dictionary<ulong, List<LavalinkTrack>> _guildTracks = new();
        private Dictionary<ulong, bool> _loopEnabled = new();

        private Dictionary<bool, string> _loopMessage = new()
        {
            { false, "disabled" },
            { true, "enabled" }
        };

        [Command("join"), Description("Join to channel with LavaLink")]
        public async Task JoinLavaLink(CommandContext ctx)
        {
            DiscordVoiceState vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            LavalinkExtension lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            LavalinkNodeConnection node = lava.ConnectedNodes.Values.First();

            if (vstat.Channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            await node.ConnectAsync(vstat.Channel);
            
            node.GetGuildConnection(vstat.Channel.Guild).PlaybackFinished += (sender, args) => ConnOnPlaybackFinished(sender, args, false);;

            await ctx.RespondAsync($"Joined {vstat.Channel.Name}!");
        }

        [Command("leave")]
        public async Task LeaveLavaLink(CommandContext ctx)
        {
            DiscordVoiceState vstat = ctx.Member?.VoiceState;
            
            if (vstat?.Channel == null && vstat.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }
            
            LavalinkExtension lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            LavalinkNodeConnection node = lava.ConnectedNodes.Values.First();

            if (vstat.Channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            LavalinkGuildConnection conn = node.GetGuildConnection(vstat.Channel.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            _guildTracks.Remove(ctx.Guild.Id);
            await conn.DisconnectAsync();
            await ctx.RespondAsync($"Left {vstat.Channel.Name}!");
        }
        
        [Command("play"), Description("Play music with LavaLink"), Aliases("p")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            DiscordVoiceState vstat = ctx.Member?.VoiceState;
            DiscordChannel channel = vstat?.Channel;
            
            if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)  
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }
            LavalinkExtension lava = ctx.Client.GetLavalink();
            LavalinkNodeConnection node = lava.ConnectedNodes.Values.FirstOrDefault();

            if (node == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }
            
            LavalinkGuildConnection conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await node.ConnectAsync(channel);
                conn = node.GetGuildConnection(channel?.Guild);

                if (conn != null) conn.PlaybackFinished += (sender, args) => ConnOnPlaybackFinished(sender, args, false);
                else return;
            }

            if (node.Rest != null)
            {
                LavalinkLoadResult loadResult = await node.Rest.GetTracksAsync(search);

                if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
                {
                    await ctx.RespondAsync($"Track search failed `{search}`.");
                    return;
                }

                LavalinkTrack track = loadResult.Tracks.First();

                if (_guildTracks.ContainsKey(ctx.Guild.Id))
                {
                    _guildTracks[ctx.Guild.Id].Add(track);
                    await ctx.RespondAsync($"Queued `{track.Title}`");
                }
                else
                {
                    _guildTracks.Add(ctx.Guild.Id, new List<LavalinkTrack> { track });
                    await conn.PlayAsync(track);
                    await ctx.RespondAsync($"Now playing `{track.Title}`");
                }
            }
        }

        [Command("pause"), Description("Pause music with LavaLink")]
        public async Task Pause(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            LavalinkExtension lava = ctx.Client.GetLavalink();
            LavalinkNodeConnection node = lava.ConnectedNodes.Values.First();
            LavalinkGuildConnection conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.PauseAsync();
        }

        [Command("skip"), Description("Skip the track")]
        public async Task Skip(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }
            
            LavalinkExtension lava = ctx.Client.GetLavalink();
            LavalinkNodeConnection node = lava.ConnectedNodes.Values.First();
            LavalinkGuildConnection conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            
            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await ConnOnPlaybackFinished(conn, null, true);

            await ctx.RespondAsync("Track skiped");
        }

        [Command("loop"), Description("Enable/Disable loop")]
        public async Task Loop(CommandContext ctx)
        {
            ulong guildId = ctx.Guild.Id;
            bool enabled;
            
            if (_loopEnabled.ContainsKey(guildId))
            {
                enabled = !_loopEnabled[guildId];
                _loopEnabled[guildId] = enabled;
            }
            else
            {
                _loopEnabled.Add(guildId, true);
                enabled = true;
            }

            await ctx.RespondAsync($"loop `{_loopMessage[enabled]}`");
        }
        
        private async Task ConnOnPlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e, bool forceSkip)
        {
            ulong guildId = sender.Guild.Id;
            if (!_loopEnabled.ContainsKey(guildId)) _loopEnabled.Add(guildId, false);
            bool isEnabled = _loopEnabled[guildId];

            if (isEnabled && !forceSkip)
                await sender.PlayAsync(_guildTracks[guildId][0]);
            else
            {
                if (_guildTracks.Count <= 1)
                {
                    _guildTracks.Remove(guildId);
                    return;
                }
                _guildTracks[guildId].RemoveAt(0);
                
                await sender.PlayAsync(_guildTracks[guildId][0]);
            }
        }
    }
}