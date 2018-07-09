using System.Collections.Generic;

namespace Gideon
{
    public class UserAccount
    {
        public ulong UserID { get; set; }
        public int Tecos { get; set; }
        public bool hasDoubleTecoBoost { get; set; }
        public bool superadmin { get; set; }
        public int localTime { get; set; }
        public string Country { get; set; }
        public uint Warns { get; set; }
        public List<string> warnReasons = new List<string>();
        public List<string> Warners = new List<string>();
    }
}