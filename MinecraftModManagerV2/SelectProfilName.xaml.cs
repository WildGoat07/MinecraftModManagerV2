using System;
using System.Collections.Generic;
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
    /// Logique d'interaction pour SelectProfilName.xaml
    /// </summary>
    public partial class SelectProfilName : UserControl
    {
        #region Private Fields

        private Window mainWindow;

        #endregion Private Fields

        #region Public Constructors

        public SelectProfilName(Window owner, string profilName = "")
        {
            mainWindow = owner;
            ProfilName = profilName;
            InitializeComponent();
            inputName.Text = profilName;
            foreach (var profil in MainWindow.profils)
            {
                var button = new Button();
                button.FontFamily = new FontFamily("Segoe UI Light");
                button.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                button.FontSize = 14;
                button.Margin = new Thickness(5);
                button.Content = profil.name;
                button.Click += (sender, e) => inputName.Text = profil.name;
                pannel.Children.Add(button);
            }
        }

        #endregion Public Constructors

        #region Public Properties

        public string ProfilName { get; set; }

        #endregion Public Properties

        #region Private Methods

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (inputName.Text.Length > 0)
            {
                mainWindow.DialogResult = true;
                ProfilName = inputName.Text;
                mainWindow.Close();
            }
            else
                inputName.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 50, 100));
        }

        #endregion Private Methods
    }
}