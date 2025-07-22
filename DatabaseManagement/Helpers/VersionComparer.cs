using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseManagement.Helpers
{
    public class VersionComparer : System.Collections.Generic.IComparer<string>
    {
        private static VersionComparer _Instance;
        public static VersionComparer Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new VersionComparer();
                return _Instance;
            }
        }
        public int Compare(string x, string y)
        {
            Version xVersion = new Version(x.ToString());
            Version yVersion = new Version(y.ToString());
            return xVersion.CompareTo(yVersion);
        }
    }
}
