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
using YoutubeDLSharp;

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
        public YoutubeDL ytdl;

        Radio radio;

        public MediaPlayer MediaPlayer { get { return mediaPlayer; } }
        public SongsManager SongsManager { get { return songManger; } }
        public UIControl UIControl { get { return uiControl; } }
        public DynamicVisualUpdate DynamicVisualUpdate { get { return dynamicVisualUpdate; } }
        public class CookieData
        {
            public string name { get; set; }
            public string path { get; set; }
            public string domain { get; set; }
            public string value { get; set; }
        }
        public MainWindow()
        {
  ;
            InitializeComponent();
            
            youtube = new YoutubeClient();
            ytdl = new YoutubeDL();
            visualizer = new SpectrumVisualizer(this);
            mediaPlayer = new MediaPlayer(this, visualizer);
            songManger = new SongsManager();
            uiControl = new UIControl(this, mediaPlayer, songManger);
            dynamicVisualUpdate = new DynamicVisualUpdate(this, mediaPlayer, songManger, visualizer);
            playlist = new Playlist(this, songManger);
            customPlaylist = new _CustomPlaylist(this, songManger, mediaPlayer);
            radio = new Radio(mediaPlayer, this);
        }
  
    }

}
