using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace MinecraftModManagerV2
{
    /// <summary>
    /// Logique d'interaction pour Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        #region Public Fields

        public static Home GlobalHome;

        #endregion Public Fields

        #region Public Constructors

        public Home()
        {
            GlobalHome = this;
            InitializeComponent();
            ListMods = new List<ListMod>();
            foreach (var mod in MainWindow.mods)
            {
                ListMods.Add(new ListMod(mod));
            }
            ListMods.Sort((left, right) => left.linkedMod.Infos.name.CompareTo(right.linkedMod.Infos.name));
            updateList();
        }

        #endregion Public Constructors

        #region Public Properties

        public List<ListMod> ListMods { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void updateList()
        {
            mainPannel.Children.Clear();
            var input = searchBar.Text.ToLower();

            foreach (var mod in ListMods)
            {
                if (mod.linkedMod.SearchString1.Length > 0)
                {
                    int validated = 0;
                    int index = 0;
                    for (int i = 0; i < input.Length; i++)
                    {
                        for (int j = index; j < mod.linkedMod.SearchString1.Length; j++)
                        {
                            if (mod.linkedMod.SearchString1[j] == input[i])
                            {
                                validated++;
                                index = j + 1;
                                break;
                            }
                        }
                    }
                    if (validated == input.Length)
                    {
                        mainPannel.Children.Add(mod);
                    }
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var mod in MainWindow.mods)
            {
                mod.Enabled = true;
                mod.ChangeState();
            }
            foreach (ListMod displayMod in mainPannel.Children)
            {
                displayMod.UpdateStatus();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var mod in MainWindow.mods)
            {
                mod.Enabled = false;
                mod.ChangeState();
            }
            foreach (ListMod displayMod in mainPannel.Children)
            {
                displayMod.UpdateStatus();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "Mods|*.jar;*.zip|Tous les fichiers|*.*",
                Multiselect = true,
                Title = "Importer un mod"
            };
            if (dialog.ShowDialog(MainWindow.App) == true)
            {
                var progress = new LoadingPage();
                var displayProgress = new BaseModel(false);
                displayProgress.Child = progress;
                displayProgress.Owner = MainWindow.App;
                Task.Factory.StartNew(() =>
                {
                    int max = dialog.FileNames.Length;
                    int current = 0;
                    var newMods = new List<Mod>();
                    foreach (var file in dialog.FileNames)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            progress.ChangeDisplayer("Chargement de : " + Path.GetFileNameWithoutExtension(file) + "...");
                            progress.ChangeFill((float)current / max);
                        });
                        if (MainWindow.mods.FirstOrDefault((m) => m.Filename == Path.GetFileName(file)) == null)
                        {
                            File.Copy(file, Path.Combine(MainWindow.ModDir, Path.GetFileName(file)), true);
                            var mod = MainWindow.LoadModFromFile(Path.Combine(MainWindow.ModDir, Path.GetFileName(file)));
                            mod.Enabled = true;
                            Dispatcher.Invoke(() => ListMods.Add(new ListMod(mod)));
                            MainWindow.mods.Add(mod);
                        }
                        current++;
                    }
                    ListMods.Sort((left, right) => left.linkedMod.Infos.name.CompareTo(right.linkedMod.Infos.name));
                    Dispatcher.Invoke(() =>
                    {
                        displayProgress.ForceClosing();
                    });
                });
                displayProgress.ShowDialog();
                MainWindow.mods.Sort((left, right) => left.Infos.name.CompareTo(right.Infos.name));
                updateList();
            }
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateList();
        }

        #endregion Private Methods
    }
}