using System;
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
        public MainWindow()
        {
            InitializeComponent();
            var mediaPlayer = new MediaPlayer(this);
            var songManger = new SongsManager();
            var uiControl = new UIControl(this, mediaPlayer,songManger);
            var dynamicVisualUpdate = new DynamicVisualUpdate(this, mediaPlayer,songManger);
            var playlist = new Playlist(this, songManger);
            var customPlaylist = new _CustomPlaylist(this,songManger,mediaPlayer);  
        }  
   
    }

}
