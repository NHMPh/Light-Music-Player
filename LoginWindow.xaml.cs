using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public LoginWindow()
        {
            InitializeComponent();
            MouseLeftButtonDown += (s, e) => DragMove();
            InitializeWebViewAsync();
        }
        private bool isAuthenticated = false;


        private async void InitializeWebViewAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
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
