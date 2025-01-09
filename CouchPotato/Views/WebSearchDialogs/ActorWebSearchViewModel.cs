using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using CouchPotato.DbModel;
using CouchPotato.DbModel.OtherDbModels.Tmdb;

namespace CouchPotato.Views.WebSearchDialogs;

public partial class ActorWebSearchViewModel : ContentViewModel
{
    private readonly DataContext _db;
    private readonly IEnumerable<Person> _excludedPeople;
    private string? _searchText;
    private Person? _selectedPerson;

    [ObservableProperty]
    private IEnumerable<Person> _searchResults;
    [ObservableProperty]
    private bool _searching;
    [ObservableProperty]
    private string _url;

    public string? SearchText
    {
        get => _searchText;
        set {
            _searchText = value;
            Task.Run(Search);
        }
    }

    public string? Characters { get; set; }

    public Person? SelectedPerson
    {
        get => _selectedPerson;
        set {
            if (SetProperty(ref _selectedPerson, value))
            {
                if (_selectedPerson is not null)
                    Url = $"https://www.themoviedb.org/person/{_selectedPerson.TmdbId}";
            }
        }
    }

    public ActorWebSearchViewModel(DataContext db, IEnumerable<Person> excludedPeople) : base(autoClose: true)
    {
        _db = db;
        _excludedPeople = excludedPeople;
        SearchResults = [];
        Url = "";
    }

    private async Task Search()
    {
        Searching = true;
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            SearchResults = [];
        }
        else
        {
            var results = await Tmdb.SearchActors(_db, _searchText, _excludedPeople);
            SearchResults = results;
        }
        Searching = false;
    }
}
