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