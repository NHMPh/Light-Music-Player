using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace NHMPh_music_player.LedStripCom
{
    internal class LedSpectrum
    {
        private int numberOfBands;

        private LedStrip[] ledStrips;

        public LedStrip[] LedStrips { get { return ledStrips; } }

        private StackPanel ledStripContainer;

        public StackPanel LedStripContainer { get { return ledStripContainer; } }


        public LedSpectrum(int numberOfBands, int numberOfLedPreBand)
        {
            this.numberOfBands = numberOfBands;
            ledStrips = new LedStrip[numberOfBands];
            ledStripContainer = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };
            for (int i = 0; i < ledStrips.Length; i++)
            {
                ledStrips[i] = new LedStrip(numberOfLedPreBand);
                ledStripContainer.Children.Add(ledStrips[i].LedContainer);
            }
        }
        public LedSpectrum(int numberOfBands, int numberOfLedPreBand,int width, int height, int space)
        {
            this.numberOfBands = numberOfBands;
            ledStrips = new LedStrip[numberOfBands];
            ledStripContainer = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };
            for (int i = 0; i < ledStrips.Length; i++)
            {
                ledStrips[i] = new LedStrip(numberOfLedPreBand,width,height,space);
                ledStripContainer.Children.Add(ledStrips[i].LedContainer);
            }
        }

        private SolidColorBrush color = new SolidColorBrush(Colors.DarkCyan);
        private SolidColorBrush color1 = new SolidColorBrush(Colors.Red);
        private SolidColorBrush color2 = new SolidColorBrush(Colors.Red);
        private SolidColorBrush color3 = new SolidColorBrush(Colors.Red);
        private SolidColorBrush scolor = new SolidColorBrush(Colors.White);
        public void DrawSpectrum(int[] data, int[] snow)
        {

            // Create SolidColorBrush with random color

            for (int i = 0; i < ledStrips.Length; i++)
            {
                if (i < 5)
                {
                    color = color1;
                }
                else if (i < 10)
                {
                    color = color2;
                }
                else
                {
                    color = color3;
                }
                ledStrips[i].FillColor(data[i], color);
                ledStrips[i].SetIndividualColor(snow[i], scolor);
            }
        }
         public void ChangeSpectrumColor(int id,int red, int green, int blue)
        {
            Color color = Color.FromRgb((byte)red, (byte)green, (byte)blue);
            SolidColorBrush solidColorBrush = new SolidColorBrush(color);

            for (int i = 0; i < ledStrips.Length; i++)
            {
               
                ledStrips[i].FillColor(20, solidColorBrush);
              
            }
        }

    }
}
