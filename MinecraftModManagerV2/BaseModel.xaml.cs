using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace MinecraftModManagerV2
{
    /// <summary>
    /// Logique d'interaction pour BaseModel.xaml
    /// </summary>
    public partial class BaseModel : Window
    {
        #region Private Fields

        private bool canClose;

        #endregion Private Fields

        #region Public Constructors

        public BaseModel(bool closeAvailable = true)
        {
            canClose = closeAvailable;
            InitializeComponent();
            if (canClose)
                cross.Source = MainWindow.idleCross;
            else
                cross.Source = null;
        }

        #endregion Public Constructors

        #region Public Properties

        public UIElement Child
        {
            get => Placeholder.Children.Count > 0 ? Placeholder.Children[0] : null;
            set
            {
                Placeholder.Children.Clear();
                Placeholder.Children.Add(value);
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void ForceClosing()
        {
            canClose = true;
            Close();
        }

        #endregion Public Methods

        #region Private Methods

        private void Cross_MouseEnter(object sender, MouseEventArgs e)
        {
            if (canClose)
                cross.Source = MainWindow.hoverCross;
        }

        private void Cross_MouseLeave(object sender, MouseEventArgs e)
        {
            if (canClose)
                cross.Source = MainWindow.idleCross;
        }

        private void Cross_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Minim_MouseEnter(object sender, MouseEventArgs e)
        {
            minim.Source = MainWindow.hoverMinimize;
        }

        private void Minim_MouseLeave(object sender, MouseEventArgs e)
        {
            minim.Source = MainWindow.idleMinimize;
        }

        private void Minim_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Normal)
                    WindowState = WindowState.Maximized;
                else if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
            }
            DragMove();
        }

        private void windowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = !canClose;
        }

        #endregion Private Methods
    }
}