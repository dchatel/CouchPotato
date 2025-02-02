using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using CouchPotato.DbModel;
using CouchPotato.DbModel.OtherDbModels.Tmdb;
using CouchPotato.Properties;
using CouchPotato.Views.InputDialog;
using CouchPotato.Views.WebSearchDialogs;

using GongSolutions.Wpf.DragDrop;

using SixLabors.ImageSharp;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;

namespace CouchPotato.Views.VideoEditor;

public partial class VideoEditorViewModel : ContentViewModel, IDropTarget
{
    private readonly DataContext _db;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SyncCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ChangePosterCommand { get; }
    public ICommand ChangeBackgroundCommand { get; }
    public ICommand AddRoleCommand { get; }
    public ICommand DeleteRoleCommand { get; }
    public ICommand AddSeasonCommand { get; }
    public ICommand DeleteSeasonCommand { get; }
    public ICommand MakeMovieCommand { get; }
    public ICommand MakeTVShowCommand { get; }

    [ObservableProperty]
    public Video _video;

    public bool EditionMode { get; }
    public bool VideoWasRemoved { get; private set; }
    public IEnumerable<Selectable<Genre>> Genres { get; private set; }
    public ObservableCollection<RoleViewModel> Roles { get; }
    public ObservableCollection<SeasonViewModel> Seasons { get; }

    public VideoType Type
    {
        get => Video.Type;
        set {
            if (Video.Type != value)
            {
                Video.Type = value;
                OnPropertyChanged();
            }
        }
    }

    public string? PosterUrl
    {
        get => Video.PosterUrl;
        set {
            if (Video.PosterUrl != value)
            {
                Video.PosterUrl = value;
                OnPropertyChanged();
            }
        }
    }

    public string? BackgroundUrl
    {
        get => Video.BackgroundUrl;
        set {
            if (Video.BackgroundUrl != value)
            {
                Video.BackgroundUrl = value;
                OnPropertyChanged();
            }
        }
    }

    public VideoEditorViewModel(DataContext db, Video video, bool editionMode) : base(autoClose: false)
    {
        _db = db;

        Video = video;
        EditionMode = editionMode;
        SaveCommand = new RelayCommand(() => Close(true));
        CancelCommand = new RelayCommand(() => Close(false));
        SyncCommand = new AsyncRelayCommand(Sync);
        DeleteCommand = new RelayCommand(Delete);
        ChangePosterCommand = new RelayCommand(() => Video.PosterUrl = ImageChange.AddImageChange(Video.PosterUrl, Video.Title, "poster", width: 200));
        ChangeBackgroundCommand = new RelayCommand(() => Video.BackgroundUrl = ImageChange.AddImageChange(Video.BackgroundUrl, Video.Title, "background"));
        AddRoleCommand = new AsyncRelayCommand(AddRole);
        DeleteRoleCommand = new RelayCommand<RoleViewModel>(DeleteRole);
        AddSeasonCommand = new RelayCommand(AddSeason);
        DeleteSeasonCommand = new RelayCommand<SeasonViewModel>(RemoveSeason);
        MakeMovieCommand = new RelayCommand(() =>
        {
            Type = VideoType.Movie;
        });
        MakeTVShowCommand = new RelayCommand(() =>
        {
            Type = VideoType.TVShow;
        });

        Genres = db.Genres
            .ToList()
            .Select(g => new Selectable<Genre>(g, Video.Genres.Any(vg => vg.Id == g.Id), (g, s) =>
            {
                if (s)
                {
                    Video.Genres.Add(g);
                }
                else
                {
                    Video.Genres.Remove(g);
                }
            }))
            .ToList();
        Roles = new(from r in Video.Roles
                    select new RoleViewModel(r));
        Seasons = new(Video.Seasons.Select(s => new SeasonViewModel(this, s)));
    }

    private void Delete()
    {
        VideoWasRemoved = true;
        Close(true);
    }

    private void AddSeason()
    {
        var seasonNumber = Seasons.Count == 0 ? 0 : Seasons.Select(s => s.Season.SeasonNumber).Max() + 1;
        var seasonName = $"{Loc.Season} {seasonNumber}";
        var season = new Season
        {
            SeasonNumber = seasonNumber,
            Name = seasonName,
        };
        Seasons.Add(new SeasonViewModel(this, season));
        Video.Seasons.Add(season);
    }

    private void RemoveSeason(SeasonViewModel? season)
    {
        if (season is not null)
        {
            Video.Seasons.Remove(season.Season);
            Seasons.Remove(season);
        }
    }

