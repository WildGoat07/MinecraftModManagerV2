using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftModManagerV2
{
    public struct Profil : IEquatable<Profil>
    {
        #region Public Fields

        public string[] modids;
        public string name;

        #endregion Public Fields

        #region Public Methods

        public static bool operator !=(Profil profil1, Profil profil2)
        {
            return !(profil1 == profil2);
        }

        public static bool operator ==(Profil profil1, Profil profil2)
        {
            return profil1.Equals(profil2);
        }

        public override bool Equals(object obj)
        {
            return obj is Profil && Equals((Profil)obj);
        }

        public bool Equals(Profil other)
        {
            return name.Equals(other.name);
        }

        public override int GetHashCode()
        {
            return 1213502048 + EqualityComparer<string>.Default.GetHashCode(name);
        }

        public override string ToString() => name;

        #endregion Public Methods
    }
}