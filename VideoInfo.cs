using AngleSharp.Media;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeDLSharp;
using YoutubeExplode.Videos;
using YoutubeSearchApi.Net.Models.Youtube;

namespace NHMPh_music_player
{
    public class VideoInfo
    {
        //Private variable
        private string title;

        private string description;

        private string url;

        private string thumbnail;

        private JArray songLyrics = new JArray();
        

        //Get set property

        public string Title { get { return title; } }
        public string Description { get { return description; } set { description = value; } }
        public string Url { get { return url; } }
        public string Thumbnail { get { return thumbnail; }  }

        public JArray SongLyrics { get { return songLyrics; } }
       
        
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
        public async void GetFullDescription(MainWindow mainWindow)
        {
            YoutubeDL ytdl = new YoutubeDL();
            var des = await ytdl.RunVideoDataFetch(this.url);
            description += "\n" + des.Data.Description;
            mainWindow.SetVisualDes(description);
        }
        public async void GetLyrics(MainWindow mainWindow)
        {
            var songName = Title;
            songName = StringUtilitiy.ProcessInvailName(songName);
            Console.WriteLine($"https://api.textyl.co/api/lyrics?q={songName}");
            // Create an instance of HttpClient
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Send GET request to a URL
                    HttpResponseMessage response = await client.GetAsync($"https://api.textyl.co/api/lyrics?q={songName}");

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the content as string
                        string responseBody = await response.Content.ReadAsStringAsync();
                        songLyrics = JArray.Parse(responseBody);
                        mainWindow.lyrics_btn.Width = 20;
                        mainWindow.lyricsSync_btn.Width = 10;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get data. Status code: {response.StatusCode}");
                        songLyrics =null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}
