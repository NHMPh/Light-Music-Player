
using System;
using System.Collections.Generic;
using System.Linq;

using YoutubeExplode.Common;

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
           
            string videoUrl = $"{currentSong.Url}&list=RD{StringUtilitiy.ExtractId(currentSong.Url)}&themeRefresh=1";
           
            // Get all playlist videos
            var videos = await mainWindow.youtube.Playlists.GetVideosAsync(videoUrl).CollectAsync(25);
            foreach (var video in videos)
            {
              
                videoInfosAutoPlayQueue.Enqueue(new VideoInfo(video.Title, "From autoplay",video.Url, video.Thumbnails.First().Url));
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
