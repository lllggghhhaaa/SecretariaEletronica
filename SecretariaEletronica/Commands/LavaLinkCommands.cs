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
        private Dictionary<ulong, LavalinkTrack> _guildTracks = new();
        private Dictionary<ulong, bool> _loopEnabled = new();
        
        [Command("join"), Description("Join to channel with LavaLink")]
        public async Task JoinLavaLink(CommandContext ctx, DiscordChannel channel = null)
        {
            DiscordVoiceState vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null && channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            if (channel == null)
                channel = vstat.Channel;
            
            LavalinkExtension lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            LavalinkNodeConnection node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            await node.ConnectAsync(channel);
            
            LavalinkGuildConnection conn = node.GetGuildConnection(channel.Guild);
            conn.PlaybackFinished += ConnOnPlaybackFinished;

            await ctx.RespondAsync($"Joined {channel.Name}!");
        }

        [Command("leave")]
        public async Task LeaveLavaLink(CommandContext ctx, DiscordChannel channel = null)
        {
            DiscordVoiceState vstat = ctx.Member?.VoiceState;
            
            if (vstat?.Channel == null && channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }
            
            if (channel == null)
                channel = vstat.Channel;
            
            LavalinkExtension lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            LavalinkNodeConnection node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            LavalinkGuildConnection conn = node.GetGuildConnection(channel.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            await conn.DisconnectAsync();
            await ctx.RespondAsync($"Left {channel.Name}!");
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
            LavalinkGuildConnection conn = node?.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await node?.ConnectAsync(channel);
                conn = node?.GetGuildConnection(channel?.Guild);

                if (conn != null) conn.PlaybackFinished += ConnOnPlaybackFinished;
                else return;
            }

            if (node?.Rest != null)
            {
                LavalinkLoadResult loadResult = await node.Rest.GetTracksAsync(search);

                if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
                {
                    await ctx.RespondAsync($"Track search failed `{search}`.");
                    return;
                }

                LavalinkTrack track = loadResult.Tracks.First();

                await conn.PlayAsync(track);

                if (_guildTracks.ContainsKey(ctx.Guild.Id))
                {
                    _guildTracks[ctx.Guild.Id] = track;
                }
                else
                {
                    _guildTracks.Add(ctx.Guild.Id, track);
                }

                await ctx.RespondAsync($"Now playing `{track.Title}`");
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

            await ctx.RespondAsync($"loop enabled: `{enabled}`");
        }
        
        private Task ConnOnPlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            ulong guildId = sender.Guild.Id;
            if (!_loopEnabled.ContainsKey(guildId)) return Task.CompletedTask;
            bool isEnabled = _loopEnabled[guildId];

            if (isEnabled)
            {
                sender.PlayAsync(_guildTracks[guildId]);
            }
            else
            {
                _guildTracks.Remove(guildId);
            }
            
            return Task.CompletedTask;
        }
    }
}