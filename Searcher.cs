using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using YoutubeSearchApi.Net.Services;
using YoutubeExplode;
using System.Windows;
using YoutubeExplode.Common;

namespace NHMPh_music_player
{
    public static class Searcher
    {
     
        public async static Task Search(string key , SongsManager songsManager)
        {        
            YoutubeClient youtube = new YoutubeClient();
            int mode = StringUtilitiy.EvaluateKeyWord(key);
            //Search by link
            if (mode == 1)
            {
                try
                {
                    var videolinkInfo = await youtube.Videos.GetAsync(key);
                    songsManager.AddSong (new VideoInfo(videolinkInfo));                  
                    
                }
                catch
                {
                    MessageBox.Show("Error! This youtube link is wrong or not found");                 
                    
                }

            }

            if (mode == 2)
            {
                try
                {
                    var videos = await youtube.Playlists.GetVideosAsync(key);
                    for (int i = 1; i < videos.Count; i++)
                    {
                        
                        songsManager.AddSong(new VideoInfo(videos[i].Title, "Song by " + videos[i].Author, videos[i].Url, videos[i].Thumbnails.FirstOrDefault().Url));
                    }                  
                   
                }
                catch
                {

                    MessageBox.Show("Error! Auto-generated playlist link is not supported or playlist is private");
                   


                }


            }
            //Search by key         
            HttpClient httpClient = new HttpClient();
            YoutubeSearchClient client = new YoutubeSearchClient(httpClient);
            var responseObject = await client.SearchAsync(key);
            songsManager.AddSong(new VideoInfo(responseObject.Results.First()));
        }
    }
}
