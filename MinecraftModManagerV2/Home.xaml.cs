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
        #region Public Constructors

        public Home()
        {
            InitializeComponent();
            updateList();
        }

        #endregion Public Constructors

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
                            progress.ChangeDisplayer("Chargement de : " + System.IO.Path.GetFileNameWithoutExtension(file) + "...");
                            progress.ChangeFill((float)current / max);
                        });
                        if (MainWindow.mods.FirstOrDefault((m) => m.Filename == Path.GetFileName(file)) == null)
                        {
                            File.Copy(file, System.IO.Path.Combine(MainWindow.ModDir, System.IO.Path.GetFileName(file)), true);
                            var mod = MainWindow.LoadModFromFile(System.IO.Path.Combine(MainWindow.ModDir, System.IO.Path.GetFileName(file)));
                            mod.Enabled = true;
                            MainWindow.mods.Add(mod);
                        }
                        current++;
                    }
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

        private void updateList()
        {
            mainPannel.Children.Clear();
            var input = searchBar.Text.ToLower();
            var newList = new List<Mod>();

            foreach (var mod in MainWindow.mods)
            {
                if (mod.SearchString1.Length > 0)
                {
                    int validated = 0;
                    int index = 0;
                    for (int i = 0; i < input.Length; i++)
                    {
                        for (int j = index; j < mod.SearchString1.Length; j++)
                        {
                            if (mod.SearchString1[j] == input[i])
                            {
                                validated++;
                                index = j + 1;
                                break;
                            }
                        }
                    }
                    if (validated == input.Length)
                    {
                        newList.Add(mod);
                    }
                }
            }

            foreach (var mod in newList)
            {
                mainPannel.Children.Add(new ListMod(mod));
            }
        }

        #endregion Private Methods
    }
}