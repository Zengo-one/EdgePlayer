using EdgePlayer;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;


namespace MainProject
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class EdgePlayer : Window
    {
        Player player;
        
        private bool IsTrackListToggle;
        private bool IsPlaying = false;
        private DispatcherTimer timer;
        private double previousVolume = 1;
        private PlayMode Mode;

        public EdgePlayer()
        {
            player = Player.Source;

            if (player.GetRoot())
            {
                InitializeComponent();
                player = Player.Source;
                MainMedia.LoadedBehavior = MediaState.Manual;
                TrackTimePosition.ApplyTemplate();
                Mode = PlayMode.Simple;
                DataContext = player;
            }
            else
            {
                MessageBox.Show("Превышено количество запусков!", "Ошибка");
                Close();
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TrackListBorder.Height = 0;

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(TimerTick);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        private void CloseProgramButton_Click(object sender, RoutedEventArgs e)
        {
            MainMedia.Stop();
            Close();
        }

        private void MinimizeProgramButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainMedia.Volume = VolumeSlider.Value;
            if (VolumeSlider.Value == 0)
                VolumeButton.Background = new ImageBrush(new BitmapImage(new Uri("Icons/volume-off.png", UriKind.Relative)));
            else if (VolumeSlider.Value > 0 && VolumeSlider.Value < 0.6)
                VolumeButton.Background = new ImageBrush(new BitmapImage(new Uri("Icons/reduced-volume.png", UriKind.Relative)));
            else
                try
                {
                    VolumeButton.Background = new ImageBrush(new BitmapImage(new Uri("Icons/speaker-filled-audio-tool.png", UriKind.Relative)));
                }
                catch
                { }
        }

        private void ListButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation();
            QuadraticEase ease = new QuadraticEase();
            ease.EasingMode = EasingMode.EaseInOut;
            da.EasingFunction = ease;

            if (!IsTrackListToggle)
            {
                da.To = 337;
                da.Duration = TimeSpan.FromSeconds(0.30);
                TrackListBorder.BeginAnimation(Border.HeightProperty, da);
                ListButton.Background = new ImageBrush(new BitmapImage(new Uri("Icons/sort-up.png", UriKind.Relative)));
                IsTrackListToggle = true;
            }

            else
            {
                da.To = 0;
                da.Duration = TimeSpan.FromSeconds(0.30);
                TrackListBorder.BeginAnimation(Border.HeightProperty, da);
                ListButton.Background = new ImageBrush(new BitmapImage(new Uri("Icons/justify-align.png", UriKind.Relative)));
                IsTrackListToggle = false;
            }
        }

        private void TrackTimePosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CurTimeText.Text = MainMedia.Position.ToString().Split('.')[0];
        }

        private void AddOneOrMoreTracks_Click(object sender, RoutedEventArgs e)
        {
            var res = player.AddOneOrMoreTracks();
            if (MainMedia.Source == null && res)
            {
                LoadFullSong();
                MainMedia.Play();
                IsPlaying = true;
            }
        }

        private void LoadFullSong()
        {
            MainMedia.Source = player.LoadSong();
            MediaImage.Source = player.LoadImage();
            TitleTextBlock.Text = player.LoadTitle();
            PerfomersTextBlock.Text = player.LoadPerfomers();
        }

        private void SetValueByTime_MediaOpened(object sender, RoutedEventArgs e)
        {
            TrackTimePosition.Value = MainMedia.Position.TotalSeconds;
            CurTimeText.Text = MainMedia.Position.ToString().Split('.')[0];
            TrackTimePosition.Maximum = MainMedia.NaturalDuration.TimeSpan.TotalSeconds;
            FullTimeText.Text = MainMedia.NaturalDuration.ToString().Split('.')[0];
        }

        private void TimerTick(object sender, EventArgs e)
        {
            TrackTimePosition.Value = MainMedia.Position.TotalSeconds;
            CurTimeText.Text = MainMedia.Position.ToString().Split('.')[0];
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            PausePlay();
        }

        private void NextTrack_Click(object sender, RoutedEventArgs e)
        {
            PlayNextTrack();
        }

        private void PlayNextTrack()
        {
            if (MainMedia.Source == null)
                return;

            MainMedia.Stop();

            if (!player.NextTrack(Mode))
            {
                FullTimeText.Text = "00:00:00";
                MediaImage.Source = null;
                TitleTextBlock.Text = "";
                PerfomersTextBlock.Text = "";
                IsPlaying = false;
            }
            else
            {
                LoadFullSong();
                if (!IsPlaying)
                    PausePlay();
                else
                    MainMedia.Play();
            }

            Thread.Sleep(500);
        }

        private void PreviousTrack_Click(object sender, RoutedEventArgs e)
        {
            if (MainMedia.Source == null)
                return;

            MainMedia.Stop();
            player.PreviousTrack(Mode);
            LoadFullSong();
            if (!IsPlaying)
                PausePlay();
            else
                MainMedia.Play();

            Thread.Sleep(500);
        }

        private void SliderThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            if (MainMedia.Source == null)
                return;
            timer.Stop();
            MainMedia.Pause();
        }

        private void SliderThumbDragCompleted(object sender, EventArgs e)
        {
            if (MainMedia.Source == null)
                return;
            MainMedia.Position = TimeSpan.FromSeconds(TrackTimePosition.Value);
            TrackTimePosition.Value = MainMedia.Position.TotalSeconds;
            CurTimeText.Text = MainMedia.Position.ToString().Split('.')[0];
            MainMedia.Play();
            timer.Start();
        }

        private void WindowMove_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void AutoPlayNextTrack_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayNextTrack();
        }

        private void ModeButton_Click(object sender, RoutedEventArgs e)
        {
            switch (Mode)
            {
                case PlayMode.Simple:
                    {
                        Mode = PlayMode.RepeatTrackList;
                        ModeButton.Background = new ImageBrush(new BitmapImage(new Uri("Icons/repeat.png", UriKind.Relative)));
                        break;
                    }
                case PlayMode.RepeatTrackList:
                    {
                        Mode = PlayMode.RepeatSong;
                        ModeButton.Background = new ImageBrush(new BitmapImage(new Uri("Icons/repeat (1).png", UriKind.Relative)));
                        break;
                    }
                case PlayMode.RepeatSong:
                    {
                        Mode = PlayMode.Random;
                        ModeButton.Background = new ImageBrush(new BitmapImage(new Uri("Icons/shuffle.png", UriKind.Relative)));
                        break;
                    }
                case PlayMode.Random:
                    {
                        Mode = PlayMode.Simple;
                        ModeButton.Background = new ImageBrush(new BitmapImage(new Uri("Icons/play.png", UriKind.Relative)));
                        break;
                    }
                default:
                    break;
            }
        }

        private void PausePlay()
        {
            if (MainMedia.Source == null)
                return;
            if (IsPlaying)
            {
                MainMedia.Pause();
                timer.Stop();
                PauseButton.Background = new ImageBrush(new BitmapImage(new Uri("Icons/play.png", UriKind.Relative)));
                IsPlaying = false;
            }
            else
            {
                MainMedia.Play();
                timer.Start();
                PauseButton.Background = new ImageBrush(new BitmapImage(new Uri("Icons/pause.png", UriKind.Relative)));
                IsPlaying = true;
            }
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (VolumeSlider.Value != 0)
            {
                previousVolume = VolumeSlider.Value;
                VolumeSlider.Value = 0;
            }
            else
                VolumeSlider.Value = previousVolume;
        }

        private void TrackList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainMedia.Stop();

            LoadFullSong();

            if (!IsPlaying)
                PausePlay();
            else
                MainMedia.Play();

            TrackList.ScrollIntoView(TrackList.SelectedItem);
        }

        private void MenuClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            player.SaveAsTracklist();
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            player.SaveTracklist();
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            if (player.OpenTracklist())
            {
                LoadFullSong();
                MainMedia.Play();
                PlaylistTitle.Text = player.TracklistName;
            }
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            PassWindow pw = new PassWindow();
            if (pw.ShowDialog() == true)
                player.SetRoot(pw.passwordBox.Password);
        }
    }
}
