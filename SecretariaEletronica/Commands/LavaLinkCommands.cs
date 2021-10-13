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

using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace SecretariaEletronica.Commands;

public class LavaLinkCommands : BaseCommandModule
{
    private static Dictionary<ulong, List<LavalinkTrack>> _guildTracks = new();
    private static Dictionary<ulong, bool> _loopEnabled = new();

    private Dictionary<bool, string> _loopMessage = new()
    {
        {false, "disabled"},
        {true, "enabled"}
    };

    [Command("join"), Description("Join to channel with LavaLink")]
    public async Task JoinLavaLink(CommandContext ctx)
    {
        DiscordVoiceState vstat = ctx.Member?.VoiceState;
        
        if (vstat?.Channel is null)
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

        if (vstat.Channel.Type is not ChannelType.Voice)
        {
            await ctx.RespondAsync("Not a valid voice channel.");
            return;
        }

        await node.ConnectAsync(vstat.Channel);

        node.GetGuildConnection(vstat.Channel.Guild).PlaybackFinished +=
            (sender, _) => ConnOnPlaybackFinished(sender, false);

        await ctx.RespondAsync($"Joined {vstat.Channel.Name}!");
    }

    [Command("leave")]
    public async Task LeaveLavaLink(CommandContext ctx)
    {
        DiscordVoiceState vstat = ctx.Member?.VoiceState;

        if (vstat?.Channel is null && vstat?.Channel is null)
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

        if (vstat.Channel.Type is not ChannelType.Voice)
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

        if (ctx.Member?.VoiceState is null || ctx.Member.VoiceState.Channel is null)
        {
            await ctx.RespondAsync("You are not in a voice channel.");
            return;
        }

        LavalinkExtension lava = ctx.Client.GetLavalink();
        LavalinkNodeConnection node = lava.ConnectedNodes.Values.FirstOrDefault();

        if (node is null)
        {
            await ctx.RespondAsync("Lavalink is not connected.");
            return;
        }

        LavalinkGuildConnection conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

        if (conn is null)
        {
            await node.ConnectAsync(channel);
            conn = node.GetGuildConnection(channel?.Guild);

            if (conn is not null) conn.PlaybackFinished += (sender, _) => ConnOnPlaybackFinished(sender, false);
            else return;
        }

        if (node.Rest is not null)
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
                _guildTracks.Add(ctx.Guild.Id, new List<LavalinkTrack> {track});
                await conn.PlayAsync(track);
                await ctx.RespondAsync($"Now playing `{track.Title}`");
            }
        }
    }

    [Command("search")]
    public async Task Search(CommandContext ctx, [RemainingText] string query)
    {
        LavalinkExtension lava = ctx.Client.GetLavalink();
        LavalinkNodeConnection node = lava.ConnectedNodes.Values.FirstOrDefault();

        if (node is null)
        {
            await ctx.RespondAsync("Lavalink is not connected.");
            return;
        }

        if (node.Rest is not null)
        {
            LavalinkLoadResult loadResult = await node.Rest.GetTracksAsync(query);

            if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed `{query}`.");
                return;
            }

            List<DiscordSelectComponentOption> options = new List<DiscordSelectComponentOption>();

            for (int i = 0; i < Math.Clamp(loadResult.Tracks.Count(), 0, 10); i++)
            {
                LavalinkTrack track = loadResult.Tracks.ElementAt(i);

                options.Add(new DiscordSelectComponentOption($"{i} - {track.Title}",
                    $"{i} - {track.Title}"));
            }

            DiscordSelectComponent dropdown =
                new DiscordSelectComponent("sdd", "Select the track", options, false, 1, options.Count);

            DiscordMessageBuilder builder = new DiscordMessageBuilder()
                .WithContent("Select the track")
                .AddComponents(dropdown);

            await builder.SendAsync(ctx.Channel);
        }
    }

    [Command("pause"), Description("Pause music with LavaLink")]
    public async Task Pause(CommandContext ctx)
    {
        if (ctx.Member.VoiceState is null || ctx.Member.VoiceState.Channel is null)
        {
            await ctx.RespondAsync("You are not in a voice channel.");
            return;
        }

        LavalinkExtension lava = ctx.Client.GetLavalink();
        LavalinkNodeConnection node = lava.ConnectedNodes.Values.First();
        LavalinkGuildConnection conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

        if (conn is null)
        {
            await ctx.RespondAsync("Lavalink is not connected.");
            return;
        }

        if (conn.CurrentState.CurrentTrack is null)
        {
            await ctx.RespondAsync("There are no tracks loaded.");
            return;
        }

        await conn.PauseAsync();
    }

    [Command("nowplaying"), Aliases("playing", "np")]
    public async Task NowPlaying(CommandContext ctx, int count = 1)
    {
        if (ctx.Member.VoiceState is null || ctx.Member.VoiceState.Channel is null)
        {
            await ctx.RespondAsync("You are not in a voice channel.");
            return;
        }

        LavalinkExtension lava = ctx.Client.GetLavalink();
        LavalinkNodeConnection node = lava.ConnectedNodes.Values.First();
        LavalinkGuildConnection conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

        if (conn is null)
        {
            await ctx.RespondAsync("Lavalink is not connected.");
            return;
        }

        if (conn.CurrentState.CurrentTrack is null)
        {
            await ctx.RespondAsync("There are no tracks loaded.");
            return;
        }

        StringBuilder tracks = new StringBuilder("``\n");

        for (int i = 0; i < Math.Clamp(_guildTracks[ctx.Guild.Id].Count, 0, count); i++)
        {
            tracks.Append(_guildTracks[ctx.Guild.Id][i].Title + "\n");
        }

        tracks.Append("``");

        await ctx.RespondAsync($"Now playing: `{tracks}`");
    }

    [Command("skip"), Description("Skip the track")]
    public async Task Skip(CommandContext ctx)
    {
        if (ctx.Member.VoiceState is null || ctx.Member.VoiceState.Channel is null)
        {
            await ctx.RespondAsync("You are not in a voice channel.");
            return;
        }

        LavalinkExtension lava = ctx.Client.GetLavalink();
        LavalinkNodeConnection node = lava.ConnectedNodes.Values.First();
        LavalinkGuildConnection conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

        if (conn is null)
        {
            await ctx.RespondAsync("Lavalink is not connected.");
            return;
        }

        if (conn.CurrentState.CurrentTrack is null)
        {
            await ctx.RespondAsync("There are no tracks loaded.");
            return;
        }

        await ConnOnPlaybackFinished(conn, true);

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

    public static async Task ConnOnPlaybackFinished(LavalinkGuildConnection sender, bool forceSkip)
    {
        ulong guildId = sender.Guild.Id;
        if (!_loopEnabled.ContainsKey(guildId)) _loopEnabled.Add(guildId, false);
        bool isEnabled = _loopEnabled[guildId];

        if (isEnabled && !forceSkip)
        {
            LavalinkTrack track = _guildTracks[guildId][0];
            _guildTracks[guildId].RemoveAt(0);
            _guildTracks[guildId].Add(track);
            
            await sender.PlayAsync(_guildTracks[guildId][0]);
        }
        else if (_guildTracks[guildId].Count < 2)
            _guildTracks.Remove(guildId);
        else
        {
            _guildTracks[guildId].RemoveAt(0);

            await sender.PlayAsync(_guildTracks[guildId][0]);
        }
    }

    public static async Task AddTracks(List<LavalinkTrack> tracks, DiscordClient client, ulong id)
    {
        LavalinkExtension lava = client.GetLavalink();
        LavalinkNodeConnection node = lava.ConnectedNodes.Values.First();
        LavalinkGuildConnection conn = node.GetGuildConnection(client.GetGuildAsync(id).Result);
            
        if (!_guildTracks.ContainsKey(id)) 
            _guildTracks.Add(id, tracks);
        else
            _guildTracks[id].AddRange(tracks);

        if (conn.CurrentState.CurrentTrack is null)
        {
            await conn.PlayAsync(_guildTracks[id][0]);
        }
    }
}