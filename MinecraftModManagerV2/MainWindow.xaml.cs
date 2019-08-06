using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static string BufferDir = Path.Combine(BaseResDir, "where the magic goes");
        public static BitmapImage DefaultActiveModIcon;
        public static BitmapImage DefaultInactiveModIcon;
        public static BitmapImage hoverCross;
        public static BitmapImage hoverMinimize;
        public static BitmapImage idleCross;
        public static BitmapImage idleMinimize;
        public static string LogFile = Path.Combine(BaseResDir, "log.txt");
        public static string MCPath;
        public static List<Mod> mods;
        public static string PrefDir = Path.Combine(BaseResDir, "preferencies.json");
        public static Preferencies Preferencies;
        public static string ProfilFile = Path.Combine(BaseResDir, "profils.json");
        public static List<Profil> profils;

        #endregion Public Fields

        #region Public Constructors

        public MainWindow()
        {
            var standardOutput = new StreamWriter(LogFile, true) { AutoFlush = true };
            Console.SetOut(standardOutput);
            Console.SetError(standardOutput);
            Console.WriteLine(DateTime.Now + "-----------------------------------");
            if (!Directory.Exists(BaseResDir))
                Directory.CreateDirectory(BaseResDir);
            LoadPreferencies();
            mods = new List<Mod>();
            if (!Directory.Exists(BufferDir))
                Directory.CreateDirectory(BufferDir);
            {
                var lines = Environment.GetCommandLineArgs();
                if (lines.Length > 1)
                {
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var newstr = "";
                        foreach (var c in lines[i])
                        {
                            if (c != ' ' && c != '\t')
                                newstr += c;
                        }
                        lines[i] = newstr;
                    }
                    var command = lines[1];
                    var arguments = lines.Where((s, i) => i >= 2).ToArray();
                    if (command.Length >= 5 && command.Substring(0, 5) == "mcmm:")
                    {
                        var data = command.Substring(5);
                        var splittedData = data.Split('?');
                        command = splittedData.First();
                        if (splittedData.Length > 1)
                            arguments = splittedData.Last().Split('&');
                        else
                            arguments = new string[0];
                    }
                    if (command == "help" || command == "h")
                    {
                        Console.WriteLine(
@"Usage : <exe> [loadprofil|loadmods|disable|enable|help]
Commands :
    loadprofil / lp : loadprofil <profil name>                  Loads the given profil
    loadmods / lm : loadmods <modid> <modid> <modid> ...        Loads the given mods. Giving no mods does the same thing as ""disable""
    disable / d : disable                                       Disable all mods
    enable / e : enable                                         Enable all mods
    help / h : help                                             Displays this section

URI scheme :
mcmm:<command>[?<argument1>&<argument2>&<argument3>...]
");
                        Environment.Exit(0);
                    }
                    else if (command == "loadprofil" || command == "lp")
                    {
                        if (arguments.Length > 0)
                        {
                            var profilString = arguments[0];
                            LoadProfils();
                            var profil = profils.Find((p) => p.name == profilString);
                            if (profil.name == null)
                            {
                                Console.WriteLine("The profil \"" + profilString + "\" does not exist.");
                                Environment.Exit(0);
                            }
                            _scan(false);
                            int added = 0, removed = 0;
                            foreach (var mod in mods)
                            {
                                if (profil.modids.Contains(mod.Infos.modid))
                                {
                                    if (!mod.Enabled)
                                    {
                                        added++;
                                        mod.Enabled = true;
                                        mod.ChangeState();
                                    }
                                }
                                else
                                {
                                    if (mod.Enabled)
                                    {
                                        removed++;
                                        mod.Enabled = false;
                                        mod.ChangeState();
                                    }
                                }
                            }
                            Console.WriteLine("Successfully applied the profil.\n\tMods enabled : " + added + "\n\tMods removed : " + removed);
                            Environment.Exit(0);
                        }
                        else
                        {
                            Console.WriteLine("No profil given. Type help for the help section.");
                            Environment.Exit(0);
                        }
                    }
                    else if (command == "loadmods" || command == "lm")
                    {
                        var modsString = new List<string>();
                        for (int i = 0; i < arguments.Length; i++)
                            modsString.Add(arguments[i]);
                        _scan(false);
                        int added = 0, removed = 0;
                        foreach (var mod in mods)
                        {
                            if (modsString.Contains(mod.Infos.modid))
                            {
                                if (!mod.Enabled)
                                {
                                    added++;
                                    mod.Enabled = true;
                                    mod.ChangeState();
                                }
                            }
                            else
                            {
                                if (mod.Enabled)
                                {
                                    removed++;
                                    mod.Enabled = false;
                                    mod.ChangeState();
                                }
                            }
                        }
                        Console.WriteLine("Successfully enabled the mods.\n\tMods enabled : " + added + "\n\tMods removed : " + removed);
                        Environment.Exit(0);
                    }
                    else if (command == "enable" || command == "e")
                    {
                        _scan(false);
                        int added = 0;
                        foreach (var mod in mods)
                        {
                            if (!mod.Enabled)
                            {
                                added++;
                                mod.Enabled = true;
                                mod.ChangeState();
                            }
                        }
                        Console.WriteLine("Successfully enabled all the mods.\n\tMods enabled : " + added);
                        Environment.Exit(0);
                    }
                    else if (command == "disabled" || command == "d")
                    {
                        _scan(false);
                        int removed = 0;
                        foreach (var mod in mods)
                        {
                            if (mod.Enabled)
                            {
                                removed++;
                                mod.Enabled = false;
                                mod.ChangeState();
                            }
                        }
                        Console.WriteLine("Successfully disabled all the mods.\n\tMods disabled : " + removed);
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("Unknown command \"" + command + "\" type help for the help section.");
                        Environment.Exit(0);
                    }
                }
            }
            App = this;
            idleCross = ToBitmapImage(Properties.Resources.idleCross);
            hoverCross = ToBitmapImage(Properties.Resources.hoverCross);
            idleMinimize = ToBitmapImage(Properties.Resources.idleMinimize);
            hoverMinimize = ToBitmapImage(Properties.Resources.hoverMinimize);
            DefaultActiveModIcon = ToBitmapImage(Properties.Resources.javaIcon);
            DefaultInactiveModIcon = ToBitmapImage(CreateBWBitmap(Properties.Resources.javaIcon));
            InitializeComponent();
            ScanMods();
            LoadProfils();
        }

        #endregion Public Constructors

        #region Public Properties

        public static string BaseResDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mcmm");
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

        public static Mod LoadMod(Stream fileStream, bool handleGraphics = true)
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
                                        Newtonsoft.Json.JsonConvert.SerializeObject(res, Newtonsoft.Json.Formatting.Indented));
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
                        if (modid.Length > 0 && mod.Dependencies.FirstOrDefault((d) => d.modid == modid) == default && modid != "forge" && modid.All((c) => c >= 'a' && c <= 'z'))
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

        public static Mod LoadModFromFile(string file, bool handleGraphics = true)
        {
            var jsonFile = Directory.GetFiles(BufferDir).FirstOrDefault((f) => Path.GetFileName(f) == Path.GetFileNameWithoutExtension(file) + ".json");
            if (jsonFile != null)
            {
                try
                {
                    var modInfos = Newtonsoft.Json.JsonConvert.DeserializeObject<JSONModBuffer>(new StreamReader(jsonFile).ReadToEnd());
                    var mod = Mod.CreateFromJSON(modInfos, handleGraphics);
                    mod.Filename = Path.GetFileName(file);
                    return mod;
                }
                catch (Exception) { }
            }
            {
                var mod = LoadMod(new FileStream(file, FileMode.Open, FileAccess.Read));
                mod.Filename = Path.GetFileName(file);
                var buff = new JSONModBuffer();
                buff.infos = mod.Infos;
                buff.dependencies = mod.Dependencies.ToArray();
                if (mod.ActiveIcon != DefaultActiveModIcon)
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(mod.ActiveIcon));
                    using (var fileStream = new FileStream(Path.Combine(BufferDir, Path.GetFileNameWithoutExtension(mod.Filename) + ".png"), FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                    buff.activeIcon = Path.GetFileNameWithoutExtension(mod.Filename) + ".png";
                    encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(mod.InactiveIcon));

                    using (var fileStream = new FileStream(Path.Combine(BufferDir, Path.GetFileNameWithoutExtension(mod.Filename) + "_inactiveIcon.png"), FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                    buff.inactiveIcon = Path.GetFileNameWithoutExtension(mod.Filename) + "_inactiveIcon.png";

                    encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(mod.Background));

                    using (var fileStream = new FileStream(Path.Combine(BufferDir, Path.GetFileNameWithoutExtension(mod.Filename) + "_backgroundImage.png"), FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                    buff.backgroundImage = Path.GetFileNameWithoutExtension(mod.Filename) + "_backgroundImage.png";
                }
                else
                {
                    buff.activeIcon = "";
                    buff.inactiveIcon = "";
                    buff.backgroundImage = "";
                }
                using (var stream = new StreamWriter(Path.Combine(BufferDir, Path.GetFileNameWithoutExtension(mod.Filename) + ".json")))
                {
                    stream.Write(Newtonsoft.Json.JsonConvert.SerializeObject(buff, Newtonsoft.Json.Formatting.Indented));
                }
                return mod;
            }
        }

        public static void LoadProfils()
        {
            profils = new List<Profil>();
            if (File.Exists(ProfilFile))
            {
                Profil[] pr;
                using (var stream = new StreamReader(ProfilFile))
                {
                    pr = Newtonsoft.Json.JsonConvert.DeserializeObject<Profil[]>(stream.ReadToEnd());
                }
                profils.AddRange(pr);
            }
            else
            {
                using (var stream = new StreamWriter(ProfilFile))
                {
                    stream.Write(Newtonsoft.Json.JsonConvert.SerializeObject(new Profil[0], Newtonsoft.Json.Formatting.Indented));
                }
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

        public void LoadPreferencies()
        {
            if (File.Exists(PrefDir))
            {
                using (var sr = new StreamReader(PrefDir))
                {
                    Preferencies = Newtonsoft.Json.JsonConvert.DeserializeObject<Preferencies>(sr.ReadToEnd());
                }
            }
            else
            {
                Preferencies = new Preferencies();
                Preferencies.customDir = false;
                using (var sw = new StreamWriter(PrefDir))
                {
                    sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Preferencies, Newtonsoft.Json.Formatting.Indented));
                }
            }
            if (Preferencies.customDir)
                MCPath = Preferencies.dirString;
            else
                MCPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
            if (!Directory.Exists(MCPath))
            {
                MCPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
                if (!Directory.Exists(MCPath))
                {
                    var dialog = new BaseModel();
                    var warning = new WarningControl("Le chemin vers le dossier Minecraft est incorrect, veuillez spécifier le chemin", dialog);
                    dialog.Child = warning;
                    if (!dialog.ShowDialog().Value)
                        Environment.Exit(0);
                    var dirDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                    dirDialog.Description = "Selectionnez le répertoire Minecraft";
                    if (dirDialog.ShowDialog().Value)
                    {
                        Preferencies.customDir = true;
                        Preferencies.dirString = dirDialog.SelectedPath;
                        using (var sw = new StreamWriter(PrefDir))
                        {
                            sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Preferencies, Newtonsoft.Json.Formatting.Indented));
                        }
                    }
                    else
                        Environment.Exit(0);
                }
                else
                {
                    Preferencies.customDir = false;
                    using (var sw = new StreamWriter(PrefDir))
                    {
                        sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Preferencies, Newtonsoft.Json.Formatting.Indented));
                    }
                }
            }
            if (!Directory.Exists(ModDir))
                Directory.CreateDirectory(ModDir);
            if (!Directory.Exists(DisabledModDir))
                Directory.CreateDirectory(DisabledModDir);
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
            Bitmap BWbase = new Bitmap(bitmap.Width, bitmap.Height);
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

        private void _scan(bool handleGraphics)
        {
            Console.WriteLine("Scanning mods...");
            int maxItems = Directory.GetFiles(ModDir).Where((file) => Path.GetExtension(file) == ".jar" || Path.GetExtension(file) == ".zip").Count() + Directory.GetFiles(DisabledModDir).Where((file) => Path.GetExtension(file) == ".jar" || Path.GetExtension(file) == ".zip").Count();

            int currentItem = 0;
            void scanFiles(string dir, bool enabled)
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    if (handleGraphics)
                    {
                        Dispatcher.Invoke(() => ((LoadingPage)Placeholder.Children[0]).ChangeFill((float)currentItem / maxItems));
                        Dispatcher.Invoke(() => ((LoadingPage)Placeholder.Children[0]).ChangeDisplayer("Chargement de : " + Path.GetFileNameWithoutExtension(file) + "..."));
                    }
                    if (Path.GetExtension(file) == ".jar" || Path.GetExtension(file) == ".zip")
                    {
                        var mod = LoadModFromFile(file, handleGraphics);
                        mod.Enabled = enabled;
                        mod.Filename = Path.GetFileName(file);
                        mods.Add(mod);
                        currentItem++;
                    }
                }
            }
            scanFiles(ModDir, true);
            scanFiles(DisabledModDir, false);
            Console.WriteLine(mods.Count + " mod(s) found");
            mods.Sort((left, right) => left.Infos.name.CompareTo(right.Infos.name));
            if (handleGraphics)
                Dispatcher.Invoke(() =>
                {
                    Width = 1100;
                    Height = 700;
                    Child = new Home();
                });
        }

        private void _scan() => _scan(true);

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

        #endregion Private Methods
    }
}