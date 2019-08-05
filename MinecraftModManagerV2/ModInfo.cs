using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MinecraftModManagerV2
{
    public struct Dependency : IEquatable<Dependency>
    {
        #region Public Fields

        public string modid;
        public bool required;

        #endregion Public Fields

        #region Public Methods

        public static bool operator !=(Dependency dependency1, Dependency dependency2)
        {
            return !(dependency1 == dependency2);
        }

        public static bool operator ==(Dependency dependency1, Dependency dependency2)
        {
            return dependency1.Equals(dependency2);
        }

        public override bool Equals(object obj)
        {
            return obj is Dependency && Equals((Dependency)obj);
        }

        public bool Equals(Dependency other)
        {
            return modid == other.modid;
        }

        #endregion Public Methods
    }

    public struct JSONModCache
    {
        #region Public Fields

        public string activeIcon;
        public string backgroundImage;
        public Dependency[] dependencies;
        public string inactiveIcon;
        public ModInfo infos;

        #endregion Public Fields
    }

    public struct ModInfo
    {
        #region Public Fields

        public string[] authorList;
        public string credits;
        public string[] dependants;
        public string[] dependencies;
        public string description;
        public string logoFile;
        public string mcversion;
        public string modid;
        public string name;
        public string parent;
        public string[] requiredMods;
        public string[] screenshots;
        public string updateJSON;

        [Obsolete]
        public string updateUrl;

        public string url;
        public bool useDependencyInformation;
        public string version;

        #endregion Public Fields
    }

    public class Mod : IEquatable<Mod>
    {
        #region Public Properties

        public BitmapImage ActiveIcon { get; set; }

        public BitmapImage Background { get; set; }
        public List<Dependency> Dependencies { get; set; }
        public bool Enabled { get; set; }

        public string Filename { get; set; }

        public BitmapImage InactiveIcon { get; set; }
        public ModInfo Infos { get; set; }
        public string SearchString1 => Infos.name != null ? Infos.name.ToLower() : "";

        public string SearchString2
        {
            get
            {
                string result = "";
                if (Infos.authorList == null)
                    return "";
                foreach (var item in Infos.authorList)
                    result += item.ToLower();
                return result;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public static Mod CreateFromJSON(JSONModCache cache)
        {
            var mod = new Mod();
            if (cache.inactiveIcon.Length > 0)
            {
                var iconPath = Path.Combine(MainWindow.CacheDir, cache.inactiveIcon);
                mod.InactiveIcon = MainWindow.ToBitmapImage(new System.Drawing.Bitmap(iconPath));
            }
            else
                mod.InactiveIcon = MainWindow.DefaultInactiveModIcon;
            if (cache.activeIcon.Length > 0)
            {
                var iconPath = Path.Combine(MainWindow.CacheDir, cache.activeIcon);
                mod.ActiveIcon = MainWindow.ToBitmapImage(new System.Drawing.Bitmap(iconPath));
            }
            else
                mod.ActiveIcon = MainWindow.DefaultActiveModIcon;
            if (cache.backgroundImage.Length > 0)
            {
                var backgroundPath = Path.Combine(MainWindow.CacheDir, cache.backgroundImage);
                mod.Background = MainWindow.ToBitmapImage(new System.Drawing.Bitmap(backgroundPath));
            }
            else
                mod.Background = null;
            mod.Infos = cache.infos;
            mod.Dependencies = new List<Dependency>(cache.dependencies);
            return mod;
        }

        public static bool operator !=(Mod mod1, Mod mod2)
        {
            return !(mod1 == mod2);
        }

        public static bool operator ==(Mod mod1, Mod mod2)
        {
            return EqualityComparer<Mod>.Default.Equals(mod1, mod2);
        }

        public void ChangeState()
        {
            var path = Path.Combine(!Enabled ? MainWindow.ModDir : MainWindow.DisabledModDir, Filename);
            var target = Path.Combine(Enabled ? MainWindow.ModDir : MainWindow.DisabledModDir, Filename);
            if (File.Exists(path))
                File.Move(path, target);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Mod);
        }

        public bool Equals(Mod other)
        {
            return other != null &&
                   Filename == other.Filename;
        }

        #endregion Public Methods
    }
}