using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Dreamward.ViewModels;

namespace Dreamward.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        const double step = 12.0;

        switch (e.Key)
        {
            case Key.W or Key.Up:
                _viewModel.MovePlayer(0, -step);
                break;
            case Key.S or Key.Down:
                _viewModel.MovePlayer(0, step);
                break;
            case Key.A or Key.Left:
                _viewModel.MovePlayer(-step, 0);
                break;
            case Key.D or Key.Right:
                _viewModel.MovePlayer(step, 0);
                break;
            case Key.E or Key.Enter:
                _viewModel.TriggerSavePoint();
                break;
            case Key.Tab:
                TransitionRoomWithFade("HallOfEchoes");
                break;
        }
    }

    /// <summary>
    /// Плавный переход между комнатами с анимацией затемнения экрана Fade-Out / Fade-In через Opacity (п. 4.1 ТЗ)
    /// </summary>
    public void TransitionRoomWithFade(string nextLocationId)
    {
        var fadeOut = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(350));
        fadeOut.Completed += (_, _) =>
        {
            _viewModel.CurrentLocation = nextLocationId;
            var fadeIn = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(350));
            FadeOverlay.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        };
        FadeOverlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);
    }
}
