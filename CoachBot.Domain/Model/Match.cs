﻿using CoachBot.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CoachBot.Domain.Model
{
    public class Match
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? TeamHomeId { get; set; }

        public int? TeamAwayId { get; set; }

        [ForeignKey("TeamHomeId")]
        public Team TeamHome { get; set; }

        [ForeignKey("TeamAwayId")]
        public Team TeamAway { get; set; }

        public int? ServerId { get; set; }

        public Server Server { get; set; }

        public int? MatchStatisticsId { get; set; }

        [ForeignKey("MatchStatisticsId")]
        public MatchStatistics MatchStatistics { get; set; }

        public DateTime? ReadiedDate { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDate { get; set; }

        public bool IsMixMatch => TeamHome?.ChannelId == TeamAway?.ChannelId;

        public Team GetTeam(TeamType teamType) => teamType == TeamType.Home ? TeamHome : TeamAway;

        public string MatchFormat => "8v8";

        [JsonIgnore]
        [NotMapped]
        public List<Player> SignedPlayers
        {
            get
            {
                var players = new List<Player>();
                if (TeamHome != null && TeamHome.PlayerTeamPositions != null)
                {
                    players.AddRange(TeamHome.PlayerTeamPositions.Select(ptp => ptp.Player));
                }
                if (TeamAway != null && TeamAway.PlayerTeamPositions != null)
                {
                    players.AddRange(TeamAway.PlayerTeamPositions.Select(ptp => ptp.Player));
                }

                return players;
            }
        }

        [JsonIgnore]
        [NotMapped]
        public List<Player> SignedSubstitutes
        {
            get
            {
                var players = new List<Player>();
                if (TeamHome != null && TeamHome.PlayerSubstitutes != null)
                {
                    players.AddRange(TeamHome.PlayerSubstitutes.Select(ps => ps.Player));
                }
                if (TeamAway != null && TeamAway.PlayerSubstitutes != null)
                {
                    players.AddRange(TeamAway.PlayerSubstitutes.Select(ps => ps.Player));
                }

                return players;
            }
        }

        [JsonIgnore]
        [NotMapped]
        public List<Player> SignedPlayersAndSubs
        {
            get
            {
                var players = new List<Player>();
                players.AddRange(SignedPlayers);
                players.AddRange(SignedSubstitutes);

                return players;
            }
        }

    }
}
