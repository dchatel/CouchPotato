using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using CouchPotato.DbModel;
using CouchPotato.DbModel.OtherDbModels.Tmdb;

using Microsoft.Web.WebView2.Wpf;

namespace CouchPotato.Views.ActorFinderDialog;

public class ActorFinderViewModel : ContentViewModel
{
    private readonly IEnumerable<Person> _excludedPeople;
    private string? _searchText;
    private Person? _selectedPerson;

    public string? SearchText
    {
        get => _searchText;
        set {
            _searchText = value;
            Task.Run(Search);
        }
    }

    public string? Characters { get; set; }

    public IEnumerable<Person> SearchResults { get; set; }
    public Person? SelectedPerson
    {
        get => _selectedPerson;
        set {
            _selectedPerson = value;
            if (_selectedPerson is not null)
                Url = $"https://www.themoviedb.org/person/{_selectedPerson.TmdbId}";
        }
    }
    public string Url { get; set; }
    public bool Searching { get; set; }

    public ActorFinderViewModel(IEnumerable<Person> excludedPeople) : base(autoClose: true)
    {
        _excludedPeople = excludedPeople;
        SearchResults = Enumerable.Empty<Person>();
        Url = "";
    }

    private async Task Search()
    {
        Searching = true;
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            SearchResults = Enumerable.Empty<Person>();
        }
        else
        {
            var results = await Tmdb.SearchActors(_searchText, _excludedPeople);
            SearchResults = results;
        }
        Searching = false;
    }
}

// Fix the issue with WebView throwing an exception when unloaded
public partial class ResourceDictionary : System.Windows.ResourceDictionary
{
    private void WebView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is WebView2 webView && webView.DataContext is null)
        {
            webView.Source = new Uri("about:blank");
        }
    }
}