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

        // System.IO.Ports.SerialPort serialPort;


        _CustomPlaylist customPlaylist;
        int[] data = new int[15] { 6, 7, 8, 9, 10, 11, 12, 13, 12, 11, 10, 9, 8, 7, 6 };
       // System.IO.Ports.SerialPort serialPort;
        public MediaPlayer MediaPlayer { get { return mediaPlayer; } }
        public SongsManager SongsManager { get { return songManger; } }
        public UIControl UIControl { get { return uiControl; } }
        public DynamicVisualUpdate DynamicVisualUpdate { get { return dynamicVisualUpdate; } }
        public MainWindow()
        {
            InitializeComponent();
            mediaPlayer = new MediaPlayer(this);
            songManger = new SongsManager();
            uiControl = new UIControl(this, mediaPlayer, songManger);
            dynamicVisualUpdate = new DynamicVisualUpdate(this, mediaPlayer, songManger);
            playlist = new Playlist(this, songManger);
            customPlaylist = new _CustomPlaylist(this, songManger, mediaPlayer);
          /*  serialPort = new SerialPort("COM4", 115200);

            try
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            String dataToSend = "1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 ";
            String dataToSend2 = "15 14 13 12 11 10 9 8 7 6 5 4 3 2 1";
            if (serialPort.IsOpen)
            {
                    serialPort.Write(dataToSend);
               *//* while (true)
                {
                    Thread.Sleep(100);
                    serialPort.Write(dataToSend2);
                    Thread.Sleep(100);
                }*//*


                Console.Write("send");

            }
            else
            {
                MessageBox.Show("Serial port is not open.");
            }*/

        }
    }

}
