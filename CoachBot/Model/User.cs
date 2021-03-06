﻿using System.Collections.Generic;

namespace CoachBot.Model
{
    public class User
    {
        public string Name { get; set; }

        public ulong DiscordUserId { get; set; }

        public string DiscordUserIdString { get { return DiscordUserId.ToString(); } }

        public bool IsAdministrator { get; set; }

        public IEnumerable<Channel> Channels { get; set; }

    }
}
