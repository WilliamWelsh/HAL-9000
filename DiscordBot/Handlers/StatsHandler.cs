using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon.Handlers
{
    class StatsHandler
    {
        public string GetCreatedDate(SocketGuildUser user)
        {
            string date = "";
            try
            {
                date = user.CreatedAt.ToString("MMMM dd, yyy");
            }
            catch (Exception)
            {
                date = "Error finding date.";
            }
            return date;
        }

        public string GetJoinedDate(SocketGuildUser user)
        {
            string date = "";
            try
            {
                date = ((DateTimeOffset)user.JoinedAt).ToString("MMMM dd, yyy");
            }
            catch (Exception)
            {
                date = "Error finding date.";
            }
            return date;
        }
    }
}