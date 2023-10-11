using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CouchPotato.DbModel;
using CouchPotato.DbModel.OtherDbModels.Tmdb;

namespace CouchPotato.Views.ActorFinderDialog;

public class ActorFinderViewModel : ContentViewModel
{
    private string? searchText;

    public string? SearchText
    {
        get => searchText;
        set {
            searchText = value;
            Task.Run(Search);
        }
    }

    public string? Characters { get; set; }
    public IEnumerable<Person> SearchResults { get; set; }
    public Person? SelectedPerson { get; set; }
    public bool Searching { get; set; }

    public ActorFinderViewModel() : base(autoClose: true)
    {
        SearchResults = Enumerable.Empty<Person>();
    }

    private async Task Search()
    {
        Searching = true;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            SearchResults = Enumerable.Empty<Person>();
        }
        else
        {
            var results = await Tmdb.SearchActors(searchText);
            SearchResults = results;
        }
        Searching = false;
    }
}