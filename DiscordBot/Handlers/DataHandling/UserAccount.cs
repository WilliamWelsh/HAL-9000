using System.Collections.Generic;

namespace Gideon
{
    public class UserAccount
    {
        public ulong userID { get; set; }
        public int coins { get; set; }
        public bool superadmin { get; set; }
        public int localTime { get; set; }
        public string country { get; set; }
        public int xp { get; set; }
        public uint level { get; set; }
    }
}