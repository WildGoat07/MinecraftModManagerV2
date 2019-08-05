using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using Path = System.IO.Path;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace MinecraftModManagerV2
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Public Fields

        public static MainWindow App;
        public static string CacheDir = "where the magic goes";
        public static BitmapImage DefaultActiveModIcon;
        public static BitmapImage DefaultInactiveModIcon;
        public static BitmapImage hoverCross;
        public static BitmapImage hoverMinimize;
        public static BitmapImage idleCross;
        public static BitmapImage idleMinimize;
        public static string MCPath = "C:/Users/Nathan/AppData/Roaming/.minecraft";
        public static List<Mod> mods;
        public static string SelectedMCVersion = "1.12.2";

        #endregion Public Fields

        #region Public Constructors

        public MainWindow()
        {
            App = this;
            if (!Directory.Exists(CacheDir))
                Directory.CreateDirectory(CacheDir);
            idleCross = ToBitmapImage(Properties.Resources.idleCross);
            hoverCross = ToBitmapImage(Properties.Resources.hoverCross);
            idleMinimize = ToBitmapImage(Properties.Resources.idleMinimize);
            hoverMinimize = ToBitmapImage(Properties.Resources.hoverMinimize);
            mods = new List<Mod>();
            DefaultActiveModIcon = ToBitmapImage(Properties.Resources.javaIcon);
            DefaultInactiveModIcon = ToBitmapImage(CreateBWBitmap(Properties.Resources.javaIcon));
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Public Properties

        public static string DisabledModDir => Path.Combine(MCPath, "disabled mods");
        public static string ModDir => Path.Combine(MCPath, "mods");

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

        public static Bitmap GenerateBackground(Bitmap original)
        {
            var result = new Bitmap(1, original.Height);
            for (int y = 0; y < original.Height; y++)
            {
                double r = 0, g = 0, b = 0;
                double coeff = 0;
                for (int x = 0; x < original.Width; x++)
                {
                    var c = original.GetPixel(x, y);
                    double perc = c.A / 255.0;
                    r += c.R * perc;
                    g += c.G * perc;
                    b += c.B * perc;
                    coeff += perc;
                }
                result.SetPixel(0, y, System.Drawing.Color.FromArgb(255, (byte)(r / coeff), (byte)(g / coeff), (byte)(b / coeff)));
            }
            return result;
        }

        public static Mod LoadMod(Stream fileStream)
        {
            void triggerError(string f)
            {
                //MessageBox.Show("Fichier d'information manquant pour le mod \"" + Path.GetFileName(f) + "\".", "erreur de chargment", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            var mod = new Mod();
            mod.Dependencies = new List<Dependency>();
            var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
            var entry = archive.GetEntry("mcmod.info");
            IEnumerable<ZipArchiveEntry> availableInfo;
            if (entry != null)
                availableInfo = new ZipArchiveEntry[] { entry };
            else
                availableInfo = archive.Entries.Where((e) => Path.GetExtension(e.FullName) == ".info");
            bool good = false;
            foreach (var entryFound in availableInfo)
            {
                var sr = new StreamReader(entryFound.Open());
                {
                    string json = sr.ReadToEnd();
                    try
                    {
                        mod.Infos = Newtonsoft.Json.JsonConvert.DeserializeObject<ModInfo[]>(json).First();
                        good = true;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            if (Newtonsoft.Json.JsonConvert.DeserializeObject(json) is JContainer container)
                            {
                                var res = RecursiveSearch(container, "modid");
                                if (res != null)
                                {
                                    mod.Infos = Newtonsoft.Json.JsonConvert.DeserializeObject<ModInfo>(
                                        Newtonsoft.Json.JsonConvert.SerializeObject(res));
                                    good = true;
                                    break;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }
            var availableChars = new List<char>() { '-', ':', '@', '[', ']', '(', ')', '.', ',', ';' };
            for (char i = 'a'; i < 'z'; i++)
                availableChars.Add(i);
            for (char i = '0'; i < '9'; i++)
                availableChars.Add(i);
            foreach (var entryFile in archive.Entries)
            {
                if (Path.GetExtension(entryFile.Name.ToLower()) == ".class")
                {
                    string content;
                    using (var sr = new StreamReader(entryFile.Open(), Encoding.UTF8))
                        content = sr.ReadToEnd();
                    int index = 0;
                    while (index + 15 < content.Length && content.Substring(index, 13) != "dependencies")
                        index++;
                    if (index + 15 == content.Length)
                        continue;
                    index += 15;
                    int len = 0;
                    while (index + len < content.Length && availableChars.Contains(content[index + len]))
                        len++;
                    var depStrings = content.Substring(index, len).Split(';');
                    foreach (var depString in depStrings)
                    {
                        var tmp = depString.Split(':');
                        var prefix = tmp.First();
                        var tmp2 = tmp.Last();
                        tmp = tmp2.Split('@');
                        var modid = tmp.First();
                        if (modid.Length > 0 && mod.Dependencies.FirstOrDefault((d) => d.modid == modid) == default && modid != "forge")
                            mod.Dependencies.Add(new Dependency() { modid = modid, required = prefix.Contains("required") });
                    }
                }
            }
            mod.Dependencies.Sort((left, right) =>
            {
                if (left.required != right.required)
                    return right.required.CompareTo(left.required);
                else
                    return left.modid.CompareTo(right.modid);
            });
            if (!good && fileStream is FileStream file)
            {
                triggerError(file.Name);
                mod.Infos = new ModInfo() { name = Path.GetFileNameWithoutExtension(file.Name), modid = "//" + Path.GetFileNameWithoutExtension(file.Name) };
            }
            try
            {
                Bitmap logo = new Bitmap(archive.GetEntry(mod.Infos.logoFile).Open());
                mod.ActiveIcon = ToBitmapImage(logo);
                mod.InactiveIcon = ToBitmapImage(CreateBWBitmap(logo));
                mod.Background = ToBitmapImage(GenerateBackground(logo));
            }
            catch (Exception)
            {
                mod.ActiveIcon = DefaultActiveModIcon;
                mod.InactiveIcon = DefaultInactiveModIcon;
                mod.Background = null;
            }
            return mod;
        }

        public static Mod LoadModFromFile(string file)
        {
            var jsonFile = Directory.GetFiles(CacheDir).FirstOrDefault((f) => Path.GetFileName(f) == Path.GetFileNameWithoutExtension(file) + ".json");
            if (jsonFile != null)
            {
                try
                {
                    var modInfos = Newtonsoft.Json.JsonConvert.DeserializeObject<JSONModCache>(new StreamReader(jsonFile).ReadToEnd());
                    var mod = Mod.CreateFromJSON(modInfos);
                    mod.Filename = Path.GetFileName(file);
                    return mod;
                }
                catch (Exception) { }
            }
            {
                var mod = LoadMod(new FileStream(file, FileMode.Open, FileAccess.Read));
                mod.Filename = Path.GetFileName(file);
                var cache = new JSONModCache();
                cache.infos = mod.Infos;
                cache.dependencies = mod.Dependencies.ToArray();
                if (mod.ActiveIcon != DefaultActiveModIcon)
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(mod.ActiveIcon));
                    using (var fileStream = new FileStream(Path.Combine(CacheDir, Path.GetFileNameWithoutExtension(mod.Filename) + ".png"), FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                    cache.activeIcon = Path.GetFileNameWithoutExtension(mod.Filename) + ".png";
                    encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(mod.InactiveIcon));

                    using (var fileStream = new FileStream(Path.Combine(CacheDir, Path.GetFileNameWithoutExtension(mod.Filename) + "_inactiveIcon.png"), FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                    cache.inactiveIcon = Path.GetFileNameWithoutExtension(mod.Filename) + "_inactiveIcon.png";

                    encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(mod.Background));

                    using (var fileStream = new FileStream(Path.Combine(CacheDir, Path.GetFileNameWithoutExtension(mod.Filename) + "_backgroundImage.png"), FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                    cache.backgroundImage = Path.GetFileNameWithoutExtension(mod.Filename) + "_backgroundImage.png";
                }
                else
                {
                    cache.activeIcon = "";
                    cache.inactiveIcon = "";
                    cache.backgroundImage = "";
                }
                using (var stream = new StreamWriter(Path.Combine(CacheDir, Path.GetFileNameWithoutExtension(mod.Filename) + ".json")))
                {
                    stream.Write(Newtonsoft.Json.JsonConvert.SerializeObject(cache));
                }
                return mod;
            }
        }

        public static JObject RecursiveSearch(JContainer container, string keyToFind)
        {
            foreach (var item in container)
            {
                if (item is JObject obj)
                {
                    if (obj.ContainsKey(keyToFind))
                        return obj;
                }
                if (item is JContainer cont)
                {
                    var res = RecursiveSearch(cont, keyToFind);
                    if (res != null)
                        return res;
                }
            }
            return null;
        }

        public static Bitmap ToBitmap(BitmapSource source)
        {
            //https://stackoverflow.com/a/2897325
            Bitmap bmp = new Bitmap(
                  source.PixelWidth,
                  source.PixelHeight,
                  PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              PixelFormat.Format32bppPArgb);
            source.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            //https://stackoverflow.com/a/23831231
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        [STAThread]
        public void ScanMods()
        {
            Placeholder.Children.Clear();
            Placeholder.Children.Add(new LoadingPage());
            var th = new Thread(_scan);
            th.Start();
        }

        #endregion Public Methods

        #region Private Methods

        private static Bitmap CreateBWBitmap(Bitmap bitmap)
        {
            System.Drawing.Bitmap BWbase = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < BWbase.Width; x++)
            {
                for (int y = 0; y < BWbase.Height; y++)
                {
                    var origPixel = bitmap.GetPixel(x, y);
                    byte value = (byte)(((float)origPixel.R + origPixel.G + origPixel.B) / 3);
                    BWbase.SetPixel(x, y, System.Drawing.Color.FromArgb(origPixel.A, value, value, value));
                }
            }
            return BWbase;
        }

        private void _scan()
        {
            int maxItems = Directory.GetFiles(ModDir).Where((file) => Path.GetExtension(file) == ".jar" || Path.GetExtension(file) == ".zip").Count() + Directory.GetFiles(DisabledModDir).Where((file) => Path.GetExtension(file) == ".jar" || Path.GetExtension(file) == ".zip").Count();

            int currentItem = 0;
            void scanFiles(string dir, bool enabled)
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    Dispatcher.Invoke(() => ((LoadingPage)Placeholder.Children[0]).ChangeFill((float)currentItem / maxItems));
                    Dispatcher.Invoke(() => ((LoadingPage)Placeholder.Children[0]).ChangeDisplayer("Chargement de : " + Path.GetFileNameWithoutExtension(file) + "..."));
                    if (Path.GetExtension(file) == ".jar" || Path.GetExtension(file) == ".zip")
                    {
                        var mod = LoadModFromFile(file);
                        mod.Enabled = enabled;
                        mod.Filename = Path.GetFileName(file);
                        mods.Add(mod);
                        currentItem++;
                    }
                }
            }
            scanFiles(ModDir, true);
            scanFiles(DisabledModDir, false);
            mods.Sort((left, right) => left.Infos.name.CompareTo(right.Infos.name));
            Dispatcher.Invoke(() =>
            {
                Width = 1100;
                Height = 700;
                Child = new Home();
            });
        }

        private void Cross_MouseEnter(object sender, MouseEventArgs e)
        {
            cross.Source = hoverCross;
        }

        private void Cross_MouseLeave(object sender, MouseEventArgs e)
        {
            cross.Source = idleCross;
        }

        private void Cross_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Minim_MouseEnter(object sender, MouseEventArgs e)
        {
            minim.Source = hoverMinimize;
        }

        private void Minim_MouseLeave(object sender, MouseEventArgs e)
        {
            minim.Source = idleMinimize;
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

        private void Window_Initialized(object sender, EventArgs e)
        {
            ScanMods();
        }

        #endregion Private Methods
    }
}