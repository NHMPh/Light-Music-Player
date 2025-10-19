
using Accord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows;
using YoutubeExplode;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeSearchApi.Net.Models.Youtube;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using Video = YoutubeExplode.Videos.Video;

namespace NHMPh_music_player
{
    public class VideoInfo
    {
        //Private variable
        private string title;

        private string description;

        private string url;

        private string thumbnail;

        private int duration;

        private List<(double, string)> songLyrics = new List<(double, string)>();
        private ClosedCaptionTrack _songLyrics = null;

        public delegate void LyricsFoundEventHandler(object sender, bool status);

        public event LyricsFoundEventHandler OnLyricsFound;

        //Get set property

        public string Title { get { return title; } }
        public string Description { get { return description; } set { description = value; } }
        public string Url { get { return url; } }
        public string Thumbnail { get { return thumbnail; } }


        public List<(double, string)> SongLyrics { get { return songLyrics; } }

        public ClosedCaptionTrack _SongLyrics { get { return _songLyrics; } }


        //Constructor
        public VideoInfo()
        {

        }
        public VideoInfo(VideoInfo videoInfo)
        {
            this.title = videoInfo.title;
            this.description = videoInfo.description;
            this.url = videoInfo.url;
            this.thumbnail = videoInfo.thumbnail;
        }
        public VideoInfo(YoutubeVideo videoInfo)
        {
            this.title = videoInfo.Title;
            this.description = $"{videoInfo.Author} - {videoInfo.Duration}";
            this.url = videoInfo.Url;
            this.thumbnail = videoInfo.ThumbnailUrl;
        }
        public VideoInfo(Video videoInfo)
        {
            this.title = videoInfo.Title;
            this.description = videoInfo.Description;
            this.url = videoInfo.Url;
            this.thumbnail = videoInfo.Thumbnails.FirstOrDefault().Url;
        }
        public VideoInfo(string title, string description, string url, string thumbnail)
        {
            this.title = title;
            this.description = description;
            this.url = url;
            this.thumbnail = thumbnail;
        }
        //Method
        public async void GetFullDescription(StaticVisualUpdate staticVisualUpdate, YoutubeClient youtube)
        {


            var des = await youtube.Videos.GetAsync(this.url);
            duration = (int)des.Duration.Value.TotalSeconds;
            description += "\n" + des.Description;
            if (!MusicSetting.isLyrics)
                staticVisualUpdate.SetVisualDes(description);
        }
        public async void GetLyricsLib(MainWindow mainWindow)
        {
            var songName = Title;
            songName = StringUtilitiy.ProcessInvailName(songName);
            Console.WriteLine($"https://lrclib.net/api/search?q={songName}");
            // Create an instance of HttpClient
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    // Send GET request to a URL
                    HttpResponseMessage response = await client.GetAsync($"https://lrclib.net/api/search?q={songName}&duration={duration}");
                    Console.WriteLine($"https://lrclib.net/api/search?q={songName}&duration={duration}");
                    Console.WriteLine(1);
                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine(2);
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(3);
                        var arrayResponse = JArray.Parse(responseBody);
                        Console.WriteLine(4);
                        int smallestRange = int.MaxValue;
                        int index = 0;
                        for (int i = 0; i < arrayResponse.Count; i++)
                        {
                            var text = arrayResponse[i]["syncedLyrics"].ToString().Trim();
                            if (text == "") continue;
                            songLyrics = StringUtilitiy.ExtractAndParseTimestampsAndLyricsToMilliseconds(text);
                            Console.WriteLine($"Index: {i} LastSongTime: {songLyrics.Last().Item1} Song id {arrayResponse[i]["id"]}");
                            if (Math.Abs( (int)(songLyrics.Last().Item1) - (duration)) < smallestRange)
                            {
                                smallestRange = Math.Abs((int)(songLyrics.Last().Item1) - (duration));
                                index = i;
                            }


                        }
                        var _text = arrayResponse[0]["syncedLyrics"].ToString().Trim();
                        Console.WriteLine($"Index: {index} LastSongTime: {songLyrics.Last().Item1} Song id {arrayResponse[index]["id"]}");
                        songLyrics = StringUtilitiy.ExtractAndParseTimestampsAndLyricsToMilliseconds(_text);

                        Console.WriteLine(songLyrics.Count);
                        Console.WriteLine(songLyrics.First());
                        OnLyricsFound?.Invoke(this, true);
                        mainWindow.lyrics_btn.Width = 20;
                        mainWindow.lyricsSync_btn.Width = 10;

                    }
                    else
                    {
                        Console.WriteLine($"Failed to get data. Status code: {response.StatusCode}");
                        songLyrics = new List<(double, string)>();
                        mainWindow.lyrics_btn.Width = 0;
                        mainWindow.lyricsSync_btn.Width = 0;
                        MusicSetting.isLyrics = false;
                        MusicSetting.lyricsOffset = 0;
                        OnLyricsFound?.Invoke(this, false);

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

            }
        }
        public async void GetLyrics(MainWindow mainWindow, YoutubeClient youtube)
        {
            try
            {
                var videoUrl = url;
                var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoUrl);
                var trackInfo = trackManifest.GetByLanguage("en");
                if (trackInfo.IsAutoGenerated)
                {
                    _songLyrics = null;
                    GetLyricsLib(mainWindow);
                    return;
                }

                _songLyrics = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);
                OnLyricsFound?.Invoke(this, true);
                mainWindow.lyrics_btn.Width = 20;
                mainWindow.lyricsSync_btn.Width = 10;

            }
            catch (Exception ex)
            {
                _songLyrics = null;
                GetLyricsLib(mainWindow);
            }




        }
        public string GetLyricBytime(double seconds)
        {

            string line = "";
            if (songLyrics.Count > 0)
            {

                foreach (var lyric in songLyrics)
                {
                    if (lyric.Item1 < seconds)
                    {
                        line = lyric.Item2.ToString().Replace("\n", " ").Replace("\r", " ");
                        if (lyric.Item2.ToString() == "")
                            line = "[Music]";
                    }
                }
            }
            else
            {
                line = _songLyrics.GetByTime(TimeSpan.FromSeconds(seconds)).Text.Replace("\n", " ").Replace("\r", " ");
            }

            return line;
        }

    }
}
