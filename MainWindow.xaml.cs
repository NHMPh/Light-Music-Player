using System;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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


        public MediaPlayer MediaPlayer { get { return mediaPlayer; } }
        public SongsManager SongsManager { get { return songManger; } }
        public UIControl UIControl { get { return uiControl; } }
        public DynamicVisualUpdate DynamicVisualUpdate { get { return dynamicVisualUpdate; } }
        public MainWindow()
        {
            InitializeComponent();
            
            visualizer = new SpectrumVisualizer(this);
            mediaPlayer = new MediaPlayer(this,visualizer);
            songManger = new SongsManager();
            uiControl = new UIControl(this, mediaPlayer, songManger);
            dynamicVisualUpdate = new DynamicVisualUpdate(this, mediaPlayer, songManger,visualizer);
            playlist = new Playlist(this, songManger);
            customPlaylist = new _CustomPlaylist(this, songManger, mediaPlayer);

        

        }
    }

}
