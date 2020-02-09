﻿using CoachBot.Domain.Model;
using CoachBot.Extensions;
using CoachBot.Model;
using Discord;
using System.Linq;
using System.Text;

namespace CoachBot.Factories
{
    public static class TeamSheetEmbedFactory
    {
        private const uint DEFAULT_EMBED_HOME_TEAM_COLOUR = 0x2463b0;
        private const uint DEFAULT_EMBED_AWAY_TEAM_COLOUR = 0xd60e0e;
        private const string DEFAULT_KIT_EMOTE = "<:redshirt:318114878063902720>";
        private const string UNICODE_SPACE = "\u200B";

        public static Embed GenerateEmbed(Channel channel, Match match, TeamType teamType = TeamType.Home)
        {
            var teamList = new StringBuilder();
            var teamColor = new Color(DEFAULT_EMBED_HOME_TEAM_COLOUR);
            var embedFooterBuilder = new EmbedFooterBuilder();
            var availablePlaceholderText = !string.IsNullOrEmpty(channel.KitEmote) && teamType == TeamType.Home ? channel.KitEmote : DEFAULT_KIT_EMOTE;

            Team team;
            Channel oppositionChannel = null;
            if (teamType == TeamType.Home)
            {
                team = match.TeamHome;
                oppositionChannel = match.TeamAway?.Channel;
                teamColor = channel.SystemColor;
            }
            else
            {
                team = match.TeamAway;
                oppositionChannel = match.TeamHome?.Channel;
                if (match.IsMixMatch)
                {
                    teamColor = new Color(DEFAULT_EMBED_AWAY_TEAM_COLOUR);
                }
            }


            if (teamType == TeamType.Home && channel.Color != null && channel.Color[0] == '#')
            {
                teamColor = new Color(ColorExtensions.FromHex(channel.Color).R, ColorExtensions.FromHex(channel.Color).G, ColorExtensions.FromHex(channel.Color).B);
            }

            var builder = new EmbedBuilder()
                           .WithTitle($"{team.Channel?.BadgeEmote ?? channel.Name} Team Sheet")
                           .WithCurrentTimestamp()
                           .WithColor(teamColor);

            if (!match.IsMixMatch && oppositionChannel != null)
            {
                builder.WithDescription($"vs {oppositionChannel.Name}");
            }

            if (channel.ChannelPositions.Count() == 8 && channel.Formation == Formation.ThreeThreeOne)
            {
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player8 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[7].Position.Name);
                builder.AddInlineField(player8 != null ? player8.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[7].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player7 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[6].Position.Name);
                builder.AddInlineField(player7 != null ? player7.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[6].Position.Name));
                var player6 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[5].Position.Name);
                builder.AddInlineField(player6 != null ? player6.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[5].Position.Name));
                var player5 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[4].Position.Name);
                builder.AddInlineField(player5 != null ? player5.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[4].Position.Name));
                var player4 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[3].Position.Name);
                builder.AddInlineField(player4 != null ? player4.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[3].Position.Name));
                var player3 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[2].Position.Name);
                builder.AddInlineField(player3 != null ? player3.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[2].Position.Name));
                var player2 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[1].Position.Name);
                builder.AddInlineField(player2 != null ? player2.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[1].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player1 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[0].Position.Name);
                builder.AddInlineField(player1 != null ? player1.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[0].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
            }
            else if (channel.ChannelPositions.Count() == 8 && channel.Formation == Formation.ThreeTwoTwo)
            {
                var player8 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[7].Position.Name);
                builder.AddInlineField(player8 != null ? player8.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[7].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player7 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[6].Position.Name);
                builder.AddInlineField(player7 != null ? player7.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[6].Position.Name));
                var player6 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[5].Position.Name);
                builder.AddInlineField(player6 != null ? player6.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[5].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player5 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[4].Position.Name);
                builder.AddInlineField(player5 != null ? player5.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[4].Position.Name));
                var player4 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[3].Position.Name);
                builder.AddInlineField(player4 != null ? player4.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[3].Position.Name));
                var player3 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[2].Position.Name);
                builder.AddInlineField(player3 != null ? player3.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[2].Position.Name));
                var player2 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[1].Position.Name);
                builder.AddInlineField(player2 != null ? player2.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[1].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player1 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[0].Position.Name);
                builder.AddInlineField(player1 != null ? player1.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[0].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
            }
            else if (channel.ChannelPositions.Count() == 8 && channel.Formation == Formation.ThreeOneTwoOne)
            {
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player8 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[7].Position.Name);
                builder.AddInlineField(player8 != null ? player8.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[7].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player7 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[6].Position.Name);
                builder.AddInlineField(player7 != null ? player7.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[6].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player6 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[5].Position.Name);
                builder.AddInlineField(player6 != null ? player6.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[5].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player5 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[4].Position.Name);
                builder.AddInlineField(player5 != null ? player5.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[4].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player4 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[3].Position.Name);
                builder.AddInlineField(player4 != null ? player4.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[3].Position.Name));
                var player3 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[2].Position.Name);
                builder.AddInlineField(player3 != null ? player3.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[2].Position.Name));
                var player2 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[1].Position.Name);
                builder.AddInlineField(player2 != null ? player2.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[1].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player1 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[0].Position.Name);
                builder.AddInlineField(player1 != null ? player1.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[0].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
            }
            else if (channel.ChannelPositions.Count() == 8 && channel.Formation == Formation.ThreeOneThree)
            {
                var player8 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[7].Position.Name);
                builder.AddInlineField(player8 != null ? player8.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[7].Position.Name));
                var player7 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[6].Position.Name);
                builder.AddInlineField(player7 != null ? player7.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[6].Position.Name));
                var player6 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[5].Position.Name);
                builder.AddInlineField(player6 != null ? player6.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[5].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player5 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[4].Position.Name);
                builder.AddInlineField(player5 != null ? player5.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[4].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player4 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[3].Position.Name);
                builder.AddInlineField(player4 != null ? player4.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[3].Position.Name));
                var player3 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[2].Position.Name);
                builder.AddInlineField(player3 != null ? player3.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[2].Position.Name));
                var player2 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[1].Position.Name);
                builder.AddInlineField(player2 != null ? player2.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[1].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player1 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[0].Position.Name);
                builder.AddInlineField(player1 != null ? player1.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[0].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
            }
            else if (channel.ChannelPositions.Count() == 4 && channel.Formation == Formation.TwoOne)
            {
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player4 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[3].Position.Name);
                builder.AddInlineField(player4 != null ? player4.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[3].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player3 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[2].Position.Name);
                builder.AddInlineField(player3 != null ? player3.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[2].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player2 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[1].Position.Name);
                builder.AddInlineField(player2 != null ? player2.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[1].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                var player1 = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == channel.ChannelPositions.ToArray()[0].Position.Name);
                builder.AddInlineField(player1 != null ? player1.Player.Name : availablePlaceholderText, AddNumericPrefix(channel.ChannelPositions.ToArray()[0].Position.Name));
                builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
            }
            else
            {
                foreach (var position in channel.ChannelPositions)
                {
                    var player = team.PlayerTeamPositions.FirstOrDefault(p => p.Position.Name == position.Position.Name);
                    builder.AddInlineField(player != null ? player.Player.Name : availablePlaceholderText, AddNumericPrefix(position.Position.Name));
                }
                if (channel.ChannelPositions.Count() % 3 == 2) // Ensure that two-column fields are three-columns to ugly alignment
                {
                    builder.AddInlineField(UNICODE_SPACE, UNICODE_SPACE);
                }
            }

            if (teamType == TeamType.Home && team.PlayerSubstitutes.Any()) builder.AddField("Subs", string.Join(", ", match.TeamHome.PlayerSubstitutes.Select(ps => ps.Player.DiscordUserMention ?? ps.Player.Name)));

            return builder.Build();
        }

        private static string AddNumericPrefix(string position)
        {
            if (int.TryParse(position, out int parsedInt) == true)
            {
                return $"#{position}";
            }
            else
            {
                return position;
            }
        }

    }
}