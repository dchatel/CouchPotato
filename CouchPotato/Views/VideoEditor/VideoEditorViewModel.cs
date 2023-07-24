using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using CouchPotato.DbModel;

using PostSharp.Aspects.Advices;

namespace CouchPotato.Views.VideoEditor;

public class VideoEditorViewModel : ContentViewModel
{
    public Video Video { get; }

    public VideoEditorToolbarViewModel Toolbar { get; }

    public VideoEditorViewModel(Video video)
    {
        Video = video;

        Toolbar = new VideoEditorToolbarViewModel(this);
        TitleBarRegion = Toolbar;
    }

    public override bool CanAutoClose => false;
}

public class VideoEditorToolbarViewModel
{
    private readonly VideoEditorViewModel videoEditorViewModel;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public VideoEditorToolbarViewModel(VideoEditorViewModel videoEditorViewModel)
    {
        this.videoEditorViewModel = videoEditorViewModel;
        SaveCommand = new RelayCommand(() => videoEditorViewModel.Close(true));
        CancelCommand = new RelayCommand(() => videoEditorViewModel.Close(false));
    }
}
