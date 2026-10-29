using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YoutubeExplode;
using YoutubeExplode.Common;

namespace NHMPh_music_player
{
    public partial class LoginWindow :Window
    {
        private CoreWebView2 _coreWebView2;
        private const string HomePageUrl = "https://www.youtube.com/";

        public YoutubeClient YoutubeClientInstance { get; private set; }
        public Cookie[] YoutubeCookies { get; private set; }

        private bool isForToken = false;
        public LoginWindow(bool isForToken)
        {
            InitializeComponent();
            MouseLeftButtonDown += (s, e) => DragMove();

            this.isForToken = isForToken;

            if (!this.isForToken)

            {
                this.Visibility = Visibility.Visible;
                InitializeWebViewAsync();

            }
        }
        private bool isAuthenticated = false;
        private async void GetLyricsToken(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = "https://lyrics.api.dacubeking.com/verify-turnstile";


                string json = "{\"token\":\"" + token + "\"}";

                using (var content = new StringContent(
                    json, Encoding.UTF8, "application/json"))
                {
                    HttpResponseMessage response =
                        await client.PostAsync(url, content);

                    string result = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("Status: " + (int)response.StatusCode);
                    Console.WriteLine("Response: " + result);

                    string newToken = JObject.Parse(result)["jwt"].ToString();

                    //save the new token to a file or use it as needed
                    SaveTokenToTxt(newToken, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lyricstoken.txt"));

                   
                }
            }
           
        }
        void SaveTokenToTxt(string tokens, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine(tokens);
            }
        }
        public  async void GetVerifyToken()
        {
            Console.WriteLine("Starting WebView for token retrieval...");

            await webView.EnsureCoreWebView2Async(null);

            Console.WriteLine("dsadasdssss");
            webView.Visibility = System.Windows.Visibility.Hidden;
            this.Visibility = Visibility.Hidden;
            string token = string.Empty;

            Console.WriteLine("WebView initialized. Waiting for token...");
            // Receive messages forwarded from JavaScript to C#.
            webView.CoreWebView2.WebMessageReceived += (sender, e) =>
            {
                token = JObject.Parse(e.WebMessageAsJson)["data"]["token"].ToString();

                Console.WriteLine("Received token: " + token);

                GetLyricsToken(token);             
            };

            string html = @"
            <!DOCTYPE html>
            <html>
            <body>
                <iframe
                    id=""targetFrame""
                    src=""https://lyrics.api.dacubeking.com/challenge""
                    width=""800""
                    height=""600"">
                </iframe>

                <script>
                    window.addEventListener(""message"", function (event) {
                        console.log(""Message received:"", event.data);
                        console.log(""Origin:"", event.origin);

                        // Forward the message to C#.
                        window.chrome.webview.postMessage({
                            origin: event.origin,
                            data: event.data
                        });
                    });
                </script>
            </body>
            </html>
            """;
            
            webView.CoreWebView2.NavigateToString(html);
        }

        private async void InitializeWebViewAsync()
        {
            await webView.EnsureCoreWebView2Async(null);

            webView.Visibility = System.Windows.Visibility.Visible;
            this.Visibility = Visibility.Visible;
            _coreWebView2 = webView.CoreWebView2;

            // Subscribe to navigation events
            _coreWebView2.NavigationStarting += WebBrowser_OnNavigationStarting;

            // Navigate to Google login (redirect to YouTube after login)
            string loginUrl =
                $"https://accounts.google.com/ServiceLogin?continue={Uri.EscapeDataString(HomePageUrl)}";

            _coreWebView2.Navigate(loginUrl);
        }
        private HttpClient CreateYoutubeClientWithCookies(Cookie[] cookies)
        {
            var cookieContainer = new CookieContainer();

            foreach (var cookie in cookies)
            {
                try
                {
                    cookieContainer.Add(cookie);
                }
                catch
                {
                    // Ignore malformed cookies
                }
            }

            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };

            var httpClient = new HttpClient(handler, disposeHandler: true);

           // Add browser-like headers
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36");



            return httpClient;
        }

        private async void WebBrowser_OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (_coreWebView2 is null)
                return;

            string url = e.Uri;
            if (url is null)
                return;

            // When redirected back to YouTube home (login finished)
            if (url.StartsWith(HomePageUrl, StringComparison.OrdinalIgnoreCase))
            {
                var cookies = await _coreWebView2.CookieManager.GetCookiesAsync(url);

                // Convert to System.Net.Cookie[]
                var systemCookies = cookies.Select(c => c.ToSystemNetCookie()).ToArray();
                var httpclient = CreateYoutubeClientWithCookies(systemCookies);
                YoutubeClient _youtube = new YoutubeClient(httpclient,systemCookies);
                YoutubeClientInstance = _youtube;
                YoutubeCookies = systemCookies;
                AuthenPannel.Height = 474;
                WebViewPannel.Height = 0;
                isAuthenticated = true;
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            webView.Dispose();
            if (isAuthenticated)
            {

                this.DialogResult = true;
            }else
            {
                this.DialogResult = false;
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await webView.EnsureCoreWebView2Async();
            _coreWebView2 = webView.CoreWebView2;

            _coreWebView2.CookieManager.DeleteAllCookies();

            // Reset panels
            AuthenPannel.Height = 0;
            WebViewPannel.Height = 474;

            isAuthenticated= false;
            InitializeWebViewAsync();
        }
    }
}
