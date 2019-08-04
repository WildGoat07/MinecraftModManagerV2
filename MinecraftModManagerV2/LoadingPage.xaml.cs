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
    /// Logique d'interaction pour LoadingPage.xaml
    /// </summary>
    public partial class LoadingPage : UserControl
    {
        #region Public Constructors

        public LoadingPage()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Public Methods

        public void ChangeDisplayer(string text)
        {
            Displayer.Text = text;
        }

        public void ChangeFill(float perc)
        {
            progression.Value = perc * progression.Maximum;
        }

        #endregion Public Methods
    }
}