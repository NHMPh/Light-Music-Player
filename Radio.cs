using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using Accord.Math;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Button = System.Windows.Controls.Button;

namespace NHMPh_music_player
{
    internal class Radio
    {

        private VideoInfo currentSong;

        private MainWindow mainWindow;
        private MediaPlayer mediaPlayer;
        public Radio(MediaPlayer mediaPlayer, MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            this.mediaPlayer = mediaPlayer;
            CreateRadioList();
        }

        public void CreateRadioList()
        {
            mainWindow.radio_ctn.Children.Clear();
            // Get all file names in the folder
            string[] fileNames = Directory.GetFiles(".\\Radio\\");
            int count = 0;
            if (fileNames.Count() != 0)
            {

                StackPanel stackPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, 2),
                    HorizontalAlignment = HorizontalAlignment.Left,
                };

                foreach (string fileName in fileNames)
                {

                    count++;
                    Console.WriteLine("Count in: " + count);
                    if (count == 6)
                    {
                        mainWindow.radio_ctn.Children.Add(stackPanel);
                        stackPanel= stackPanel = new StackPanel()
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0, 0, 0, 2),
                            HorizontalAlignment = HorizontalAlignment.Left,
                        };
                        count = 1;
                    }
                    var name = System.IO.Path.GetFileNameWithoutExtension(fileName);

                    var item = CreateRadioSelection(name);

                    stackPanel.Children.Add(item);

                    

                }
                Console.WriteLine("Count: "+count);
                if(count > 0)
                {
                    mainWindow.radio_ctn.Children.Add(stackPanel);
                }

            }
        }

        private Grid CreateRadioSelection(string name)
        {
            Console.WriteLine(3);

            JObject radioJSON = new JObject();
            radioJSON = StringUtilitiy.ReadJsonFile($".\\Radio\\{name}.json");
            VideoInfo videoInfo = new VideoInfo("Radio: " + radioJSON["title"].ToString(), radioJSON["urls"][0]["description"].ToString(), radioJSON["urls"][0]["url"].ToString(), $"{Environment.CurrentDirectory}{radioJSON["thumbnail"].ToString()}");
            // Create the Grid
            Grid grid = new Grid();
            grid.Margin = new Thickness(2, 0, 0, 0);
            Console.WriteLine(4);
            // Create the Button
            Button button = new Button();
            button.Width = 70;
            button.BorderBrush = new SolidColorBrush(Colors.Cyan);
            button.Background = Brushes.Transparent;
            button.Click += (sender, e) => Button_Click(sender, e, videoInfo);
            // Create the Image and set it as the Button's content
            Image image = new Image();
            image.Width = 70;

            image.Source = new BitmapImage(new Uri($"{videoInfo.Thumbnail}")); // Adjust the path as necessary
            image.Stretch = Stretch.UniformToFill;
            image.HorizontalAlignment = HorizontalAlignment.Center;
            image.VerticalAlignment = VerticalAlignment.Center;
            button.Content = image;

            // Create a Style for the Button's Border
            Style borderStyle = new Style(typeof(Border));
            borderStyle.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(2)));
            button.Resources.Add(typeof(Border), borderStyle);

            // Create the Border
            Border border = new Border();
            border.Width = 70;
            border.Height = 70;
            border.CornerRadius = new CornerRadius(2);
            border.Background = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0)); // #66000000
            border.IsEnabled = false;
            // Create the TextBlock and add it to the Border
            TextBlock textBlock = new TextBlock();
            textBlock.Text = videoInfo.Title.Replace("Radio: ", "");
            textBlock.Foreground = Brushes.White;
            textBlock.FontSize = 16;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.FontFamily = new FontFamily("Arial Black");
            textBlock.Width = 70;
            textBlock.Height = 70;
            textBlock.VerticalAlignment = VerticalAlignment.Bottom;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.MouseLeftButtonDown += (sender, e) => Button_Click(sender, e, videoInfo);
            border.Child = textBlock;
            border.MouseLeftButtonDown += (sender, e) => Button_Click(sender, e, videoInfo);

            Button button2 = new Button();
            button2.Width = 70;
            button2.Opacity = 0.2;
            button2.BorderBrush = Brushes.Transparent;
            button2.Background = Brushes.Transparent;
            button2.Click += (sender, e) => Button_Click(sender, e, videoInfo);
            // Add the Button and Border to the Grid
            grid.Children.Add(button);
            grid.Children.Add(border);
            grid.Children.Add(button2);

            return grid;
        }

        private async void Button_Click(object sender, RoutedEventArgs e, VideoInfo videoInfo)
        {
            if (!MusicSetting.isSpectrum)
            {
                mainWindow.description.Height = 76;
                mainWindow.spectrum_ctn.Height = 0;


            }
            if (MusicSetting.isSpectrum)
            {
                mainWindow.description.Height = 16;
                mainWindow.spectrum_ctn.Height = 60;

            };
            mainWindow.radio_view.Height = 0;
            mediaPlayer.CurrentSong = videoInfo;
            mediaPlayer.PlayRadio();
           
        }
    }
}
