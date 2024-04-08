using AngleSharp;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace NHMPh_music_player
{
    public class SongsManager
    {
        private VideoInfo currentSong;

        public VideoInfo CurrentSong { get { return currentSong; } }


        private Queue<VideoInfo> videoInfosQueue = new Queue<VideoInfo>();

        public Queue<VideoInfo> VideoInfosQueue { get { return videoInfosQueue; } set { videoInfosQueue = value; } }

        private Queue<VideoInfo> videoInfosAutoPlayQueue = new Queue<VideoInfo>();

        public Queue<VideoInfo> VideoInfosAutoPlayQueue { get { return videoInfosAutoPlayQueue; } set { videoInfosAutoPlayQueue = value; } }

        public event EventHandler OnVideoQueueChange;
        public SongsManager() { }

        public void AddSong(VideoInfo video)
        {
            videoInfosQueue.Enqueue(video);
            OnVideoQueueChange?.Invoke(this, null);
        }
        public void RemoveSong(int index)
        {
            RemoveAtIndex(videoInfosQueue, index);
            OnVideoQueueChange?.Invoke(this, null);
        }
        public void RemoveSongAutoPlay()
        {           
            videoInfosAutoPlayQueue.Clear();
            OnVideoQueueChange?.Invoke(this, null);
        }
        public void RemoveSongAutoPlay(int index)
        {
            RemoveAtIndex(videoInfosAutoPlayQueue, index);
            OnVideoQueueChange?.Invoke(this, null);
        }
        public int TotalSongInQueue()
        {
            return videoInfosQueue.Count + videoInfosAutoPlayQueue.Count;
        }
        public async void AddSongAutoplay( MainWindow mainWindow)
        {
            
            MusicSetting.isBrowser = true;
            mainWindow.status.Text = "Fetching...";
            IBrowser browser;
            try
            {

                browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe"
                });
            }
            catch
            {
                try
                {
                    browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true,
                        ExecutablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
                    });
                }
                catch
                {
                    await new BrowserFetcher().DownloadAsync();
                    browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true,
                    });
                }

            }
            string videoUrl = $"{currentSong.Url}&list=RD{StringUtilitiy.ExtractId(currentSong.Url)}&themeRefresh=1";
            var page = await browser.NewPageAsync();
            await page.GoToAsync(videoUrl);
            //style-scope ytd-playlist-panel-renderer
            await page.WaitForSelectorAsync(".style-scope.ytd-playlist-panel-renderer");
            await page.WaitForSelectorAsync("a#wc-endpoint");
            await page.WaitForSelectorAsync("img");
            await page.WaitForNetworkIdleAsync();
            var content = await page.GetContentAsync();
            
            await browser.CloseAsync();  
            
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(res => res.Content(content));
            var wcEndpointHrefs = document.QuerySelectorAll("a#wc-endpoint")
           .Select(a => a.GetAttribute("href"))
           .ToList();
            var wcEndpointTitles = document.QuerySelectorAll("#video-title")
          .Select(a => a.GetAttribute("title"))
          .ToList();

            List<string> urls = new List<string>();
            List<string> titles = new List<string>();
            List<string> thumbnails = new List<string>();

            int stopCount = 2;
            // Print the extracted href values
            foreach (var href in wcEndpointHrefs)
            {
                if (stopCount == 2)
                {
                    urls.Add($"https://www.youtube.com/watch?v={StringUtilitiy.ExtractId(href)}");
                    thumbnails.Add($"https://i.ytimg.com/vi/{StringUtilitiy.ExtractId(href)}/hqdefault.jpg?sqp=-oaymwEjCOADEI4CSFryq4qpAxUIARUAAAAAGAElAADIQj0AgKJDeAE=&rs=AOn4CLBzQ9ogPrePWJD7x2FhmZKlos8bDA");
                    stopCount = 0;
                    //https://i.ytimg.com/vi/nfs8NYg7yQM/hqdefault.jpg?sqp=-oaymwEjCOADEI4CSFryq4qpAxUIARUAAAAAGAElAADIQj0AgKJDeAE=&rs=AOn4CLBzQ9ogPrePWJD7x2FhmZKlos8bDA
                }
                else
                {
                    stopCount++;
                }
            }
            stopCount = 0;
            foreach (var title in wcEndpointTitles)
            {
                if (title != null && stopCount < 25)
                {
                    titles.Add(title);
                    stopCount++;
                }
            }
            for (int i = 0; i < 25; i++)
            {
                videoInfosAutoPlayQueue.Enqueue(new VideoInfo(titles[i], "From autoplay", urls[i], thumbnails[i]));
                Console.WriteLine($"Thumbnail: {thumbnails[i]} Title: {titles[i]} Url: {urls[i]} ");
            }        
            MusicSetting.isBrowser = false;
            mainWindow.status.Text = "Playlist loaded";
            //remove the original
            videoInfosAutoPlayQueue.Dequeue();           
            OnVideoQueueChange?.Invoke(this, null);
        }
        public void NextSong()
        {
            if (videoInfosQueue.Count + videoInfosAutoPlayQueue.Count == 0) return;

            if (videoInfosQueue.Count > 0)
                currentSong = videoInfosQueue.Dequeue();
            else
                currentSong = videoInfosAutoPlayQueue.Dequeue();
        }
        public void InvokeVideoQueueChange()
        {
            OnVideoQueueChange?.Invoke(this, null);
        }
        private void RemoveAtIndex<T>(Queue<T> queue, int index)
        {
            if (index < 0 || index >= queue.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Queue<T> tempQueue = new Queue<T>();

            // Dequeue elements until the desired index
            for (int i = 0; i < index; i++)
            {
                tempQueue.Enqueue(queue.Dequeue());
            }

            // Remove the element at the desired index
            queue.Dequeue();
            while (queue.Count > 0)
            {
                tempQueue.Enqueue(queue.Dequeue());
            }
            // Enqueue back the remaining elements from tempQueue
            while (tempQueue.Count > 0)
            {
                queue.Enqueue(tempQueue.Dequeue());
            }

        }
    }
}