    private async Task Sync()
    {
        var videoWebSearchViewModel = new VideoWebSearchViewModel(Video.Type, Video.Title, Video.ReleaseDate?.Year);
        if (await videoWebSearchViewModel.Show() && videoWebSearchViewModel.SelectedVideo is not null)
        {
            var selectedVideo = await Tmdb.GetVideo(_db, videoWebSearchViewModel.SelectedVideo.TmdbId, videoWebSearchViewModel.SelectedVideo.MediaType);
            var videoSyncViewModel = new VideoSyncViewModel(Video, selectedVideo);
            if (await videoSyncViewModel.Show())
            {
                Video.Title = videoSyncViewModel.Title.Selected;
                Video.Tagline = videoSyncViewModel.Tagline.Selected;
                Video.Overview = videoSyncViewModel.Overview.Selected;
                Video.ReleaseDate = videoSyncViewModel.ReleaseDate.Selected;
                Video.Status = videoSyncViewModel.Status.Selected;
                Video.Origin = videoSyncViewModel.Origin.Selected;
                Video.OriginalTitle = videoSyncViewModel.OriginalTitle.Selected;
                Video.Version = videoSyncViewModel.Version.Selected;
                Video.PosterUrl = videoSyncViewModel.PosterUrl.Selected;
                Video.BackgroundUrl = videoSyncViewModel.BackgroundUrl.Selected;
                Video.TmdbId = videoSyncViewModel.TmdbId.Selected;
                Video.TmdbRating = videoSyncViewModel.TmdbRating.Selected;
                Video.TmdbRatingCount = videoSyncViewModel.TmdbRatingCount.Selected;
                Video.Runtime = videoSyncViewModel.Runtime.Selected;

                foreach (var genre in Genres)
                {
                    genre.IsSelected = videoSyncViewModel.Genres.Selected.Any(g => g.Id == genre.Value.Id);
                }

                var roles = videoSyncViewModel.Roles.Selected.Select(role => Video.Roles.SingleOrDefault(r => r.Person.TmdbId == role.Person.TmdbId) ?? role);
                Video.Roles.Clear();
                Roles.Clear();
                foreach (var role in roles)
                {
                    var roleViewModel = new RoleViewModel(role);
                    Video.Roles.Add(role);
                    Roles.Add(roleViewModel);
                }

                if (videoSyncViewModel.ReplaceSeasons)
                {
                    Video.Seasons.Clear();
                    Seasons.Clear();
                    foreach (var season in videoSyncViewModel.Seasons)
                    {
                        var seasonViewModel = new SeasonViewModel(this, season);
                        Video.Seasons.Add(season);
                        Seasons.Add(seasonViewModel);
                    }
                }
                else
                {
                    foreach (var season in videoSyncViewModel.Seasons)
                    {
                        var existingSeason = Seasons.SingleOrDefault(s => s.Season.SeasonNumber == season.SeasonNumber);
                        if (existingSeason is null)
                        {
                            var seasonViewModel = new SeasonViewModel(this, season);
                            Video.Seasons.Add(season);
                            Seasons.Add(seasonViewModel);
                        }
                        else
                        {
                            foreach (var episode in season.Episodes)
                            {
                                if (existingSeason.Episodes.All(e => e.EpisodeNumber != episode.EpisodeNumber))
                                {
                                    existingSeason.Episodes.Add(episode);
                                }
                            }
                        }
                    }
                }
                OnPropertyChanged(nameof(Video));
            }
        }
    }

    private async Task AddRole()
    {
        var addRoleViewModel = new ActorWebSearchViewModel(_db, Video.Roles.Select(r => r.Person));
        if (await addRoleViewModel.Show() && addRoleViewModel.SelectedPerson is not null)
        {
            var role = new Role
            {
                Person = addRoleViewModel.SelectedPerson,
                Order = Video.Roles.Count == 0 ? 0 : Video.Roles.Max(r => r.Order) + 1,
            };
            Video.Roles.Add(role);
            var roleViewModel = new RoleViewModel(role);
            var inputDialog = new InputDialogViewModel(Loc.InputCharacterRole)
            {
                InputText = role.Characters
            };
            if (await inputDialog.Show())
            {
                role.Characters = inputDialog.InputText;
                Roles.Add(roleViewModel);
            }
        }
    }

    private void DeleteRole(RoleViewModel? roleViewModel)
    {
        if (roleViewModel is not null)
        {
            roleViewModel.Remove();
            Roles.Remove(roleViewModel);
        }
    }

