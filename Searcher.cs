using Accord;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YoutubeDLSharp;
using YoutubeExplode;
using YoutubeExplode.Common;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Pagination;
using YoutubeSearchApi.Net.Models.Youtube;
using YoutubeSearchApi.Net.Services;
namespace NHMPh_music_player
{
    public static class Searcher
    {
        public async static Task<YoutubeVideo> ReSearch(string key)
        {
            HttpClient httpClient = new HttpClient();
            YoutubeSearchClient client = new YoutubeSearchClient(httpClient);
            var responseObject = await client.SearchAsync(key);
            return responseObject.Results.First();
        }
        public async static Task Search(string key, SongsManager songsManager, MainWindow mainWindow)
        {


            int mode = StringUtilitiy.EvaluateKeyWord(key);
            //Search by link
            if (mode == 1)
            {
                try
                {
                    var videolinkInfo = await mainWindow.youtube.Videos.GetAsync(key);
                    songsManager.AddSong(new VideoInfo(videolinkInfo));

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
                    var videos = await mainWindow.youtube.Playlists.GetVideosAsync(key).CollectAsync(25);
                    for (int i = 1; i < videos.Count; i++)
                    {

                        songsManager.AddSong(new VideoInfo(videos[i].Title, "Song by " + videos[i].Author, videos[i].Url, videos[i].Thumbnails.FirstOrDefault().Url));
                    }

                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error! Auto-generated playlist link is not supported or playlist is private" + ex.ToString());



                }


            }
            if (mode == 3)
            {
                HttpClient httpClient = new HttpClient();
                if (key.First() == '`')
                {
                    YouTubeMusicClient yt = new YouTubeMusicClient("US", null, null, null, httpClient);
                    PaginatedAsyncEnumerable<SearchResult> searchResults = yt.SearchAsync(key.Remove(0,1), SearchCategory.Songs);
                    IReadOnlyList<SearchResult> bufferedSearchResults = await searchResults.FetchItemsAsync(0, 1);
                    foreach (SongSearchResult song in bufferedSearchResults.Cast<SongSearchResult>())
                    {
                        
                        var albumName = song.Album?.Name ?? "Unknown Album";
                        var artistNames = song.Artists?.Select(artist => artist?.Name ?? "Unknown Artist") ?? new List<string> { "Unknown Artist" };
                        int minWidth =0;
                        string maxThumbUrl = song.Thumbnails.FirstOrDefault().Url;
                        bool isSquare = false;
                        foreach (var thumb in song.Thumbnails)
                        {
                            if(minWidth < thumb.Width&& thumb.Width==thumb.Height)
                            {
                                maxThumbUrl = thumb.Url;
                                minWidth = thumb.Width;
                                isSquare = true;
                            }
                               
                            Console.WriteLine($"Thumb: {thumb.Url} {thumb.Width}x{thumb.Height} ");
                        }
                        songsManager.AddSong(new VideoInfo(song.Name,"From youtube music", "https://youtube.com/watch?v=" + song.Id,song.Thumbnails.FirstOrDefault().Url,artistNames.First(),albumName,isSquare));
                        Console.WriteLine($"{song.Name} - {song.Thumbnails} - {song.Category} - {song.Artists}");
                        return;
                    }
                }
                
               
                YoutubeSearchClient client = new YoutubeSearchClient(httpClient);
                var responseObject = await client.SearchAsync(key);
                songsManager.AddSong(new VideoInfo(responseObject.Results.First()));
                

            }

        }
    }
}
