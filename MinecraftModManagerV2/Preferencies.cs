using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftModManagerV2
{
    [Serializable]
    public struct Preferencies : ISerializable, IEquatable<Preferencies>
    {
        #region Public Fields

        public bool customDir;

        public string dirString;

        #endregion Public Fields

        #region Public Constructors

        public Preferencies(SerializationInfo info, StreamingContext context)
        {
            customDir = info.GetBoolean("customDirectory");
            if (customDir)
                dirString = info.GetString("directory");
            else
                dirString = "";
        }

        #endregion Public Constructors

        #region Public Methods

        public static bool operator !=(Preferencies preferencies1, Preferencies preferencies2)
        {
            return !(preferencies1 == preferencies2);
        }

        public static bool operator ==(Preferencies preferencies1, Preferencies preferencies2)
        {
            return preferencies1.Equals(preferencies2);
        }

        public override bool Equals(object obj)
        {
            return obj is Preferencies && Equals((Preferencies)obj);
        }

        public bool Equals(Preferencies other)
        {
            if (customDir == other.customDir && !customDir)
                return true;
            else
                return customDir == other.customDir &&
                   dirString == other.dirString;
        }

        public override int GetHashCode()
        {
            var hashCode = -810236670;
            hashCode = hashCode * -1521134295 + customDir.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(dirString);
            return hashCode;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("customDirectory", customDir);
            if (customDir)
                info.AddValue("directory", dirString);
        }

        #endregion Public Methods
    }
}