    public void DragOver(IDropInfo dropInfo)
    {
        if (dropInfo.Data is RoleViewModel && dropInfo.TargetItem is RoleViewModel)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Move;
        }
    }

    public void Drop(IDropInfo dropInfo)
    {
        if (dropInfo.Data is RoleViewModel source && dropInfo.TargetItem is RoleViewModel target)
        {
            int index = target.Order;

            if (source.Order < index)
            {
                if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.BeforeTargetItem))
                    index--;
                foreach (var r in from r in Roles
                                  where source.Order < r.Order
                                  && r.Order <= index
                                  select r)
                    r.Order--;
            }
            else
            {
                if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.AfterTargetItem))
                    index++;
                foreach (var r in from r in Roles
                                  where index <= r.Order
                                  && r.Order < source.Order
                                  select r)
                    r.Order++;
            }
            source.Order = index;
        }
    }
}

public partial class RoleViewModel : ObservableObject
{
    public ICommand EditCommand { get; }

    [ObservableProperty]
    private Role _role;

    public Person Person => Role.Person;

    public int Order
    {
        get => Role.Order;
        set {
            if (Role.Order == value) return;

            Role.Order = value;
            OnPropertyChanged();
        }
    }

    public string? Characters
    {
        get => Role.Characters;
        set {
            if (Role.Characters == value) return;

            Role.Characters = value;
            OnPropertyChanged();
        }
    }

    public RoleViewModel(Role role)
    {
        Role = role;
        EditCommand = new AsyncRelayCommand(Edit);
    }

    public void Remove()
    {
        Role.Video.Roles.Remove(Role);
    }

    private async Task Edit()
    {
        var inputDialog = new InputDialogViewModel(Loc.InputCharacterRole)
        {
            InputText = Characters
        };
        if (await inputDialog.Show())
        {
            Characters = inputDialog.InputText;
        }
    }
}

public class SeasonViewModel
{
    private readonly VideoEditorViewModel _videoEditorViewModel;

    public Season Season { get; }
    public ObservableCollection<Episode> Episodes { get; }
    public ICommand AddEpisodeCommand { get; }
    public ICommand EditEpisodeCommand { get; }
    public ICommand DeleteEpisodeCommand { get; }

    public SeasonViewModel(VideoEditorViewModel videoEditorViewModel, Season season)
    {
        AddEpisodeCommand = new AsyncRelayCommand(AddEpisode);
        EditEpisodeCommand = new AsyncRelayCommand<Episode>(EditEpisode);
        DeleteEpisodeCommand = new RelayCommand<Episode>(RemoveEpisode);
        _videoEditorViewModel = videoEditorViewModel;
        Season = season;
        Episodes = new(Season.Episodes);
        Episodes.CollectionChanged += Episodes_CollectionChanged;
    }

    private void Episodes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is not null)
                {
                    foreach (var item in e.NewItems)
                    {
                        if (item is Episode episode)
                        {
                            Season.Episodes.Add(episode);
                        }
                    }
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is not null)
                {
                    foreach (var item in e.OldItems)
                    {
                        if (item is Episode episode)
                        {
                            Season.Episodes.Remove(episode);
                        }
                    }
                }
                break;
        }
    }

    private async Task AddEpisode()
    {
        var episode = new Episode();
        if (await new EpisodeEditorViewModel(_videoEditorViewModel, episode).Show())
        {
            Episodes.Add(episode);
        }
    }


    private async Task EditEpisode(Episode? episode)
    {
        if (episode is not null)
        {
            await new EpisodeEditorViewModel(_videoEditorViewModel, episode).Show();
        }
    }

    private void RemoveEpisode(Episode? episode)
    {
        if (episode is not null)
        {
            Episodes.Remove(episode);
        }
    }
}

public class EpisodeEditorViewModel : ContentViewModel
{
    private readonly VideoEditorViewModel _videoEditorViewModel;

    public Episode Episode { get; set; }
    public ICommand ChangeEpisodeImageCommand { get; }

    public EpisodeEditorViewModel(VideoEditorViewModel videoEditorViewModel, Episode episode) : base(true)
    {
        _videoEditorViewModel = videoEditorViewModel;
        Episode = episode;

        ChangeEpisodeImageCommand = new RelayCommand(() => Episode.ImageUrl = ImageChange.AddImageChange(
            Episode.ImageUrl,
            $"{_videoEditorViewModel.Video.Title}",
            $"S{Episode.Season.SeasonNumber}E{Episode.EpisodeNumber}"
            ));
    }
}

