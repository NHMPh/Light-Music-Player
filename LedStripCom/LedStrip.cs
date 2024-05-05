using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace NHMPh_music_player.LedStripCom
{
    internal class LedStrip
    {
        private int numberOfLed;

        private Led[] leds;

        private SolidColorBrush[] colorPallet = new SolidColorBrush[3];
        

  
       

        private StackPanel ledContainer;

        public StackPanel LedContainer { get { return ledContainer; } }

        private SolidColorBrush uncolor = new SolidColorBrush(Colors.Black);
       
        public LedStrip(int numberOfLed)
        {
            colorPallet[0] = new SolidColorBrush(Colors.Green);
            colorPallet[1] = new SolidColorBrush(Colors.Yellow);
            colorPallet[2] = new SolidColorBrush(Colors.Red);
            this.numberOfLed = numberOfLed;
            leds = new Led[numberOfLed];
            ledContainer = new StackPanel() { 
            Orientation = Orientation.Vertical,
            };
            for (int i = 19; i >= 0; i--)
            {
                leds[i] = new Led();
                ledContainer.Children.Add(leds[i].LedDisplay);
            }

        }

        public void SetIndividualColor(int index, SolidColorBrush color)
        {
            leds[index].ChangeColor(color);
        }

        public void FillColor(int startIndex, int endIndex,SolidColorBrush color)
        {
           for (int i = startIndex; i < endIndex; i++) 
            {
                leds[i].ChangeColor(color);
            }

            for (int i = endIndex; i < 16; i++)
            {
                leds[i].ChangeColor(uncolor);
            }
        }
        public void FillColor(int endIndex, SolidColorBrush color)
        {
           
            if (endIndex < 0) endIndex = 0; 
            for (int i = 0; i < endIndex; i++)
            {
                if (i < 5)
                {
                    color = colorPallet[0];

                }else if (i < 10)
                {
                    color = colorPallet[1];
                }
                else
                {
                    color = colorPallet[2];
                }
                leds[i].ChangeColor(color);
            }
          
            for (int i = endIndex; i < 20; i++)
            {
                leds[i].ChangeColor(uncolor);
            }
         
        }

    }
}
