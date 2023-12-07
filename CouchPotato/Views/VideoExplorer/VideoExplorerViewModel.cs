using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using CouchPotato.DbModel;
using CouchPotato.Views.VideoEditor;

using Microsoft.EntityFrameworkCore;

namespace CouchPotato.Views.VideoExplorer;

public class VideoExplorerViewModel : ContentViewModel
{
    private SearchResultViewModel? _selectedResult;
    private string _searchText;

    public ICommand EditCommand { get; }
    public ICommand SearchCommand { get; }
    public IEnumerable<SearchResultViewModel>? SearchResults { get; set; }
    public bool IsSearching { get; set; }

    public SearchResultViewModel? SelectedResult
    {
        get => _selectedResult;
        set {
            _selectedResult = value;
            _selectedResult?.LoadData();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set {
            _searchText = value;
            Task.Run(SearchAsync);
        }
    }

    public VideoExplorerViewModel() : base(autoClose: false)
    {
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        EditCommand = new AsyncRelayCommand(Edit);
        IsSearching = false;
        _searchText = "";
    }

    private async Task Edit()
    {
        if (SelectedResult is null) return;

        var db = new DataContext();
        VideoEditorViewModel videoEditor = VideoEditorViewModel.Create(db, SelectedResult.Video);
        if (await videoEditor.Show())
        {
            SelectedResult.Video = videoEditor.Video;

            ChangeImage("poster", videoEditor.PosterUrl, videoEditor.Video.PosterUrl);
            ChangeImage("background", videoEditor.BackgroundUrl, videoEditor.Video.BackgroundUrl);

            await db.SaveChangesAsync();

            void ChangeImage(string type, string? newUrl, string? oldUrl)
            {
                if (oldUrl != newUrl)
                {
                    if (oldUrl is not null)
                        File.Delete(oldUrl);

                    Action<string?> set = type switch
                    {
                        "poster" => v => videoEditor.Video.PosterUrl = v,
                        "background" => v => videoEditor.Video.BackgroundUrl = v,
                        _ => throw new NotImplementedException()
                    };

                    if (newUrl is not null)
                    {
                        try
                        {
                            var stem = Utils.GetValidFileName(videoEditor.Video.Title);
                            var dstFile = $"Images/{stem}.{type}.{Guid.NewGuid()}{Path.GetExtension(newUrl)}";
                            File.Copy(newUrl, dstFile, overwrite: false);
                            set(dstFile);
                        }
                        catch { }
                    }
                }
            }
        }
    }

    private async Task SearchAsync()
    {
        IsSearching = true;
        using var db = new DataContext();

        SearchResults = await Task.Run(() => db.Videos
            .Where(video => video.Title.ToLower().Contains(SearchText.ToLower()))
            .OrderBy(video => video.Title.ToLower())
            .Select(video => SearchResultViewModel.Create(video))
            .ToArray());
        SelectedResult = SearchResults.FirstOrDefault();
        IsSearching = false;
    }
}