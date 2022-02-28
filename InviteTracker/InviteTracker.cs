using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HAL9000
{
    public class InviteTracker
    {
        private InvitedHistory[] InvitedHistory;

        public InviteTracker() => InvitedHistory = JsonConvert.DeserializeObject<InvitedHistory[]>(File.ReadAllText("Resources/inviteHistory.json"));

        public string GetInviterAsync(IReadOnlyCollection<Discord.Rest.RestInviteMetadata> currentInvites, ulong userID)
        {
            var invites = DeserializeInvites();

            foreach (var currentInvite in currentInvites)
            {
                var savedVersion = invites.Where(invite => invite.Id == currentInvite.Code).FirstOrDefault();

                if (currentInvite.Uses > savedVersion?.Uses)
                {
                    // Save invite codes and count
                    SaveInvites(currentInvites);

                    // Save user ID with the code they joined with
                    Array.Resize(ref InvitedHistory, InvitedHistory.Length + 1);
                    InvitedHistory[InvitedHistory.GetUpperBound(0)] = new InvitedHistory
                    {
                        UserID = userID,
                        InviteCode = currentInvite.Code
                    };
                    File.WriteAllText("Resources/inviteHistory.json", JsonConvert.SerializeObject(InvitedHistory, Formatting.None));

                    // Return the result
                    return $"Invited by {currentInvite.Inviter.Mention} (`{currentInvite.Code}`).";
                }
            }

            SaveInvites(currentInvites);
            return "I couldn't find who invited them.";
        }

        // Save the invite list
        public void SaveInvites(IReadOnlyCollection<Discord.Rest.RestInviteMetadata> currentInvites)
        {
            var savedInvites = new Invite[currentInvites.Count];

            for (int i = 0; i < savedInvites.Length; i++)
            {
                savedInvites[i] = new Invite
                {
                    Id = currentInvites.ElementAt(i).Code,
                    Uses = (int)currentInvites.ElementAt(i).Uses
                };
            }

            File.WriteAllText("Resources/invites.json", JsonConvert.SerializeObject(savedInvites, Formatting.None));
        }

        // Read the invite list
        private Invite[] DeserializeInvites() => JsonConvert.DeserializeObject<Invite[]>(File.ReadAllText("Resources/invites.json"));
    }
}