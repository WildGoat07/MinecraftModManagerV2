using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Logique d'interaction pour ModDetail.xaml
    /// </summary>
    public partial class ModDetail : UserControl
    {
        #region Private Fields

        private Mod currentMod;
        private UIElement OldPage;

        #endregion Private Fields

        #region Public Constructors

        public ModDetail(Mod mod, UIElement oldPage)
        {
            currentMod = mod;
            OldPage = oldPage;
            InitializeComponent();
            if (mod.Enabled)
            {
                toggle.Content = "Activé";
                toggle.Foreground = new SolidColorBrush(Color.FromRgb(50, 255, 100));
            }
            else
            {
                toggle.Content = "Désactivé";
                toggle.Foreground = new SolidColorBrush(Color.FromRgb(255, 50, 100));
            }
            modTitle.Text = mod.Infos.name;
            modBackground.Source = mod.Background;
            modIcon.Source = mod.ActiveIcon;
            modIcon.Height = mod.ActiveIcon.PixelHeight > 200 ? 200 : mod.ActiveIcon.PixelHeight;
            if (modIcon.Margin.Top + modIcon.Height > 50)
                viewer.Margin = new Thickness(0, modIcon.Margin.Top + modIcon.Height, 0, 0);
            else
                viewer.Margin = new Thickness(0, 50, 0, 0);
            modid.Text = "Identifiant : " + mod.Infos.modid;
            description.Text = "Description : " + mod.Infos.description;
            version.Text = "Version du mod : " + mod.Infos.version;
            mcversion.Text = "Version de Minecraft : " + mod.Infos.mcversion;
            authors.Text = "auteurs :\n\t";
            if (mod.Infos.authorList != null)
                for (int i = 0; i < mod.Infos.authorList.Length; i++)
                {
                    authors.Text += mod.Infos.authorList[i];
                    if (i < mod.Infos.authorList.Length - 1)
                        authors.Text += ", ";
                }
            try
            {
                var urlString = mod.Infos.url;
                if (urlString[0] != 'h' && urlString[0] != 'w')
                    urlString = "http://" + urlString;
                url.NavigateUri = new Uri(urlString);
            }
            catch (Exception)
            {
                url.NavigateUri = null;
            }
            urlDisplay.Text = mod.Infos.url;
            if (mod.Dependencies.Count > 0)
            {
                foreach (var dep in mod.Dependencies)
                {
                    var linkText = new TextBlock();
                    linkText.FontFamily = new FontFamily("Segoe UI Light");
                    linkText.FontSize = 14;
                    var specifiedMod = MainWindow.mods.FirstOrDefault((m) => m.Infos.modid == dep.modid);
                    if (specifiedMod != null)
                    {
                        linkText.Text = specifiedMod.Infos.name;
                        if (specifiedMod.Enabled)
                            linkText.Foreground = new SolidColorBrush(Color.FromRgb(50, 255, 100));
                        else if (dep.required)
                            linkText.Foreground = new SolidColorBrush(Color.FromRgb(255, 50, 100));
                        else
                            linkText.Foreground = new SolidColorBrush(Color.FromRgb(255, 150, 50));
                        if (dep.required)
                            linkText.Text += " (requis)";
                        else
                            linkText.Text += " (optionel)";
                        var hyperlink = new Hyperlink();
                        hyperlink.Inlines.Add(new InlineUIContainer(linkText));
                        hyperlink.Click += (sender, e) =>
                        {
                            MainWindow.App.Child = new ModDetail(specifiedMod, this);
                        };
                        dependencies.Children.Add(new TextBlock(hyperlink));
                    }
                    else
                    {
                        linkText.Text = dep.modid;
                        if (dep.required)
                        {
                            linkText.Foreground = new SolidColorBrush(Color.FromRgb(255, 50, 100));
                            linkText.Text += " (requis)";
                        }
                        else
                        {
                            linkText.Foreground = new SolidColorBrush(Color.FromRgb(255, 150, 50));
                            linkText.Text += " (optionel)";
                        }
                        linkText.ToolTip = "Le mod manquant n'a pas été trouvé dans les mods disponibles";
                        dependencies.Children.Add(linkText);
                    }
                }
            }
            credits.Text = "Crédits : " + mod.Infos.credits;
        }

        #endregion Public Constructors

        #region Private Methods

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (OldPage is ModDetail detail)
            {
                detail.dependencies.Children.Clear();
                if (detail.currentMod.Dependencies.Count > 0)
                {
                    foreach (var dep in detail.currentMod.Dependencies)
                    {
                        var linkText = new TextBlock();
                        linkText.FontFamily = new FontFamily("Segoe UI Light");
                        linkText.FontSize = 14;
                        var specifiedMod = MainWindow.mods.FirstOrDefault((m) => m.Infos.modid == dep.modid);
                        if (specifiedMod != null)
                        {
                            linkText.Text = specifiedMod.Infos.name;
                            if (specifiedMod.Enabled)
                                linkText.Foreground = new SolidColorBrush(Color.FromRgb(50, 255, 100));
                            else if (dep.required)
                                linkText.Foreground = new SolidColorBrush(Color.FromRgb(255, 50, 100));
                            else
                                linkText.Foreground = new SolidColorBrush(Color.FromRgb(255, 150, 50));
                            if (dep.required)
                                linkText.Text += " (requis)";
                            else
                                linkText.Text += " (optionel)";
                            var hyperlink = new Hyperlink();
                            hyperlink.Inlines.Add(new InlineUIContainer(linkText));
                            hyperlink.Click += (a, b) =>
                            {
                                MainWindow.App.Child = new ModDetail(specifiedMod, detail);
                            };
                            detail.dependencies.Children.Add(new TextBlock(hyperlink));
                        }
                        else
                        {
                            linkText.Text = dep.modid;
                            if (dep.required)
                            {
                                linkText.Foreground = new SolidColorBrush(Color.FromRgb(255, 50, 100));
                                linkText.Text += " (requis)";
                            }
                            else
                            {
                                linkText.Foreground = new SolidColorBrush(Color.FromRgb(255, 150, 50));
                                linkText.Text += " (optionel)";
                            }
                            linkText.ToolTip = "Le mod manquant n'a pas été trouvé dans les mods disponibles";
                            detail.dependencies.Children.Add(linkText);
                        }
                    }
                }
            }
            MainWindow.App.Child = OldPage;
            Home.GlobalHome.updateList();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.App.DragMove();
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            currentMod.Enabled = !currentMod.Enabled;
            currentMod.ChangeState();
            Home.GlobalHome.ListMods.First((lm) => lm.linkedMod == currentMod).UpdateStatus();
            if (currentMod.Enabled)
            {
                toggle.Content = "Activé";
                toggle.Foreground = new SolidColorBrush(Color.FromRgb(50, 255, 100));
            }
            else
            {
                toggle.Content = "Désactivé";
                toggle.Foreground = new SolidColorBrush(Color.FromRgb(255, 50, 100));
            }
        }

        private void Url_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            if (hyperlink.NavigateUri != null)
                Process.Start(hyperlink.NavigateUri.ToString());
        }

        #endregion Private Methods
    }
}