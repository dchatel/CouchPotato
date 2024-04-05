using System;

using Microsoft.Web.WebView2.Wpf;

namespace CouchPotato.Views;

public class WebView : WebView2
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