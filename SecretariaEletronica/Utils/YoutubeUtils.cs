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

using System.IO;
using VideoLibrary;

namespace SecretariaEletronica.Utils
{
    public class YoutubeUtils
    {
        /// <summary>
        /// Download video from youtube
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public YouTubeVideo DownloadVideo(string url, string path)
        {
            YouTube youtube = YouTube.Default;
            YouTubeVideo vid = youtube.GetVideo(url);
            string videopath = Path.Combine(path, vid.FullName.Replace(" ", "-"));
            File.WriteAllBytes(videopath, vid.GetBytes());

            return vid;
        }
    }
}