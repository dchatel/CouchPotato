using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using CouchPotato.Views.OkDialog;

using GongSolutions.Wpf.DragDrop;

using Microsoft.EntityFrameworkCore;

using PostSharp.Patterns.Model;

using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;

namespace CouchPotato.Views.VideoEditor;

public class VideoEditorViewModel : ContentViewModel, IDropTarget
{
    private readonly DataContext db;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ChangePosterCommand { get; }
    public ICommand ChangeBackgroundCommand { get; }
    public ICommand AddRoleCommand { get; }
    public ICommand DeleteRoleCommand { get; }

    public Video Video { get; }
    public string? PosterUrl { get; set; }
    public string? BackgroundUrl { get; set; }
    public IEnumerable<Selectable> Genres { get; private set; }
    public ObservableCollection<RoleViewModel> Roles { get; }

    public VideoEditorViewModel(int videoId) : base(autoClose: false)
    {
        db = new DataContext();

        SaveCommand = new AsyncRelayCommand(Save);
        CancelCommand = new RelayCommand(() => Close(false));
        ChangePosterCommand = new RelayCommand(() => PosterUrl = GetFile() ?? PosterUrl);
        ChangeBackgroundCommand = new RelayCommand(() => BackgroundUrl = GetFile() ?? BackgroundUrl);
        AddRoleCommand = new AsyncRelayCommand(AddRole);
        DeleteRoleCommand = new RelayCommand<RoleViewModel>(DeleteRole);

        Video = db.Videos
            .Include(v => v.Genres)
            .Include(v => v.Roles).ThenInclude(r => r.Person)
            .Single(v => v.Id == videoId);
        PosterUrl = Video.PosterUrl;
        BackgroundUrl = Video.BackgroundUrl;
        Genres = db.Genres
            .ToList()
            .Select(g => new Selectable(g, Video.Genres.Any(vg => vg.Id == g.Id)))
            .ToList();
        Roles = new(from r in Video.Roles
                    select new RoleViewModel(r));
    }

    private async Task AddRole()
    {
        var addRoleViewModel = new ActorFinderViewModel(Video.Roles.Select(r=>r.Person));
        if (await addRoleViewModel.Show() && addRoleViewModel.SelectedPerson is not null)
        {
            var role = new Role
            {
                Person = addRoleViewModel.SelectedPerson,
                Order = Video.Roles.Max(r => r.Order) + 1,
            };
            Video.Roles.Add(role);
            Roles.Add(new RoleViewModel(role));
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

    public async Task Save()
    {
        foreach (var genre in Genres)
        {
            if (genre.IsSelected == true && Video.Genres.Contains(genre.Value) == false)
                Video.Genres.Add((Genre)genre.Value);
            else if (genre.IsSelected == false && Video.Genres.Contains(genre.Value) == true)
                Video.Genres.Remove((Genre)genre.Value);
        }

        ChangeImage("poster", PosterUrl, Video.PosterUrl);
        ChangeImage("background", BackgroundUrl, Video.BackgroundUrl);

        await db.SaveChangesAsync();
        Close(true);

        void ChangeImage(string type, string? newUrl, string? oldUrl)
        {
            if (oldUrl != newUrl)
            {
                if (oldUrl is not null)
                    File.Delete(oldUrl);

                Action<string?> set = type switch
                {
                    "poster" => v => Video.PosterUrl = v,
                    "background" => v => Video.BackgroundUrl = v,
                    _ => throw new NotImplementedException()
                };

                if (newUrl is not null)
                {
                    try
                    {
                        var stem = Path.GetInvalidFileNameChars().Aggregate(Video.Title, (current, c) => current.Replace(c, '_'));
                        var dstFile = $"Images/{stem}.{type}.{Guid.NewGuid()}{Path.GetExtension(newUrl)}";
                        File.Copy(newUrl, dstFile, overwrite: false);
                        set(dstFile);
                    }
                    catch { }
                }
            }
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