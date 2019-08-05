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

        #region Private Fields

        private bool init = false;

        #endregion Private Fields

        #region Public Constructors

        public Home()
        {
            GlobalHome = this;
            InitializeComponent();
            UpdateProfilsList();
            ListMods = new List<ListMod>();
            foreach (var mod in MainWindow.mods)
            {
                ListMods.Add(new ListMod(mod));
            }
            ListMods.Sort((left, right) => left.linkedMod.Infos.name.CompareTo(right.linkedMod.Infos.name));
            updateList();
            init = true;
        }

        #endregion Public Constructors

        #region Public Properties

        public List<ListMod> ListMods { get; set; }

        #endregion Public Properties

        #region Public Methods

        public static void CreateProfilFromSelection()
        {
            var profil = new Profil();
            profil.modids = MainWindow.mods.Where((m) => m.Enabled).Select((m) => m.Infos.modid).ToArray();
            var window = new BaseModel();
            window.Title = "Profils";
            window.Height = 0;
            window.Width = 0;
            var selector = new SelectProfilName(window, "Nouveau profil");
            window.Child = selector;
            window.Owner = MainWindow.App;
            var res = window.ShowDialog();
            if (res.HasValue && res.Value)
            {
                profil.name = selector.ProfilName;
                MainWindow.profils.Remove(profil);
                MainWindow.profils.Add(profil);
                using (var sw = new StreamWriter(MainWindow.ProfilFile))
                {
                    sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(MainWindow.profils.ToArray()));
                }
                GlobalHome.UpdateProfilsList();
            }
        }

        public void OperateView(ListMod mod)
        {
            if (viewSelector.SelectedIndex == 1 && !mod.linkedMod.Enabled && mainPannel.Children.Contains(mod))
            {
                mainPannel.Children.Remove(mod);
            }
            else if (viewSelector.SelectedIndex == 2 && mod.linkedMod.Enabled && mainPannel.Children.Contains(mod))
            {
                mainPannel.Children.Remove(mod);
            }
        }

        public void updateList()
        {
            mainPannel.Children.Clear();
            var input = searchBar.Text.ToLower();

            foreach (var mod in ListMods)
            {
                if ((viewSelector.SelectedIndex == 1 && !mod.linkedMod.Enabled) ||
                    (viewSelector.SelectedIndex == 2 && mod.linkedMod.Enabled))
                    continue;
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

        public void UpdateProfilsList()
        {
            profilSelector.Items.Clear();
            profilSelector.Items.Add("Charger un profil");
            profilSelector.SelectedIndex = 0;
            foreach (var element in MainWindow.profils)
                profilSelector.Items.Add(element);
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
            foreach (var displayMod in ListMods)
            {
                displayMod.UpdateStatus();
            }
            updateList();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var mod in MainWindow.mods)
            {
                mod.Enabled = false;
                mod.ChangeState();
            }
            foreach (var displayMod in ListMods)
            {
                displayMod.UpdateStatus();
            }
            updateList();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog()
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

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            CreateProfilFromSelection();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var window = new BaseModel();
            window.Title = "Paramètres";
            window.Child = new Options(window);
            window.ShowDialog();
            UpdateProfilsList();
        }

        private void ProfilSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (profilSelector.SelectedIndex > 0)
            {
                var profil = (Profil)profilSelector.SelectedItem;
                foreach (var mod in MainWindow.mods)
                {
                    if (profil.modids.Contains(mod.Infos.modid) && !mod.Enabled)
                    {
                        mod.Enabled = true;
                        mod.ChangeState();
                    }
                    else if (mod.Enabled && !profil.modids.Contains(mod.Infos.modid))
                    {
                        mod.Enabled = false;
                        mod.ChangeState();
                    }
                }
                foreach (var lm in ListMods)
                {
                    lm.UpdateStatus();
                }
                profilSelector.SelectedIndex = 0;
                updateList();
            }
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateList();
        }

        private void ViewSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init)
                updateList();
        }

        #endregion Private Methods
    }
}