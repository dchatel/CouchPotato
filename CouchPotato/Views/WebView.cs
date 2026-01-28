using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

using System;
using System.IO;
using System.Windows;

namespace CouchPotato.Views;

public partial class WebView : WebView2
{
    public WebView() : base()
    {
        this.DataContextChanged += (sender, e) =>
        {
            if (sender is WebView2 webView && webView.DataContext is null)
            {
                webView.Source = new Uri("about:blank");
            }
        };
    }
}

public static class VideoBehavior
{
    public static readonly DependencyProperty FilePathProperty =
        DependencyProperty.RegisterAttached(
            "FilePath",
            typeof(string),
            typeof(VideoBehavior),
            new PropertyMetadata(null, OnFilePathChanged));

    public static string GetFilePath(DependencyObject obj) => (string)obj.GetValue(FilePathProperty);
    public static void SetFilePath(DependencyObject obj, string value) => obj.SetValue(FilePathProperty, value);

    private static async void OnFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WebView2 webView && e.NewValue is string videoPath && !string.IsNullOrWhiteSpace(videoPath))
        {
            // 1. Initialize WebView2
            await webView.EnsureCoreWebView2Async();

            // 2. Security: Map the drive (e.g., "C:\") to a fake website "http://local-video/"
            // This tricks the browser into thinking it's streaming from the web.
            string driveRoot = Path.GetPathRoot(videoPath)!;
            string virtualHostName = "local-video";

            // Clear old mappings to be safe, then add the new one
            webView.CoreWebView2.ClearVirtualHostNameToFolderMapping(virtualHostName);
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                virtualHostName,
                driveRoot,
                CoreWebView2HostResourceAccessKind.Allow
            );

            // 3. Convert absolute path to our fake URL
            // Example: C:\Videos\Clip.mp4 -> http://local-video/Videos/Clip.mp4
            string relativePath = videoPath.Substring(driveRoot.Length).Replace("\\", "/");
            string streamUrl = $"http://{virtualHostName}/{relativePath}";

            // 4. Inject the HTML Player
            string html = $@"
                    <!DOCTYPE html>
                    <html>
                    <body style='margin:0; background:#222; display:flex; align-items:center; justify-content:center; height:100vh; overflow:hidden;'>
                        <video controls autoplay style='width:100%; height:100%; object-fit:contain;'>
                            <source src='{streamUrl}' type='video/mp4'>
                        </video>
                    </body>
                    </html>";

            webView.NavigateToString(html);
        }
    }
}