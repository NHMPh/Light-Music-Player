using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace NHMPh_music_player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MediaPlayer mediaPlayer;
        SongsManager songManger;
        UIControl uiControl;
        DynamicVisualUpdate dynamicVisualUpdate;
        Playlist playlist;
        _CustomPlaylist customPlaylist;
        SpectrumVisualizer visualizer;
        public YoutubeClient youtube;
        public Cookie[] youtubeCookies;
        Radio radio;

        public MediaPlayer MediaPlayer { get { return mediaPlayer; } }
        public SongsManager SongsManager { get { return songManger; } }
        public UIControl UIControl { get { return uiControl; } }
        public DynamicVisualUpdate DynamicVisualUpdate { get { return dynamicVisualUpdate; } }
     
        public MainWindow()
        {
  ;
            InitializeComponent();
         
            youtube = new YoutubeClient();
            visualizer = new SpectrumVisualizer(this);
            mediaPlayer = new MediaPlayer(this, visualizer);
            songManger = new SongsManager();
            uiControl = new UIControl(this, mediaPlayer, songManger);
            dynamicVisualUpdate = new DynamicVisualUpdate(this, mediaPlayer, songManger, visualizer);
            playlist = new Playlist(this, songManger);
            customPlaylist = new _CustomPlaylist(this, songManger, mediaPlayer);
            radio = new Radio(mediaPlayer, this);
        }
        public void LoginToYoutube(YoutubeClient youtubeClient)
        {
            youtube = youtubeClient;
        }
        public void RefreshYoutubeClientHttpClient()
        {
         
            youtube = new YoutubeClient(CreateYoutubeClientWithCookies(youtubeCookies),youtubeCookies);
        }
        private HttpClient CreateYoutubeClientWithCookies(Cookie[] cookies)
        {
            var cookieContainer = new CookieContainer();

            foreach (var cookie in cookies)
            {
                try
                {
                    cookieContainer.Add(cookie);
                }
                catch
                {
                    // Ignore malformed cookies
                }
            }

            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };

            var httpClient = new HttpClient(handler, disposeHandler: true);

           // Add browser-like headers
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36");

            httpClient.DefaultRequestHeaders.Add("sec-ch-ua",
                "\"Google Chrome\";v=\"141\", \"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"141\"");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-arch", "\"x86\"");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-bitness", "\"64\"");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-form-factors", "\"Desktop\"");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-full-version", "\"141.0.7390.108\"");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-full-version-list",
                "\"Google Chrome\";v=\"141.0.7390.108\", \"Not?A_Brand\";v=\"8.0.0.0\", \"Chromium\";v=\"141.0.7390.108\"");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-model", "\"\"");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform-version", "\"19.0.0\"");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-wow64", "?0");
            httpClient.DefaultRequestHeaders.Add("upgrade-insecure-requests", "1");

            return httpClient;
        }
    }

}
