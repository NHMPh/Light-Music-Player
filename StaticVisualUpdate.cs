using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NHMPh_music_player
{
    public class StaticVisualUpdate
    {
        MainWindow mainWindow;
        public StaticVisualUpdate(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }
        public void SetVisual(VideoInfo videoInfo)
        {
            mainWindow.title.Text = videoInfo.Title;
            mainWindow.description.Text = videoInfo.Description;
            mainWindow.thumbnail.ImageSource = new BitmapImage(new Uri(videoInfo.Thumbnail));
        }
        public void SetVisualDes(string des)
        {
            mainWindow.description.Text = des;
        }


    }
}
