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
    /// Logique d'interaction pour WarningControl.xaml
    /// </summary>
    public partial class WarningControl : UserControl
    {
        #region Private Fields

        private Window app;

        #endregion Private Fields

        #region Public Constructors

        public WarningControl(string text, Window parent)
        {
            InitializeComponent();
            app = parent;
            parent.Height = 0;
            parent.Width = 300;
            textDisplay.Text = text;
        }

        #endregion Public Constructors

        #region Private Methods

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            app.DialogResult = true;
            app.Close();
        }

        #endregion Private Methods
    }
}