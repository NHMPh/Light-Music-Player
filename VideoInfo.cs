using Accord;
using AngleSharp.Io;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.ClosedCaptions;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models.Info;
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

        private string artist;

        private string album;

        private bool squareThumbnail;

        public string rawLyrics { get { return songLyricsRaw; } set { songLyricsRaw = value; } }

        private List<(double, string)> songLyrics = new List<(double, string)>();
        private string songLyricsRaw = "";
        private ClosedCaptionTrack _songLyrics = null;

        public delegate void LyricsFoundEventHandler(object sender, bool status);

        public event LyricsFoundEventHandler OnLyricsFound;

        //Get set property

        public string Title { get { return title; } }
        public string Description { get { return description; } set { description = value; } }
        public string Url { get { return url; } }
        public string Thumbnail { get { return thumbnail; } }

        public int Duration { get { return duration; } set { duration = value; } }

        public string Artist { get { return artist; } set { artist = value; } }

        public string Album { get { return album; } set { album = value; } }

        public bool SquareThumbnail { get { return squareThumbnail; } set { squareThumbnail = value; } }

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
        public VideoInfo(string title, string description, string url, string thumbnail, string artist, string album, bool squareThumbnail)
        {
            this.title = title;
            this.description = description;
            this.url = url;
            this.thumbnail = thumbnail;
            this.artist = artist;
            this.album = album;
            this.squareThumbnail = squareThumbnail;
        }
        public async Task<string> GetHighResThumbnail(YoutubeClient youtube)
        {
            var thumb = await youtube.Videos.GetAsync(this.url);

            return thumb.Thumbnails.GetWithHighestResolution().Url;
        }
        //Method
        private string songtitle;
        public async Task GetFullDescription(StaticVisualUpdate staticVisualUpdate, YoutubeClient youtube)
        {
            // var des = await youtube.Videos.GetAsync(this.url);
            HttpClient httpClient = new HttpClient();

            YouTubeMusicClient yt = new YouTubeMusicClient("US", null, null, null,null , httpClient);
            SongVideoInfo info = await yt.GetSongVideoInfoAsync(StringUtilitiy.ExtractId(url));
            Console.WriteLine($"{info.Name}, {string.Join(", ", info.Artists.Select(artist => artist.Name))} - {info.Description}");

            foreach (var thu in info.Thumbnails)
            {
                Console.WriteLine(thu.Url);
            }
            songtitle = info.Name;
            artist = string.Join(", ", info.Artists.Select(artist => artist.Name));
            album = info.Album != null ? info.Album.Name : "";
            thumbnail = info.Thumbnails.Last().Url;
            duration = (int)info.Duration.TotalSeconds;
            description += "\n" + info.Description;
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
                    Console.WriteLine(songtitle);
                    Console.WriteLine(Artist);
                    Console.WriteLine(Album);
                    string searhurl = "";
                    // Send GET request to a URL
                    songtitle = StringUtilitiy.ProcessInvailName(songtitle);
                    songtitle = Regex.Replace(songtitle, @"\((?=.{1,2}\))[^)]*\)", "");
                    searhurl = $"https://lrclib.net/api/search?track_name={songtitle}";
                    if (artist != null && artist != "")
                    {
                        searhurl += $"&artist_name={artist.Replace(" ", "%20")}";
                    }



                    HttpResponseMessage response = await client.GetAsync(searhurl);
                    Console.WriteLine(searhurl);
                    Console.WriteLine(1);
                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine(2);
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(responseBody);


                        JToken jsonResponse = JToken.Parse(responseBody);
                        JArray arrayResponse;
                        if (jsonResponse is JArray jArray)
                        {
                            arrayResponse = jArray;
                        }
                        else if (jsonResponse is JObject jObject)
                        {
                            arrayResponse = new JArray(jObject);
                        }
                        else
                        {
                            arrayResponse = new JArray();
                        }

                        Console.WriteLine(4);

                        int index = 0;
                        for (int i = 0; i < arrayResponse.Count; i++)
                        {
                            var text = arrayResponse[i]["syncedLyrics"].ToString().Trim();
                            var rtext = arrayResponse[i]["plainLyrics"].ToString().Trim();

                            Console.WriteLine($"Index{index} lyris:{text.Length}");
                            var ablbumName = arrayResponse[i]["albumName"].ToString().Trim();
                            if ( rtext.Length!=0)
                            {
                                rawLyrics = rtext;
                                OnLyricsFound?.Invoke(this, true);
                                mainWindow.lyrics_btn.Width = 20;
                            }
                            if (text.Length == 0)
                            {
                                index++;
                                continue;
                            }
                            if (ablbumName != album)
                            {
                                index++;
                                continue;
                            }



                            break;

                        }
                        Console.WriteLine($"Index {index}");
                        var _text = arrayResponse[index == 0 ? index : index - 1]["syncedLyrics"].ToString().Trim();

                        songLyrics = StringUtilitiy.ExtractAndParseTimestampsAndLyricsToMilliseconds(_text);

                        Console.WriteLine(songLyrics.Count);
                        Console.WriteLine(songLyrics.First());
                        OnLyricsFound?.Invoke(this, true);
                        mainWindow.lyrics_btn.Width = 20;
                        mainWindow.lyricsSync_btn.Width = 10;
                        mainWindow.lyricsBox.Text = "Intermisson";

                    }
                    else
                    {
                        Console.WriteLine($"Failed to get data. Status code: {response.StatusCode}");
                        songLyrics = new List<(double, string)>();
                        mainWindow.lyrics_btn.Width = 0;
                        mainWindow.lyricsSync_btn.Width = 0;
                        MusicSetting.isLyrics = false;
                        MusicSetting.lyricsOffset = 0;
                        mainWindow.lyricsTextBox.Height = 0;
                        OnLyricsFound?.Invoke(this, false);

                    }


                }
                catch (Exception ex)
                {
                    try
                    {
                        Console.WriteLine($"An error occurred: {ex}");
                        Console.WriteLine(songtitle);
                        Console.WriteLine(Artist);
                        string searhurl = "";
                        // Send GET request to a URL
                        songtitle = StringUtilitiy.ProcessInvailName(songtitle);
                        songtitle = Regex.Replace(songtitle, @"\((?=.{1,2}\))[^)]*\)", "");
                        searhurl = $"https://lrclib.net/api/search?q={songtitle}";
                        if (artist != null && artist != "")
                        {
                            searhurl += $"&artist_name={artist.Replace(" ", "%20")}";
                        }



                        HttpResponseMessage response = await client.GetAsync(searhurl);
                        Console.WriteLine(searhurl);
                        Console.WriteLine(1);
                        // Check if the response is successful
                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine(2);
                            if (response.Content == null) return;
                            string responseBody = await response.Content.ReadAsStringAsync();
                            Console.WriteLine(responseBody);


                            JToken jsonResponse = JToken.Parse(responseBody);
                            JArray arrayResponse;
                            if (jsonResponse is JArray jArray)
                            {
                                arrayResponse = jArray;
                            }
                            else if (jsonResponse is JObject jObject)
                            {
                                arrayResponse = new JArray(jObject);
                            }
                            else
                            {
                                arrayResponse = new JArray();
                            }

                            Console.WriteLine(4);
                            int smallestRange = int.MaxValue;
                            int index = 0;
                            for (int i = 0; i < arrayResponse.Count; i++)
                            {
                                var text = arrayResponse[i]["syncedLyrics"].ToString().Trim();
                                var ablbumName = arrayResponse[i]["albumName"].ToString().Trim();
                                var artitstName = arrayResponse[i]["artistName"].ToString().Trim();
                                var trackname = arrayResponse[i]["trackName"].ToString().Trim();
                                Console.WriteLine($"Index{index} lyris:{text.Length}");

                                if (text.Length == 0)
                                {
                                    Console.WriteLine("Null lyrics");
                                    index++;
                                    continue;
                                }
                                Console.WriteLine($"Album Name: {ablbumName} vs {album} {trackname.ToLower() != songtitle.ToLower()}");
                                Console.WriteLine(trackname.ToLower());
                                Console.WriteLine(songtitle.ToLower());
                                if (trackname.ToLower().Replace(" ","%20") != songtitle.ToLower())
                                {
                                    index++;
                                    continue;
                                }
                                if (artist != artitstName)
                                {
                                    index++;
                                    continue;
                                }

                                songLyrics = StringUtilitiy.ExtractAndParseTimestampsAndLyricsToMilliseconds(text);
                                Console.WriteLine($"Index: {i} LastSongTime: {songLyrics.Last().Item1} Song id {arrayResponse[i]["id"]}");
                                if (Math.Abs((int)(songLyrics.Last().Item1) - (duration)) < smallestRange)
                                {
                                    smallestRange = Math.Abs((int)(songLyrics.Last().Item1) - (duration));
                                    //index = i;
                                }
                                break;

                            }
                            Console.WriteLine($"Index {index}");
                            var _text = arrayResponse[index]["syncedLyrics"].ToString().Trim();
                            Console.WriteLine($"Index: {index} LastSongTime: {songLyrics.Last().Item1} Song id {arrayResponse[index]["id"]}");
                            songLyrics = StringUtilitiy.ExtractAndParseTimestampsAndLyricsToMilliseconds(_text);

                            Console.WriteLine(songLyrics.Count);
                            Console.WriteLine(songLyrics.First());
                            OnLyricsFound?.Invoke(this, true);
                            mainWindow.lyrics_btn.Width = 20;
                            mainWindow.lyricsSync_btn.Width = 10;
                            mainWindow.lyricsBox.Text = "Intermisson";

                        }
                        else
                        {
                            Console.WriteLine($"Failed to get data. Status code: {response.StatusCode}");
                            songLyrics = new List<(double, string)>();
                            mainWindow.lyrics_btn.Width = 0;
                            mainWindow.lyricsSync_btn.Width = 0;
                            MusicSetting.isLyrics = false;
                            MusicSetting.lyricsOffset = 0;
                            mainWindow.lyricsTextBox.Height = 0;
                            OnLyricsFound?.Invoke(this, false);

                        }
                    }
                    catch {
                        if (rawLyrics.Length != 0)
                        {
                            if(MusicSetting.isLyrics)
                            mainWindow.lyricsTextBox.Height = 76;
                            return;
                        }
                       
                        songLyrics = new List<(double, string)>();
                        mainWindow.lyrics_btn.Width = 0;
                        mainWindow.lyricsSync_btn.Width = 0;
                        MusicSetting.isLyrics = false;
                        MusicSetting.lyricsOffset = 0;
                        mainWindow.lyricsTextBox.Height = 0;
                        if (mainWindow.spectrum_ctn.Height == 0)
                            mainWindow.description.Height = 76;
                        else
                            mainWindow.description.Height = 16;
                        OnLyricsFound?.Invoke(this, false);
                    }
                   
                }
                Console.WriteLine("CHECK RAW LYRICS"+ rawLyrics != null);
              
                

            }
        }
        private static readonly HttpClient client = new HttpClient();
        public async Task<string> GetLyricsAsync(
    string videoId,
    string song,
    string artist,
    int duration,
    string album,
    string token)
        {
            const string url = "https://lyrics.api.dacubeking.com/v2/lyrics";

            var data = new Dictionary<string, string>
            {
                ["videoId"] = videoId,
                ["song"] = song,
                ["artist"] = artist,
                ["duration"] = duration.ToString(),
                ["album"] = album,
                ["alwaysFetchMetadata"] = "false",
                ["token"] = token
            };

            var content = new FormUrlEncodedContent(data);

            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, new Uri(url))
            {
                Content = content
            };

            request.Headers.TryAddWithoutValidation(
                "Accept", "*/*");

            request.Headers.TryAddWithoutValidation(
                "Referer", "https://music.youtube.com/");

            var response = await client.SendAsync(request);

            string responseBody =
                await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            return responseBody;
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
        public string GetNextLyricByTime(double seconds)
        {
            if (songLyrics.Count > 0)
            {
                foreach (var lyric in songLyrics)
                {
                    if (lyric.Item1 > seconds)
                    {
                        var line = lyric.Item2.Replace("\n", " ").Replace("\r", " ");
                        return string.IsNullOrEmpty(line) ? "[Music]" : line;
                    }
                }
                return ""; // No next lyric found
            }
            else if (_songLyrics != null)
            {
                var nextCaption = _songLyrics.Captions.FirstOrDefault(c => c.Offset.TotalSeconds > seconds);
                if (nextCaption != null)
                    return nextCaption.Text.Replace("\n", " ").Replace("\r", " ");
                return "";
            }
            return "";
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
