using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using CouchPotato.DbModel;
using CouchPotato.Properties;
using CouchPotato.Views.ActorFinderDialog;
using CouchPotato.Views.InputDialog;

using GongSolutions.Wpf.DragDrop;

using Microsoft.EntityFrameworkCore;

using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;

namespace CouchPotato.Views.VideoEditor;

public class VideoEditorViewModel : ContentViewModel, IDropTarget
{
    public static VideoEditorViewModel Create(DataContext db, Video video)
    {
        VideoEditorViewModel videoEditorViewModel = video switch
        {
            Movie => new VideoEditorViewModel(db, video.Id),
            TVShow => new TVEditorViewModel(db, video.Id),
            _ => new VideoEditorViewModel(db, video.Id),
        };
        return videoEditorViewModel;
    }

    protected readonly DataContext _db;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ChangePosterCommand { get; }
    public ICommand ChangeBackgroundCommand { get; }
    public ICommand AddRoleCommand { get; }
    public ICommand DeleteRoleCommand { get; }

    public Video Video { get; }
    public string? PosterUrl { get; set; }
    public string? BackgroundUrl { get; set; }
    public IEnumerable<Selectable<Genre>> Genres { get; private set; }
    public ObservableCollection<RoleViewModel> Roles { get; }

    public VideoEditorViewModel(DataContext db, int videoId) : base(autoClose: false)
    {
        _db = db;

        SaveCommand = new RelayCommand(() => Close(true));
        CancelCommand = new RelayCommand(() => Close(false));
        ChangePosterCommand = new RelayCommand(() => PosterUrl = GetFile() ?? PosterUrl);
        ChangeBackgroundCommand = new RelayCommand(() => BackgroundUrl = GetFile() ?? BackgroundUrl);
        AddRoleCommand = new AsyncRelayCommand(AddRole);
        DeleteRoleCommand = new RelayCommand<RoleViewModel>(DeleteRole);

        Video = _db.Videos
            .Include(v => v.Genres)
            .Include(v => v.Roles).ThenInclude(r => r.Person)
            .Single(v => v.Id == videoId);
        if (Video is TVShow tv)
        {
            _db.Entry(tv).Collection(t => t.Seasons).Load();
        }
        PosterUrl = Video.PosterUrl;
        BackgroundUrl = Video.BackgroundUrl;
        Genres = _db.Genres
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
    }

    private async Task AddRole()
    {
        var addRoleViewModel = new ActorFinderViewModel(Video.Roles.Select(r => r.Person));
        if (await addRoleViewModel.Show() && addRoleViewModel.SelectedPerson is not null)
        {
            var role = new Role
            {
                Person = addRoleViewModel.SelectedPerson,
                Order = Video.Roles.Max(r => r.Order) + 1,
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
            Video.Roles.Remove(roleViewModel.Role);
            Roles.Remove(roleViewModel);
        }
    }

    private static string? GetFile()
    {
        var ofd = new System.Windows.Forms.OpenFileDialog
        {
            Title = Loc.SelectNewImage,
        };
        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            return ofd.FileName;
        }
        else
        {
            return null;
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

public class RoleViewModel : INotifyPropertyChanged
{
    public ICommand EditCommand { get; }

    public Role Role { get; }
    public int Order
    {
        get => Role.Order;
        set {
            Role.Order = value;
            OnPropertyChanged();
        }
    }

    public RoleViewModel(Role role)
    {
        Role = role;
        EditCommand = new AsyncRelayCommand(Edit);
    }

    private async Task Edit()
    {
        var inputDialog = new InputDialogViewModel(Loc.InputCharacterRole)
        {
            InputText = Role.Characters
        };
        if (await inputDialog.Show())
        {
            Role.Characters = inputDialog.InputText;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        => PropertyChanged?.Invoke(this, new(propertyName));
}