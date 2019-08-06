using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace MinecraftModManagerV2
{
    /// <summary>
    /// Logique d'interaction pour Options.xaml
    /// </summary>
    public partial class Options : UserControl
    {
        #region Private Fields

        private Window app;
        private Preferencies oldPref;

        #endregion Private Fields

        #region Public Constructors

        public Options(Window parent)
        {
            app = parent;
            oldPref = MainWindow.Preferencies;
            InitializeComponent();
            assemblyVersion.Text = "Version du logiciel : " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            foreach (var profil in MainWindow.profils)
                profilsList.Items.Add(profil);
            if (oldPref.customDir)
            {
                customMCDir.IsChecked = true;
                mCDir.Text = oldPref.dirString;
            }
            else
                defaultMCDir.IsChecked = true;
            parent.Closing += (sender, e) =>
            {
                if (oldPref != MainWindow.Preferencies)
                {
                    var dialog = new BaseModel("Redémarrage requis");
                    dialog.Owner = parent;
                    var warning = new WarningControl("Le logiciel doit redémarrer pour appliquer les changements.", dialog);
                    dialog.Child = warning;
                    dialog.ShowDialog();
                }
            };
            updateBuffCleanerDisplay();
        }

        #endregion Public Constructors

        #region Public Methods

        public void updateBuffCleanerDisplay()
        {
            long size = 0;
            foreach (var file in Directory.GetFiles(MainWindow.BufferDir))
                size += new FileInfo(file).Length;
            string suffix = "o";
            if (size > 1024)
            {
                size /= 1024;
                suffix = "Ko";
            }
            if (size > 1024)
            {
                size /= 1024;
                suffix = "Mo";
            }
            if (size > 1024)
            {
                size /= 1024;
                suffix = "Go";
            }
            buffCleaner.Content = "Vider le cache (" + size + suffix + ")";
        }

        #endregion Public Methods

        #region Private Methods

        private void BuffCleaner_Click(object sender, RoutedEventArgs e)
        {
            foreach (var file in Directory.GetFiles(MainWindow.BufferDir))
                File.Delete(file);
            updateBuffCleanerDisplay();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dirDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dirDialog.UseDescriptionForTitle = true;
            dirDialog.Description = "Selectionnez le répertoire Minecraft";
            if (dirDialog.ShowDialog(app).Value)
            {
                MainWindow.Preferencies.customDir = true;
                MainWindow.Preferencies.dirString = dirDialog.SelectedPath;
                mCDir.Text = dirDialog.SelectedPath;
                customMCDir.IsChecked = true;
            }
        }

        private void CustomMCDir_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.Preferencies.customDir = true;
            using (var sw = new StreamWriter(MainWindow.PrefDir))
            {
                sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(MainWindow.Preferencies, Newtonsoft.Json.Formatting.Indented));
            }
        }

        private void DefaultMCDir_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.Preferencies.customDir = false;
            using (var sw = new StreamWriter(MainWindow.PrefDir))
            {
                sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(MainWindow.Preferencies, Newtonsoft.Json.Formatting.Indented));
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.profils.Remove((Profil)profilsList.SelectedItem);
            profilsList.Items.Remove(profilsList.SelectedItem);
            profilsList.SelectedIndex = -1;
            deleteButton.IsEnabled = false;
            using (var sw = new StreamWriter(MainWindow.ProfilFile))
            {
                sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(MainWindow.profils.ToArray(), Newtonsoft.Json.Formatting.Indented));
            }
            Home.GlobalHome.UpdateProfilsList();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }

        private void ProfilsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (profilsList.SelectedIndex > -1)
                deleteButton.IsEnabled = true;
        }

        #endregion Private Methods
    }
}