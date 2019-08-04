using System;
using System.Collections.Generic;
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
using Path = System.IO.Path;

namespace MinecraftModManagerV2
{
    /// <summary>
    /// Logique d'interaction pour ListMod.xaml
    /// </summary>
    public partial class ListMod : UserControl
    {
        #region Private Fields

        private Mod linkedMod;

        #endregion Private Fields

        #region Public Constructors

        public ListMod(Mod mod)
        {
            InitializeComponent();
            linkedMod = mod;
            modTitle.Text = mod.Infos.name;
            modMCVersion.Text = mod.Infos.mcversion;
            modAuthors.Text = "";
            if (mod.Infos.authorList != null)
                for (int i = 0; i < mod.Infos.authorList.Length; i++)
                {
                    var author = mod.Infos.authorList[i];
                    modAuthors.Text += author;
                    if (i < mod.Infos.authorList.Length - 1)
                        modAuthors.Text += ", ";
                }
            if (mod.Infos.description != null)
                ToolTip = mod.Infos.description;
            UpdateStatus();
        }

        #endregion Public Constructors

        #region Public Methods

        public void UpdateStatus()
        {
            if (linkedMod.Enabled)
            {
                status.Content = "Activé";
                status.Foreground = new SolidColorBrush(Color.FromRgb(50, 255, 100));
                modIcon.Source = linkedMod.ActiveIcon;
            }
            else
            {
                status.Content = "Désactivé";
                status.Foreground = new SolidColorBrush(Color.FromRgb(255, 50, 100));
                modIcon.Source = linkedMod.InactiveIcon;
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void ModClicked(object sender, RoutedEventArgs e)
        {
        }

        private void MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            controlBorder.Background = new SolidColorBrush(Color.FromRgb(60, 90, 120));
        }

        private void MouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            controlBorder.Background = new SolidColorBrush(Color.FromRgb(33, 50, 68));
        }

        private void MouseEntered(object sender, MouseEventArgs e)
        {
            controlBorder.Background = new SolidColorBrush(Color.FromRgb(50, 60, 75));
        }

        private void MouseLeaved(object sender, MouseEventArgs e)
        {
            controlBorder.Background = new SolidColorBrush(Color.FromRgb(33, 50, 68));
        }

        private void Status_Checked(object sender, RoutedEventArgs e)
        {
            linkedMod.Enabled = !linkedMod.Enabled;
            linkedMod.ChangeState();
            UpdateStatus();
        }

        #endregion Private Methods
    }
}