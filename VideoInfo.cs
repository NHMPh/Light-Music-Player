using Accord;
using Acornima;
using AngleSharp.Io;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.ClosedCaptions;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models.Info;
using YoutubeSearchApi.Net.Models.Youtube;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using HttpMethod = System.Net.Http.HttpMethod;
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
           
            HttpClient httpClient = new HttpClient();

            YouTubeMusicClient yt = new YouTubeMusicClient("US", null, null, null, null, httpClient);
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
            
            var des = await youtube.Videos.GetAsync(this.url);
    
            description =$"{info.Description} - {(int)info.Duration.TotalMinutes}:{info.Duration.Seconds:00} \n{des.Description}";
            Console.WriteLine($"Description: {description}");
            //if (!MusicSetting.isLyrics)
                staticVisualUpdate.SetVisualDes(description);
        }
        private async void GetLyricsLib(MainWindow mainWindow)
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
                    for (int itryattempt = 0; itryattempt < 4; itryattempt++)
                    {
                        Console.WriteLine(songtitle);
                        Console.WriteLine(Artist);
                        Console.WriteLine(Album);
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
                                if (rtext.Length != 0)
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

                                //if (ablbumName != album)
                                //{
                                //    index++;
                                //    continue;
                                //}



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
                            break;
                        }
                        else
                        {
                            songLyrics = new List<(double, string)>();
                            mainWindow.lyrics_btn.Width = 0;
                            mainWindow.lyricsSync_btn.Width = 0;
                            MusicSetting.isLyrics = false;
                            MusicSetting.lyricsOffset = 0;
                            mainWindow.lyricsTextBox.Height = 0;
                            if(!MusicSetting.isSpectrum)
                            mainWindow.description.Height = 74;
                            else
                            mainWindow.description.Height = 16;
                            OnLyricsFound?.Invoke(this, false);
                            await Task.Delay((itryattempt + 2) * 1000); // Wait for 1 second before retrying
                            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                            {
                                itryattempt--;
                            }
                            Console.WriteLine($"Failed to get data. Status code: {response.StatusCode}");

                        }
                    }



                }

                catch { }


            }
            Console.WriteLine("CHECK RAW LYRICS" + rawLyrics != null);



        }

        private static readonly HttpClient client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(60)
        };
        public async Task<string> GetLyricsAsync(
                string videoId,
                string song,
                string artist,
                int duration,
                string album,
                string token)
        {
            const string url =
                "https://lyrics.api.dacubeking.com/v2/lyrics";

            var total = Stopwatch.StartNew();

            void Log(string message)
            {
                Console.WriteLine(
                    $"[{total.Elapsed.TotalSeconds:F2}s] {message}");
            }

            try
            {
                Log("Preparing request");

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

                var request = new HttpRequestMessage(
                    HttpMethod.Post, url)
                {
                    Content = content
                };

                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                request.Headers.TryAddWithoutValidation(
                    "Referer", "https://music.youtube.com/");

                Log("Sending request...");

                var response = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead);

                Log($"Response headers received: {(int)response.StatusCode} " +
                    $"{response.StatusCode}");

                Log("Reading response body...");

                Console.WriteLine(response.Content);
                string responseBody =
                    await response.Content.ReadAsStringAsync();

                Log($"Response body received: {responseBody.Length} characters");
                Log($"Total request completed in {total.Elapsed.TotalSeconds:F2}s");

                if (!response.IsSuccessStatusCode)
                {
                    Log($"HTTP error response: {responseBody}");   
                }

                return responseBody;
            }
            catch (TaskCanceledException ex)
            {
                Log($"Request timed out or was canceled: {ex}");
                throw;
            }
            catch (HttpRequestException ex)
            {
                Log($"HTTP/network error: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                Log($"Unexpected error: {ex}");
                throw;
            }
        }
        private string convertTTMLToLRC(XDocument doc)
        {
            try
            {
                XNamespace ns = "http://www.w3.org/ns/ttml";
                string lrc = string.Join("\n", doc.Descendants(ns + "p").Select(p =>
                {
                    string begin = (string)p.Attribute("begin");
                    TimeSpan time;
                    if (!TimeSpan.TryParse(begin, CultureInfo.InvariantCulture, out time) &&
                        !TimeSpan.TryParseExact(begin,
                            new[] { @"m\:ss\.fff", @"s\.fff", @"m\:ss\.ff" },
                            CultureInfo.InvariantCulture,
                            out time))
                    {
                        throw new FormatException($"Invalid time format: {begin}");
                    }
                    string timestamp =
                        $"[{(int)time.TotalMinutes:00}:{time.Seconds:00}.{time.Milliseconds / 10:00}]";

                    if (p.HasElements)
                    {
                        string value = string.Join(" ", p.Elements(ns + "span")
                            .Select(span => span.Value));

                        return timestamp + " " + value;
                    }
                    
                    return timestamp + " " + p.Value;
                }));
                return lrc;
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
                return string.Empty;
            }
           
        }
        private string lyricsWordbyWordConvert(string wbwlyrics)
        {
            string lyrics = Regex.Replace(wbwlyrics, @"<[^>]*>|\[offset:[^\]]*\]", "").Trim();
            lyrics = Regex.Replace(lyrics, @"[ \t]+", " ");
            return lyrics;
        }
        private  string ProcessLyrics(string rawResponse)
        {
           Console.WriteLine();
           Console.WriteLine("////////////////Processing lyrics//////////////////////");

            Console.WriteLine($"Raw response: {rawResponse}");

            string[] providerPiority = { "binimum", "musixmatch", "lrclib", "golyrics", "kugou",  };

            StringUtilitiy.ExtractProviderResults(rawResponse, out JObject providerResults);

            Console.WriteLine($"Providers: {providerResults}");

            foreach (var provider in providerPiority) {

                switch (provider)
                {
                    case "lrclib":
                        if (providerResults.ContainsKey("lrclib"))
                        {
                            Console.WriteLine("lrclib found");

                            if (providerResults["lrclib"]?["results"]?["synced"].ToString() != "")
                            {
                                Console.WriteLine("lrclib synced lyrics found");

                                string synced = (string)providerResults["lrclib"]["results"]["synced"];

                                Console.WriteLine("Synced lyrics: " + synced);

                                return synced;
                            }
                        }
                        break;

                    case "golyrics":
                        if (providerResults.ContainsKey("golyrics"))
                        {
                            Console.WriteLine("golyrics found");

                            JObject lyricsObj = JObject.Parse(providerResults["golyrics"]?["results"]?["lyrics"].ToString());
                            XDocument doc = XDocument.Parse(lyricsObj["ttml"].ToString());
                            XNamespace ns = "http://www.w3.org/ns/ttml";
                            XNamespace[] lrc = { "http://lrc.red/lyric-ttml-internal", "http://music.apple.com/lyric-ttml-internal" };

                            bool isTimingNone = (string)doc.Root.Attribute(lrc[0] + "timing") == "None" || (string)doc.Root.Attribute(lrc[1] + "timing") == "None";

                            if (isTimingNone)
                            {
                                Console.WriteLine("golyrics timing is None?");

                                Console.WriteLine((string)doc.Root.Attribute(lrc[0] + "timing")+ (string)doc.Root.Attribute(lrc[1] + "timing"));

                                break;
                            }

                                string lyrics = convertTTMLToLRC(doc);

                                Console.WriteLine("golyrics lyrics: " + lyrics);

                            return lyrics;
                        }
                        break;

                    case "kugou":
                        if (providerResults.ContainsKey("kugou"))
                        {
                            Console.WriteLine("kugou found");

                            JObject lyricsObj = JObject.Parse(providerResults["kugou"]?["results"]?["lyrics"].ToString());
                            var lyrics = lyricsObj["lyrics"].ToString();

                            Console.WriteLine("kugou lyrics: " + lyrics);

                            return lyrics;
                        }
                        break;
                    case "binimum":

                        if (providerResults.ContainsKey("binimum"))
                        {
                            Console.WriteLine("binimum found");
                            string lyricsObj = providerResults["binimum"]?["results"]?["lyrics"].ToString();
                            Console.WriteLine(lyricsObj);
                            XDocument doc = XDocument.Parse(lyricsObj);
                            Console.WriteLine(doc);
                            XNamespace ns = "http://www.w3.org/ns/ttml";
                            XNamespace[] lrc = { "http://lrc.red/lyric-ttml-internal", "http://music.apple.com/lyric-ttml-internal" };

                            bool isTimingNone = (string)doc.Root.Attribute(lrc[0] + "timing") == "None" || (string)doc.Root.Attribute(lrc[1] + "timing") == "None";

                            if (isTimingNone)
                            {
                                Console.WriteLine("golyrics timing is None?");

                                Console.WriteLine((string)doc.Root.Attribute(lrc[0] + "timing") + (string)doc.Root.Attribute(lrc[1] + "timing"));

                                break;
                            }

                            string lyrics = convertTTMLToLRC(doc);

                            Console.WriteLine("binimum lyrics: " + lyrics);

                            return lyrics;
                        }
                        break;
                    case "musixmatch":
                        if (providerResults.ContainsKey("musixmatch"))
                        {
                            if (providerResults["musixmatch"]?["results"]?["synced"].ToString() != "")
                            {
                                string synced = (string)providerResults["musixmatch"]["results"]["synced"];

                                return synced;
                            }
                            if(providerResults["musixmatch"]?["results"]?["wordByWord"].ToString() != "")
                            {
                                Console.WriteLine("musixmatch word by word convert");
                                string synced = lyricsWordbyWordConvert((string)providerResults["musixmatch"]["results"]["wordByWord"]);
                                Console.WriteLine(synced);
                                return synced;
                            }
                        }
                        

                            break;

                }
                
            }

            Console.WriteLine("No lyrics found from any provider.");

            return string.Empty;
           
        }

        public async void GetLyrics(MainWindow mainWindow, YoutubeClient youtube)
        {


            _songLyrics = null;

            try
            {
                var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(url);

                var trackInfo = trackManifest.TryGetByLanguage("ru");
                if (trackInfo == null) trackInfo = trackManifest.TryGetByLanguage("en");
                if (!trackInfo.IsAutoGenerated)
                {
                    Console.WriteLine("Found in caption");
                    _songLyrics = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);
                    Console.WriteLine(_songLyrics.Captions);
                    Console.WriteLine(_songLyrics.Captions.First());
                    OnLyricsFound?.Invoke(this, true);
                    mainWindow.lyrics_btn.Width = 20;
                    mainWindow.lyricsSync_btn.Width = 10;
                    return;
                }
            }
            catch
            {

            }
           

           

        
            Console.WriteLine("Not found in caption");

            string token = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lyricstoken.txt")).ToString().Trim();

            string result = await GetLyricsAsync(
                      StringUtilitiy.ExtractId(url),
                      title,
                      artist,
                      duration,
                      album,
                      token
                  );

            Console.WriteLine("Raw lyrics response: " + result);

            if(!result.Contains("event: metadata"))
            {
                Console.WriteLine("Token invaild");

                LoginWindow loginWindow = new LoginWindow(true);

                loginWindow.Visibility = Visibility.Visible;
                loginWindow.Height = 0;
                loginWindow.Width = 0;

                Console.WriteLine("Getting new token");

                loginWindow.GetVerifyToken();



                while (File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lyricstoken.txt")).ToString().Trim() == token) await Task.Delay(1000);

                loginWindow.Close();

                Console.WriteLine("Token change");

                result = await GetLyricsAsync(
                    StringUtilitiy.ExtractId(url),
                    title,
                    artist,
                    duration,
                    album,
                    File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lyricstoken.txt")).ToString().Trim()
                );
            }

           

            string processedLyrics = ProcessLyrics(result);

            if(!string.IsNullOrEmpty(processedLyrics))
            {
                songLyrics = StringUtilitiy.ExtractAndParseTimestampsAndLyricsToMilliseconds(processedLyrics);
                OnLyricsFound?.Invoke(this, true);
                mainWindow.lyrics_btn.Width = 20;
                mainWindow.lyricsSync_btn.Width = 10;
                mainWindow.lyricsBox.Text = "Intermisson";
            }
            else
            {
               
               
                    Console.WriteLine("Fall back to lrclib");
                 
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
