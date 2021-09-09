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

using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using SecretariaEletronica.Utils;
using VideoLibrary;

namespace SecretariaEletronica.Commands
{
    public class VoiceCommands : BaseCommandModule
    {
        [Command("joinVN"), Description("Joins a voice channel.")]
        public async Task Join(CommandContext ctx, DiscordChannel chn = null)
        {
            VoiceNextExtension vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            VoiceNextConnection vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                await ctx.RespondAsync("Already connected in this guild.");
                return;
            }

            DiscordVoiceState vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null && chn == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            if (chn == null)
                chn = vstat.Channel;

            await vnext.ConnectAsync(chn);
            await ctx.RespondAsync($"Connected to `{chn.Name}`");
        }

        [Command("leaveVN"), Description("Leaves a voice channel.")]
        public async Task Leave(CommandContext ctx)
        {
            VoiceNextExtension vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            VoiceNextConnection vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

            vnc.Disconnect();
            await ctx.RespondAsync("Disconnected");
        }

        [Command("song"), Description("Plays an audio file.")]
        public async Task Play(CommandContext ctx, [RemainingText, Description("Full path to the file to play.")] string path)
        {
            string filename = Path.Combine(Directory.GetCurrentDirectory(), "Audios", path);

            VoiceNextExtension vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            VoiceNextConnection vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

            if (!File.Exists(filename))
            {
                await ctx.RespondAsync($"File `{path}` does not exist.");
                return;
            }

            while (vnc.IsPlaying)
                await vnc.WaitForPlaybackFinishAsync();

            Exception exc = null;
            await ctx.Message.RespondAsync($"Playing `{path}`");

            try
            {
                await vnc.SendSpeakingAsync();

                Stream ffout = new FfmpegUtils().GetffmpegStream(filename);

                VoiceTransmitSink txStream = vnc.GetTransmitSink();
                await ffout.CopyToAsync(txStream);
                await txStream.FlushAsync();
                await vnc.WaitForPlaybackFinishAsync();
            }
            catch (Exception ex)
            {
                exc = ex;
            }
            finally
            {
                await vnc.SendSpeakingAsync(false);
                await ctx.Message.RespondAsync($"Finished playing `{path}`");
            }

            if (exc != null)
                await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
        }

        [Command("audios"), Description("List Audios"), RequireOwner]
        public async Task ListAudios(CommandContext ctx)
        {
            string message = "Audios:" + Environment.NewLine;
            FileInfo[] path = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Audios")).GetFiles();
            foreach (FileInfo fileInfo in path)
            {
                message += $"`{fileInfo.Name}`{Environment.NewLine}";
            }

            await ctx.RespondAsync(message);
        }

        [Command("youget"), Description("Get Audio from youtube")]
        public async Task YoutubeAudio(CommandContext ctx , [RemainingText, Description("url")] string url)
        {
            await ctx.Channel.SendMessageAsync("Baixando...");

            try
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "Audios");
                YouTubeVideo vid = new YoutubeUtils().DownloadVideo(url, path);
                string videopath = Path.Combine(path, vid.FullName.Replace(" ", "-"));
                
                string output = Path.Combine(path, vid.Title.Replace(" ", "-")) + ".ogg";
                new FfmpegUtils().ConvertAndDelete(videopath, output, ctx);

                await ctx.Channel.SendMessageAsync("Download terminado. " + vid.FullName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}