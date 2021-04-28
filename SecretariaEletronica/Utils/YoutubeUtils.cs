using System.IO;
using VideoLibrary;

namespace SecretariaEletronica.Utils
{
    public class YoutubeUtils
    {
        public YouTubeVideo DownloadVideo(string url, string path)
        {
            var youtube = YouTube.Default;
            var vid = youtube.GetVideo(url);
            string videopath = Path.Combine(path, vid.FullName.Replace(" ", "-"));
            File.WriteAllBytes(videopath, vid.GetBytes());

            return vid;
        }
    }
}