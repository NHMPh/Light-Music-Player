using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMPh_music_player
{
    internal class SongsManager
    {
        private VideoInfo currentSong;

        public VideoInfo CurrentSong { get { return currentSong; } }


        private Queue<VideoInfo> videoInfosQueue = new Queue<VideoInfo>();

        public Queue<VideoInfo> VideoInfosQueue { get { return videoInfosQueue; } }

        private Queue<VideoInfo> videoInfosAutoPlayQueue = new Queue<VideoInfo>();

        public Queue<VideoInfo> VideoInfosAutoPlayQueue { get { return videoInfosAutoPlayQueue; } }
        public SongsManager() { }

        public void AddSong(VideoInfo video)
        {
            videoInfosQueue.Enqueue(video);
        }
        public void AddSongAutoPlay(VideoInfo video)
        {
            videoInfosQueue.Enqueue(video);
        }
        public void RemoveSong(int index)
        {
            RemoveAtIndex(videoInfosQueue, index);
        }
        public void RemoveSongAutoPlay(int index)
        {
            RemoveAtIndex(videoInfosAutoPlayQueue, index);
        }
        public void NextSong()
        {
            if (videoInfosQueue.Count + videoInfosAutoPlayQueue.Count == 0) return;

            if (videoInfosQueue.Count > 0)
                currentSong = videoInfosQueue.Dequeue();
            else
                currentSong = videoInfosAutoPlayQueue.Dequeue();
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
