using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace NHMPh_music_player.LedStripCom
{
    internal class Led
    {
        private SolidColorBrush color = new SolidColorBrush(Colors.Cyan);


        public SolidColorBrush Color { get { return color; } }

        private Button ledDisplay = new Button();
        public Button LedDisplay { get { return ledDisplay; } }


        public Led()
        {
            ledDisplay.Width = 60;
            ledDisplay.Height = 30;
            ledDisplay.Margin = new Thickness(20,5,20,0);
            ledDisplay.Background = color;
            ledDisplay.BorderThickness = new Thickness(0);
        }
        public Led( int width, int height, int space)
        {
            ledDisplay.Width = width;
            ledDisplay.Height = height;
            ledDisplay.Margin = new Thickness(space, 5, space, 0);
            ledDisplay.Background = color;
            ledDisplay.BorderThickness = new Thickness(0);
        }


        private Random rnd = new Random();
        public void ChangeColor(Brush color)
        {
          
        
            ledDisplay.Background = color;
        }

        public Brush PickBrush()
        {
            Brush result = Brushes.Transparent;

            Random rnd = new Random();

            Type brushesType = typeof(Brushes);

            PropertyInfo[] properties = brushesType.GetProperties();

            int random = rnd.Next(properties.Length);
            result = (Brush)properties[random].GetValue(null, null);

            return result;
        }
    }
}
