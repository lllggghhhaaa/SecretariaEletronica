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

using System.Diagnostics;
using System.IO;
using DSharpPlus.CommandsNext;

namespace SecretariaEletronica.Utils
{
    public class FfmpegUtils
    {
        /// <summary>
        /// Convert the midea and delete the old midea
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="ctx"></param>
        public async void ConvertAndDelete(string inputPath, string outputPath, CommandContext ctx)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $@"-i {inputPath} -vn -acodec libvorbis -y {outputPath}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            Process ffmpeg = Process.Start(psi);
            
            await ffmpeg?.WaitForExitAsync();

            await ctx.RespondAsync("Convertido para: " + outputPath);
            File.Delete(inputPath);
        }

        /// <summary>
        /// Get the stream of audio file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public Stream GetffmpegStream(string file)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $@"-i ""{file}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            Process ffmpeg = Process.Start(psi);
            Stream ffout = ffmpeg?.StandardOutput.BaseStream;

            return ffout;
        }
    }
}