using Microsoft.Win32;
using System.Diagnostics;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Linq;

namespace MainProject
{
    public enum PlayMode
    {
        Simple,
        Random,
        RepeatSong,
        RepeatTrackList
    }
    [DataContract]
    internal sealed class Player :INotifyPropertyChanged
    {
        private static readonly Lazy<Player> lazy =
            new Lazy<Player>(() => new Player());

        public static Player Source { get { return lazy.Value; } }

        private DispatcherTimer checkingTimer;

        [DataMember]
        private ObservableCollection<Song> songs;
        public ObservableCollection<Song> Songs
        {
            get
            {
                return songs;
            }
            set
            {
                songs = value;
                OnPropertyChanged("Songs");
            }
        }

        private Song selectedSong;
        public Song SelectedSong
        {
            get
            {
                return selectedSong;
            }
            set
            {
                if (songs.Count != 0)
                {
                    selectedSong = value;
                    OnPropertyChanged("SelectedSong");
                }
            }
        }

        private TagLib.File SelectedSongFile;
        private string TracklistPath;
        public string TracklistName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(TracklistPath);
            }
            private set
            {
                TracklistPath = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        RegistryKey key;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private Player()
        {
            Songs = new ObservableCollection<Song>();

            LoadInfoRegister();
            key = Registry.CurrentUser.OpenSubKey("Software\\Edge Player\\Root", true);
            checkingTimer = new DispatcherTimer();
            checkingTimer.Tick += new EventHandler(Tick);
            checkingTimer.Interval = new TimeSpan(0, 10, 0);
            checkingTimer.Start();
        }

        private void Tick(object sender, EventArgs e)
        {
            CheckingExistence();
        }

        public bool AddOneOrMoreTracks()
        {
            OpenFileDialog of = new OpenFileDialog
            {
                Filter = "Музыкальные файлы(*.mp3;*.wav)|*.mp3;*.wav" + "|Все файлы (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = true
            };
            of.ShowDialog();

            if (of.FileName != string.Empty)
            {
                foreach (var i in of.FileNames)
                {
                    try
                    {
                        var song = TagLib.File.Create(i);

                        Song s;
                        if (song.Tag.Performers != null && song.Tag.Title != null)
                            s = new Song(
                                song.Name,
                                song.Tag.Title + "\n" + string.Join(", ", song.Tag.Performers),
                                song.Properties.Duration.ToString("mm\\:ss"));

                        else
                            s = new Song(
                                song.Name,
                                System.IO.Path.GetFileNameWithoutExtension(song.Name) + "\n",
                                song.Properties.Duration.ToString("mm\\:ss"));

                        Songs.Add(s);
                    }
                    catch { }
                }
                return true;
            }
            return false;
        }

        public Uri LoadSong()
        {
            if (SelectedSong == null)
                SelectedSong = Songs[0];
            try
            {
                SelectedSongFile = TagLib.File.Create(SelectedSong.Ref);
                return new Uri(SelectedSongFile.Name);
            }
            catch
            {
                SelectedSongFile = null;
                return null;
            }
        }

        public string LoadTitle()
        {
            if (SelectedSongFile == null)
                return "";
            if (SelectedSongFile.Tag.Performers != null && SelectedSongFile.Tag.Title != null)
                return SelectedSongFile.Tag.Title;
            else
                return System.IO.Path.GetFileNameWithoutExtension(SelectedSongFile.Name);
        }

        public string LoadPerfomers()
        {
            if (SelectedSongFile == null)
                return "";
            if (SelectedSongFile.Tag.Performers != null && SelectedSongFile.Tag.Title != null)
                return string.Join(", ", SelectedSongFile.Tag.Performers);
            else
                return "";
        }

        public BitmapImage LoadImage()
        {
            if (SelectedSongFile == null)
                return null;
            try
            {
                var file = TagLib.File.Create(SelectedSong.Ref);
                TagLib.IPicture pic = file.Tag.Pictures[0];
                MemoryStream ms = new MemoryStream(pic.Data.Data);
                ms.Seek(0, SeekOrigin.Begin);

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                return bitmap;
            }
            catch (Exception ex)
            {
                if (ex is NotSupportedException || ex is IndexOutOfRangeException)
                {
                    return null;
                }
            }
            return null;
        }
        
        public bool NextTrack(PlayMode _mode)
        {
            switch (_mode)
            {
                case PlayMode.Simple:
                    {
                        if (Songs[Songs.Count - 1] != SelectedSong)
                        {
                            SelectedSong = Songs[Songs.IndexOf(SelectedSong)+1];
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case PlayMode.RepeatSong:
                    {
                        return true;
                    }
                case PlayMode.RepeatTrackList:
                    {
                        if (Songs[Songs.Count - 1] != SelectedSong)
                        {
                            SelectedSong = Songs[Songs.IndexOf(SelectedSong) + 1];
                            return true;
                        }
                        else
                        {
                            SelectedSong = Songs[0];
                            return true;
                        }
                    }
                case PlayMode.Random:
                    {
                        Random rand = new Random();
                        int index;
                        do
                        {
                            index = rand.Next(Songs.Count);
                        }
                        while (Songs[index] == SelectedSong);

                        SelectedSong = Songs[index];

                        return true;
                    }
                default:
                    return false;
            }
        }

        public void PreviousTrack(PlayMode _mode)
        {
            switch (_mode)
            {
                case PlayMode.Simple:
                    {
                        if (Songs[0] != SelectedSong)
                        {
                            SelectedSong = Songs[Songs.IndexOf(SelectedSong) - 1];
                        }
                        break;
                    }
                case PlayMode.RepeatSong:
                    {
                        break;
                    }
                case PlayMode.RepeatTrackList:
                    {
                        if (Songs[0] != SelectedSong)
                        {
                            SelectedSong = Songs[Songs.IndexOf(SelectedSong) - 1];
                        }
                        else
                        {
                            SelectedSong = Songs[Songs.Count - 1];
                        }
                        break;
                    }
                case PlayMode.Random:
                    {
                        Random rand = new Random();
                        int index;
                        do
                        {
                            index = rand.Next(Songs.Count);
                        }
                        while (Songs[index] == SelectedSong);

                        SelectedSong = Songs[index];
                        break;
                    }
                default:
                    break;
            }
        }

        public void SaveTracklist()
        {
            if (TracklistPath == null)
            {
                SaveAsTracklist();
            }
            else
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ObservableCollection<Song>));
                using (FileStream fs = new FileStream(TracklistPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    jsonSerializer.WriteObject(fs, songs);
                }
            }
        }

        public void SaveAsTracklist()
        {
            if (songs.Count == 0)
            {
                MessageBox.Show(
                          "Плейлист не имеет песен",
                          "Сообщение",
                          MessageBoxButton.OK,
                          MessageBoxImage.Error,
                          MessageBoxResult.OK,
                          MessageBoxOptions.DefaultDesktopOnly
                          );
                return;
            }

            SaveFileDialog saveFile = new SaveFileDialog
            {
                Filter = "Плейлисты (*.epl)|*.epl" + "|Все файлы (*.*)|*.*"
            };

            if (saveFile.ShowDialog() == true)
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ObservableCollection<Song>));
                using (FileStream fs = new FileStream(saveFile.FileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    jsonSerializer.WriteObject(fs, songs);
                }
            }
        }

        public bool OpenTracklist()
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true,
                Filter = "Плейлисты (*.epl)|*.epl"
            };

            open.ShowDialog();

            if(open.FileName != string.Empty)
            {
                if (Songs.Count != 0)
                {
                    SelectedSong = null;
                    Songs.Clear();
                }

                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ObservableCollection<Song>));
                using (FileStream fs = new FileStream(open.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    try
                    {
                        Songs = (ObservableCollection<Song>)jsonSerializer.ReadObject(fs);

                        CheckingExistence();

                        if (songs.Count != 0)
                        {
                            TracklistPath = open.FileName;
                            return true;
                        }
                        else
                            MessageBox.Show(
                                "Плейлист не имеет песен",
                                "Сообщение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK,
                                MessageBoxOptions.DefaultDesktopOnly
                                );
                    }
                    catch
                    {
                        MessageBox.Show(
                            "Плейдист поврежден",
                            "Сообщение",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.OK,
                            MessageBoxOptions.DefaultDesktopOnly
                            );
                        return false;
                    }
                }
            }
            return false;
        }

        private void CheckingExistence()
        {
            for(var i = 0; i < Songs.Count; )
            {
                if (!File.Exists(Songs[i].Ref))
                {
                    Songs.RemoveAt(i);
                }
                else
                    i++;
            }
        }

        private void LoadInfoRegister()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

            try
            {
                if (key.OpenSubKey("Edge Player") != null)
                    return;

                RegistryKey param;
                RegistryKey subKey = key.CreateSubKey("Edge Player");
                subKey.OpenSubKey("Edge Player");

                subKey.CreateSubKey("Name");
                param = subKey.OpenSubKey("Name", true);
                param.SetValue("Name", "Edge Player");

                subKey.CreateSubKey("Version");
                param = subKey.OpenSubKey("Version", true);
                param.SetValue("Version", "0.9");

                subKey.CreateSubKey("Root");
                param = subKey.OpenSubKey("Root", true);
                param.SetValue("Enabled", "false");

                key = Registry.CurrentUser.OpenSubKey("Software\\Edge Player\\Root", true);

                using (var deriveBytes = new Rfc2898DeriveBytes("TryToRoot", 20))
                {
                    byte[] salt = deriveBytes.Salt;
                    byte[] passkey = deriveBytes.GetBytes(20);
                    param.SetValue("Pass", passkey);
                    param.SetValue("Salt", salt);
                }
            }
            catch { }
        }

        public bool GetRoot()
        {
            try
            {
                if ((string)key.GetValue("Enabled") == "true")
                {
                    return true;
                }
                else
                {
                    Process name = Process.GetCurrentProcess();
                    Process[] processes;
                    try
                    {
                        processes = Process.GetProcessesByName(name.ProcessName);
                    }
                    catch
                    {
                        processes = null;
                    }
                    if (processes != null)
                    {
                        if (processes.Length <= 5)
                            return true;
                        else
                            return false;
                    }
                    else
                        return true;
                }
            }

            catch { return true; }
        }

        public void SetRoot(string pass)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(pass, (byte[])key.GetValue("Salt")))
            {
                byte[] newKey = deriveBytes.GetBytes(20);
                byte[] oldKey = (byte[])key.GetValue("pass");

                if (newKey.SequenceEqual(oldKey))
                {
                    key.SetValue("Enabled", "true");
                }
                else
                    MessageBox.Show("Неверный пароль!", "Ошибка");
            }
        }
    }
}
