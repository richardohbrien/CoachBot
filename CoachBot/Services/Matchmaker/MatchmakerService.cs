﻿using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoachBot.Model;
using System;
using CoachBot.Extensions;
using Discord.WebSocket;
using System.Threading.Tasks;
using RconSharp;

namespace CoachBot.Services.Matchmaker
{
    public class MatchmakerService
    {
        private readonly ConfigService _configService;
        private readonly StatisticsService _statisticsService;
        private DiscordSocketClient _client;

        public MatchmakerService(ConfigService configService, StatisticsService statisticsService, DiscordSocketClient client)
        {
            _configService = configService;
            _statisticsService = statisticsService;
            _client = client;
        }

        public string ConfigureChannel(ulong channelId, string teamName, List<Position> positions, int regionId, string kitEmote = null, string badgeEmote = null, string color = null, bool isMixChannel = false, Formation formation = 0, bool classicLineup = false, bool disableSearchNotifications = false, bool enableUnsignWhenPlayerStartsOtherGame = false)
        {
            if (positions.Count() <= 1) return ":no_entry: You must add at least two positions";
            if (positions.GroupBy(p => p).Where(g => g.Count() > 1).Any()) return ":no_entry: All positions must be unique";

            var existingChannelConfig = _configService.Config.Channels.FirstOrDefault(c => c.Id.Equals(channelId));
            if (existingChannelConfig != null) _configService.Config.Channels.Remove(existingChannelConfig);

            var channel = new Channel()
            {
                Id = channelId,
                Positions = positions.Select(p => new Position() { PositionName = p.PositionName.ToUpper() }).ToList(),
                Team1 = new Team()
                {
                    IsMix = true,
                    Name = teamName,
                    KitEmote = kitEmote,
                    BadgeEmote = badgeEmote,
                    Color = color,
                    Players = new List<Player>(),
                    Substitutes = new List<Player>()
                },
                Team2 = new Team()
                {
                    IsMix = isMixChannel,
                    Name = isMixChannel ? "Mix #2" : null,
                    Players = new List<Player>(),
                },
                Formation = formation,
                ClassicLineup = classicLineup,
                IsMixChannel = isMixChannel,
                DisableSearchNotifications = disableSearchNotifications,
                EnableUnsignWhenPlayerStartsOtherGame = enableUnsignWhenPlayerStartsOtherGame,
                RegionId = regionId
            };
            _configService.UpdateChannelConfiguration(channel);
            (_client.GetChannel(channelId) as SocketTextChannel).SendMessageAsync("", embed: GenerateTeamList(channelId, Teams.Team1));
            if (channel.IsMixChannel) (_client.GetChannel(channelId) as SocketTextChannel).SendMessageAsync("", embed: GenerateTeamList(channelId, Teams.Team2));
            return ":white_check_mark: Channel successfully configured";
        }

        public string AddPlayer(ulong channelId, IUser user, string position = null, Teams team = Teams.Team1)
        {
            if (position != null) position = position.Replace("#", string.Empty);
            var channel = _configService.Config.Channels.First(c => c.Id == channelId);
            var player = new Player()
            {
                DiscordUserId = user.Id,
                Name = user.Username,
                DiscordUserMention = user.Mention
            };
            if (channel.SignedPlayers.Any(p => p.DiscordUserId == user.Id)) return $":no_entry: You are already signed, {user.Mention}";
            if (channel.Team1.Substitutes.Any(s => s.DiscordUserId == user.Id))
            {
                var sub = channel.Team1.Substitutes.First(s => s.DiscordUserId == user.Id);
                channel.Team1.Substitutes.Remove(sub);
            }
            if (position == null && team == Teams.Team1)
            {
                position = channel.Positions.FirstOrDefault(p => !channel.Team1.Players.Any(pl => pl.Position.PositionName == p.PositionName)).PositionName;
            }
            else if (position == null && team == Teams.Team2)
            {
                position = channel.Positions.FirstOrDefault(p => !channel.Team2.Players.Any(pl => pl.Position.PositionName == p.PositionName)).PositionName;
            }

            position = position.ToUpper();
            var positionAvailableTeam1 = !channel.Team1.Players.Any(p => p.Position.PositionName == position) && channel.Positions.Any(p => p.PositionName == position);
            var positionAvailableTeam2 = !channel.Team2.Players.Any(p => p.Position.PositionName == position) && channel.Positions.Any(p => p.PositionName == position) && channel.Team2.IsMix;

            if (positionAvailableTeam1 && team == Teams.Team1)
            {
                player.Position = new Position(position);
                channel.Team1.Players.Add(player);
                return $":white_check_mark:  Signed **{player.Name}** to **{position}** for **{channel.Team1.Name}**";
            }
            else if (positionAvailableTeam2)
            {
                player.Position = new Position(position);
                channel.Team2.Players.Add(player);
                return $":white_check_mark:  Signed **{player.Name}** to **{position}** for **{channel.Team2.Name ?? "Mix"}**";
            }
            else
            {
                return $":no_entry: Position unavailable. Please try again, {user.Mention}.";
            }
        }

        public string AddPlayer(ulong channelId, string playerName, string position, Teams team = Teams.Team1)
        {
            position = position.Replace("#", string.Empty);
            var channel = _configService.Config.Channels.First(c => c.Id == channelId);
            var player = new Player()
            {
                Name = playerName,
                Position = new Position()
            };
            position = position.ToUpper();
            if (channel.SignedPlayers.Any(p => p.Name == playerName)) return $":no_entry: **{playerName}** is already signed.";
            var positionAvailableTeam1 = !channel.Team1.Players.Any(p => p.Position.PositionName == position) && channel.Positions.Any(p => p.PositionName == position);
            var positionAvailableTeam2 = !channel.Team2.Players.Any(p => p.Position.PositionName == position) && channel.Positions.Any(p => p.PositionName == position);
            if (positionAvailableTeam1 && team == Teams.Team1)
            {
                player.Position.PositionName = position;
                channel.Team1.Players.Add(player);
                return $":white_check_mark:  Signed **{player.Name}** to **{position}** for **{channel.Team1.Name}**";
            }
            else if (positionAvailableTeam2 && channel.Team2.IsMix)
            {
                player.Position.PositionName = position;
                channel.Team2.Players.Add(player);
                return $":white_check_mark:  Signed **{player.Name}** to **{position}** for **{channel.Team2.Name ?? "Mix"}**";
            }
            else
            {
                return ":no_entry: Position unavailable. Please try again.";
            }
        }

        public string RemovePlayer(ulong channelId, IUser user)
        {
            var channel = _configService.Config.Channels.FirstOrDefault(c => c.Id == channelId);
            if (channel.Team1.Players.Any(p => p.DiscordUserId == user.Id))
            {
                var player = channel.Team1.Players.First(p => p.DiscordUserId == user.Id);
                channel.Team1.Players.Remove(player);
                if (channel.Team1.Substitutes.Any() && player.Position.PositionName.ToLower() != "gk")
                {
                    var sub = channel.Team1.Substitutes.FirstOrDefault();
                    channel.Team1.Substitutes.Remove(sub);
                    sub.Position = player.Position;
                    channel.Team1.Players.Add(sub);
                    return $":arrows_counterclockwise:  **Substitution** {Environment.NewLine} {sub.DiscordUserMention} comes off the bench to replace **{user.Username}**";
                }
                return $":negative_squared_cross_mark: Unsigned **{user.Username}**";
            }
            if (channel.Team2.Players.Any(p => p.DiscordUserId == user.Id))
            {
                channel.Team2.Players.Remove(channel.Team2.Players.First(p => p.DiscordUserId == user.Id));
                return $":negative_squared_cross_mark: Unsigned **{user.Username}**";
            }
            return $":no_entry: You are not signed {user.Mention}";
        }

        public string RemovePlayer(ulong channelId, string playerName)
        {
            var channel = _configService.Config.Channels.FirstOrDefault(c => c.Id == channelId);
            if (channel.Team1.Players.Any(p => p.Name == playerName))
            {
                var player = channel.Team1.Players.First(p => p.Name == playerName);
                channel.Team1.Players.Remove(player);
                if (channel.Team1.Substitutes.Any() && player.Position.PositionName.ToLower() != "gk")
                {
                    var sub = channel.Team1.Substitutes.FirstOrDefault();
                    channel.Team1.Substitutes.Remove(sub);
                    sub.Position = player.Position;
                    channel.Team1.Players.Add(sub);
                    return $":arrows_counterclockwise:  **Substitution** {Environment.NewLine} {sub.DiscordUserMention} comes off the bench to replace **{playerName}**";
                }
                return $":negative_squared_cross_mark: Unsigned **{playerName}**";
            }
            if (channel.Team2.Players.Any(p => p.Name == playerName))
            {
                channel.Team2.Players.Remove(channel.Team2.Players.First(p => p.Name == playerName));
                return $":negative_squared_cross_mark: Unsigned **{playerName}**";
            }
            return $":no_entry: **{playerName}** is not signed";
        }

        public string AddSub(ulong channelId, IUser user)
        {
            var channel = _configService.Config.Channels.First(c => c.Id == channelId);
            var player = new Player()
            {
                DiscordUserId = user.Id,
                Name = user.Username,
                DiscordUserMention = user.Mention
            };
            if (channel.Team1.Substitutes.Any(p => p.DiscordUserId == user.Id)) return $":no_entry: You are already signed as a sub, {user.Mention}";
            if (channel.Team1.Players.Any(p => p.DiscordUserId == user.Id)) return $":no_entry: You are already signed, {user.Mention}";

            channel.Team1.Substitutes.Add(player);
            return $":white_check_mark:  Added **{player.Name}** to subs bench for **{channel.Team1.Name ?? "Mix"}**";
        }

        public string RemoveSub(ulong channelId, IUser user)
        {
            var channel = _configService.Config.Channels.First(c => c.Id == channelId);
            var player = channel.Team1.Substitutes.FirstOrDefault(s => s.DiscordUserId == user.Id);
            if (player != null)
            {
                channel.Team1.Substitutes.Remove(player);
                return $":negative_squared_cross_mark: Removed **{player.Name}** from the subs bench";
            }
            return $":no_entry: You are not on the subs bench, {user.Mention}";
        }

        public string RemoveSub(ulong channelId, string playerName)
        {
            var channel = _configService.Config.Channels.First(c => c.Id == channelId);
            var player = channel.Team1.Substitutes.FirstOrDefault(s => s.Name == playerName);
            if (player != null)
            {
                channel.Team1.Substitutes.Remove(player);
                return $":negative_squared_cross_mark: Removed **{player.Name}** from the subs bench";
            }
            return $":no_entry: {playerName} is not on the subs bench";
        }

        public string ChangeOpposition(ulong channelId, Team team, string userMention = null)
        {
            var channel = _configService.Config.Channels.First(c => c.Id == channelId);
            var previousOpposition = _configService.Config.Channels.First(c => c.Id == channelId).Team2;
            channel.Team2 = team;
            if (channel.IsSearching) return $":no_entry: You are currently searching for opposition. Please type **!stopsearch** if you have found an opponent.";
            if (team.Name == null && previousOpposition.Name != null)
            {
                if (channel.IsMixChannel)
                {
                    channel.Team2 = new Team()
                    {
                        Name = "Mix #2",
                        IsMix = true,
                        Players = new List<Player>()
                    };
                }
                return $":negative_squared_cross_mark: Opposition removed";
            }
            if (team.Name == null && previousOpposition.Name == null) return $":no_entry: You must provide a team name to face";
            string confirmationMessage = $":busts_in_silhouette: **{team.Name}** are challenging! ";
            if (!string.IsNullOrEmpty(userMention))
            {
                confirmationMessage += $"Contact {userMention} for more information";
            }

            return confirmationMessage;
        }

        public void ResetMatch(ulong channelId)
        {
            var channelConfig = _configService.ReadChannelConfiguration(channelId);
            _configService.Config.Channels.FirstOrDefault(c => c.Id == channelId).Team1 = new Team()
            {
                IsMix = channelConfig.Team1.IsMix,
                Name = channelConfig.Team1.Name,
                KitEmote = channelConfig.Team1.KitEmote,
                BadgeEmote = channelConfig.Team1.BadgeEmote,
                Color = channelConfig.Team1.Color,
                Players = new List<Player>(),
                Substitutes = new List<Player>()
            };
            _configService.Config.Channels.FirstOrDefault(c => c.Id == channelId).Team2 = new Team()
            {
                IsMix = channelConfig.IsMixChannel,
                Name = channelConfig.IsMixChannel ? "Mix #2" : null,
                Players = new List<Player>()
            };
            _configService.Config.Channels.FirstOrDefault(c => c.Id == channelId).LastHereMention = null;
        }

        public async Task ReadyMatchAsync(ulong channelId, int? serverId = null, bool ignorePlayerCounts = false)
        {
            var channel = _configService.Config.Channels.First(c => c.Id == channelId);
            var socketChannel = (SocketTextChannel)_client.GetChannel(channel.Id);
            var regionServers = _configService.Config.Servers.Where(s => s.RegionId == channel.RegionId).ToList();

            if (!ignorePlayerCounts && channel.Team2.IsMix == true && (channel.Positions.Count() * 2) - 1 > (channel.SignedPlayers.Count()))
            {
                await socketChannel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription(":no_entry: All positions must be filled").Build());
                return;
            }
            if (!ignorePlayerCounts && channel.Team2.IsMix == false && (channel.Positions.Count()) - 1 > (channel.SignedPlayers.Count()))
            {
                await socketChannel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription(":no_entry: All positions must be filled").Build());
                return;
            }
            if (channel.Team2.Name == null)
            {
                await socketChannel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription(":no_entry: You must set a team to face").Build());
                return;
            }
            if (serverId == null || serverId == 0 || serverId > regionServers.Count())
            {
                await socketChannel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription(":no_entry: Please supply a server number (e.g. !ready 3). Type !servers for the server list.").Build());
                return;
            }

            foreach (var otherChannel in _configService.Config.Channels.Where(c => c.Id != channel.Id))
            {
                var playersToRemove = otherChannel.SignedPlayers.Where(p => channel.SignedPlayers.Any(x => x.DiscordUserId != null && x.DiscordUserId > 0 && x.DiscordUserId == p.DiscordUserId)).ToList();
                if (playersToRemove != null)
                {
                    foreach (var player in playersToRemove)
                    {
                        if (_client.GetChannel(otherChannel.Id) is SocketTextChannel otherSocketChannel)
                        {
                            try
                            {
                                var otherMatchmakingChannel = _configService.Config.Channels.FirstOrDefault(c => c.Id == otherChannel.Id);
                                if (otherMatchmakingChannel != null && otherMatchmakingChannel.EnableUnsignWhenPlayerStartsOtherGame)
                                {
                                    await otherSocketChannel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription(RemovePlayer(otherChannel.Id, player.Name)).Build());
                                }
                                await otherSocketChannel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($":stadium: {player.DiscordUserMention ?? player.Name} has gone to play another match with {channel.Name} ({socketChannel.Guild.Name})").Build());
                            }
                            catch
                            {
                                await socketChannel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription("<:coach:578653739766906883> I've picked up a niggle. Can you let the physio know?"));
                            }                            
                        }
                    }
                }
            }

            var server = regionServers[(int)serverId - 1];

            if (!string.IsNullOrEmpty(server.RconPassword) && server.Address.Contains(":"))
            {
                try
                {
                    INetworkSocket socket = new Extensions.RconSocket();
                    RconMessenger messenger = new RconMessenger(socket);
                    bool isConnected = await messenger.ConnectAsync(server.Address.Split(':')[0], int.Parse(server.Address.Split(':')[1]));
                    bool authenticated = await messenger.AuthenticateAsync(server.RconPassword);
                    if (authenticated)
                    {
                        var status = await messenger.ExecuteCommandAsync("status");
                        if (int.Parse(status.Split("players :")[1].Split('(')[0]) < channel.Positions.Count())
                        {
                            await messenger.ExecuteCommandAsync($"exec {channel.Positions.Count()}v{channel.Positions.Count()}.cfg");
                            if (channel.Team1.Players.Any(p => p.Position.PositionName.ToUpper() == "GK"))
                            {
                                await messenger.ExecuteCommandAsync("sv_singlekeeper 0");
                            }
                            else
                            {
                                await messenger.ExecuteCommandAsync("sv_singlekeeper 1");
                            }
                            await messenger.ExecuteCommandAsync("say Have a great game, and remember what I taught you in training - Coach");
                            await socketChannel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription(":stadium: The stadium has successfully been automatically set up").Build());
                        }
                        else
                        {
                            await socketChannel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($":no_entry: The selected server seems to be in use, as there are more than {channel.Positions.Count()} on the server.").Build());
                            return;
                        }
                    }
                }
                catch
                {
                    await socketChannel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($":no_entry: The server seems to be offline. Please choose another server and use the !ready command again.").Build());
                    return;
                }
            }

            var sb = new StringBuilder();
            sb.Append($":checkered_flag: Match Ready! {Environment.NewLine} Join {server.Name} steam://connect/{server.Address} ");
            foreach (var player in channel.SignedPlayers)
            {
                sb.Append($"{player.DiscordUserMention ?? player.Name} ");
            }
            _statisticsService.AddMatch(channel);
            sb.AppendLine();
            ResetMatch(channelId);
            await socketChannel.SendMessageAsync(sb.ToString());           
        }

        public string UnreadyMatch(ulong channelId)
        {
            return ":no entry: This functionality has not yet been implemented";
        }

        public async Task SingleKeeperAsync(ulong channelId, int serverId, bool enable)
        {
            var channel = _configService.Config.Channels.First(c => c.Id == channelId);
            var socketChannel = (SocketTextChannel)_client.GetChannel(channel.Id);
            var regionServers = _configService.Config.Servers.Where(s => s.RegionId == channel.RegionId).ToList();
            var server = regionServers[serverId - 1];
            if (string.IsNullOrEmpty(server.RconPassword) || !server.Address.Contains(":"))
            {
                await socketChannel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription(":no_entry: This server is not set up for auto configuration").Build());
            }
            INetworkSocket socket = new Extensions.RconSocket();
            RconMessenger messenger = new RconMessenger(socket);
            bool isConnected = await messenger.ConnectAsync(server.Address.Split(':')[0], int.Parse(server.Address.Split(':')[1]));
            bool authenticated = await messenger.AuthenticateAsync(server.RconPassword);
            if (authenticated)
            {
                await messenger.ExecuteCommandAsync($"sv_singlekeeper {(enable ? 1 : 0)}");
                await messenger.ExecuteCommandAsync($"say \"Single keeper {(enable ? "enabled" : "disabled")} by Coach\"");
            }
            await socketChannel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"Single keeper {(enable ? "enabled" : "disabled")} on {server.Name}").Build());
        }

        public string Search(ulong channelId, string challengerMention)
        {
            var challenger = _configService.Config.Channels.First(c => c.Id == channelId);
            if (challenger.LastSearch != null && challenger.LastSearch > DateTime.Now.AddMinutes(-10)) return $":no_entry: Your last search started less than 10 minutes ago. Please wait until {String.Format("{0:T}", challenger.LastSearch.Value.AddMinutes(10))} before searching again.";
            if (challenger.IsMixChannel) return ":no_entry: Mix channels cannot search for opposition";
            if (challenger.Positions.Count() - 1 > challenger.SignedPlayers.Count()) return ":no_entry: All outfield positions must be filled";
            if (challenger.IsSearching) return ":no_entry: You're already searching for a match. Type **!stopsearch** to cancel the previous search.";

            var embed = new EmbedBuilder()
                .WithTitle($":mag: {challenger.Team1.BadgeEmote ?? challenger.Team1.Name} are searching for a team to face")
                .WithDescription($"To challenge {challenger.Team1.Name} type **!challenge {challenger.Id}** and contact {challengerMention} for more information")
                .WithCurrentTimestamp();
            if (challenger.Team1.Color != null && challenger.Team1.Color[0] == '#')
            {
                embed.WithColor(new Color(ColorExtensions.FromHex(challenger.Team1.Color).R, ColorExtensions.FromHex(challenger.Team1.Color).G, ColorExtensions.FromHex(challenger.Team1.Color).B));
            }
            else
            {
                embed.WithColor(new Color(0xFFFFFF));
            }
            challenger.IsSearching = true;
            var oppositionServers = _configService.Config.Channels.Where(c =>
                c.Positions.Count == challenger.Positions.Count && c.Id != channelId && c.IsMixChannel == false && !c.DisableSearchNotifications && c.RegionId == challenger.RegionId);
            foreach (var channel in oppositionServers)
            {
                (_client.GetChannel(channel.Id) as SocketTextChannel)?.SendMessageAsync("", embed: embed.Build());
            }
            var timeout = TimeoutSearch(channelId);
            timeout.ConfigureAwait(false);
            challenger.LastSearch = DateTime.Now;

            return ":white_check_mark: Searching for opposition.. To cancel, type **!stopsearch**";
        }

        public async Task TimeoutSearch(ulong channelId)
        {
            await Task.Delay(900000);
            var channel = _configService.Config.Channels.First(c => c.Id == channelId);
            if (channel.IsSearching)
            {
                (_client.GetChannel(channelId) as SocketTextChannel)?.SendMessageAsync("", embed: new EmbedBuilder().WithDescription(":timer: Your search for an opponent has timed out after 15 minutes. Please try again if you are still searching").WithCurrentTimestamp().Build());
                channel.IsSearching = false;
            }
        }

        public string StopSearch(ulong channelId)
        {
            var challenger = _configService.Config.Channels.First(c => c.Id == channelId);
            if (!challenger.IsSearching)
            {
                return ":no_entry: Your team is not currently searching for a match.";
            }
            challenger.IsSearching = false;
            return ":negative_squared_cross_mark: Cancelled search for opposition";
        }

        public string Challenge(ulong challengerChannelId, ulong oppositionId, string challengerMention)
        {
            var challenger = _configService.Config.Channels.First(c => c.Id == challengerChannelId);
            var opposition = _configService.Config.Channels.First(c => c.Id == oppositionId);
            if (challenger.IsMixChannel) return ":no_entry: Mix channels cannot challenge teams";
            if (!opposition.IsSearching) return $":no_entry: {opposition.Team1.Name} are no longer search for a team to face";
            if (challengerChannelId == oppositionId) return $":no_entry: You can't face yourself. Don't waste my time.";
            if (challenger.Positions.Count() != opposition.Positions.Count()) return $":no_entry: Sorry, {opposition.Team1.Name} are looking for an {opposition.Positions.Count()}v{opposition.Positions.Count()}";
            if (Math.Round(challenger.Positions.Count() * 0.7) > challenger.SignedPlayers.Count()) return $":no_entry: At least {Math.Round(challenger.Positions.Count() * 0.7)} positions must be filled";
            if (challenger.RegionId != opposition.RegionId) return $":no_entry: You can't challenge opponents from other regions";
            opposition.IsSearching = false;
            challenger.IsSearching = false;
            var acceptMsg = $":handshake: {challenger.Team1.Name} have accepted the challenge! Contact {challengerMention} to arrange further.";
            (_client.GetChannel(opposition.Id) as SocketTextChannel).SendMessageAsync("", embed: new EmbedBuilder().WithDescription(acceptMsg).WithCurrentTimestamp().Build());
            opposition.Team2 = new Team()
            {
                Name = challenger.Team1.Name,
                IsMix = false,
                ChannelId = challenger.Id,
                Players = new List<Player>()
            };
            challenger.Team2 = new Team()
            {
                Name = opposition.Team1.Name,
                IsMix = false,
                ChannelId = opposition.Id,
                Players = new List<Player>()
            };
            (_client.GetChannel(challengerChannelId) as SocketTextChannel).SendMessageAsync("", embed: GenerateTeamList(challengerChannelId, Teams.Team1));
            (_client.GetChannel(oppositionId) as SocketTextChannel).SendMessageAsync("", embed: GenerateTeamList(oppositionId, Teams.Team1));
            return $":handshake: You have successfully challenged {opposition.Team1.Name}. !ready will send both teams to the server";
        }

        public string Unchallenge(ulong challengerChannelId, string unchallenger)
        {
            var challenger = _configService.Config.Channels.First(c => c.Id == challengerChannelId);
            var opposition = _configService.Config.Channels.First(c => c.Id == challenger.Team2.ChannelId);
            if (opposition == null) return $":no_entry: You don't have any active accepted challenges to cancel. Maybe !ready has already been called?";
            var unchallengeMsg = $"The game between {challenger.Team1.Name} & {opposition.Team1.Name} has been called off by {unchallenger}";
            (_client.GetChannel(opposition.Id) as SocketTextChannel).SendMessageAsync("", embed: new EmbedBuilder().WithTitle(":thunder_cloud_rain: Match Abandoned!").WithDescription(unchallengeMsg).WithCurrentTimestamp().Build());
            (_client.GetChannel(challenger.Id) as SocketTextChannel).SendMessageAsync("", embed: new EmbedBuilder().WithTitle(":thunder_cloud_rain: Match Abandoned!").WithDescription(unchallengeMsg).WithCurrentTimestamp().Build());
            opposition.Team2.Name = null;
            opposition.Team2.ChannelId = null;
            challenger.Team2.Name = null;
            challenger.Team2.ChannelId = null;
            (_client.GetChannel(challengerChannelId) as SocketTextChannel).SendMessageAsync("", embed: GenerateTeamList(challengerChannelId, Teams.Team1));
            (_client.GetChannel(opposition.Id) as SocketTextChannel).SendMessageAsync("", embed: GenerateTeamList(opposition.Id, Teams.Team1));
            return $":negative_squared_cross_mark: You have successfully unchallenged {opposition.Team1.Name}";
        }

        public string MentionHere(ulong channelId)
        {
            var channel = _configService.Config.Channels.First(c => c.Id == channelId);
            if (channel.LastHereMention == null || channel.LastHereMention < DateTime.Now.AddMinutes(-10))
            {
                channel.LastHereMention = DateTime.Now;
                return "@here";
            }
            else
            {
                return $"The last channel highlight was less than 10 minutes ago ({String.Format("{0:T}", channel.LastHereMention)})";
            }
        }

        public Embed GenerateTeamList(ulong channelId, Teams teamType = Teams.Team1)
        {
            var teamList = new StringBuilder();
            var embedFooterBuilder = new EmbedFooterBuilder();
            var channel = _configService.Config.Channels.First(c => c.Id == channelId);
            if (channel.ClassicLineup) return GenerateTeamListVintage(channelId, teamType);
            var availablePlaceholderText = !string.IsNullOrEmpty(channel.Team1.KitEmote) && teamType == Teams.Team1 ? channel.Team1.KitEmote : ":shirt:";
            if (teamType == Teams.Team2 && (channelId == 252113301004222465 || channelId == 295580567649648641)) availablePlaceholderText = "<:redshirt:318130493755228160>";
            if (teamType == Teams.Team2 && channelId == 310829524277395457) availablePlaceholderText = "<:redshirt:318114878063902720>";
            var team = teamType == Teams.Team1 ? channel.Team1 : channel.Team2;
            var oppositionTeam = teamType == Teams.Team1 ? channel.Team2 : channel.Team1;
            var builder = new EmbedBuilder().WithTitle($"{team.BadgeEmote ?? team.Name} Team Sheet")
                                            .WithDescription(oppositionTeam.Name != null ? $"vs {oppositionTeam.Name}" : "")
                                            .WithCurrentTimestamp();
            if (teamType == Teams.Team1)
            {
                if (team.Color != null && team.Color[0] == '#')
                {
                    builder.WithColor(new Color(ColorExtensions.FromHex(team.Color).R, ColorExtensions.FromHex(team.Color).G, ColorExtensions.FromHex(team.Color).B));
                }
                else
                {
                    builder.WithColor(new Color(0x2463b0));
                }
            }
            else
            {
                builder.WithColor(new Color(0xd60e0e));
            }

            if (channel.Positions.Count() == 8 && channel.Formation == Formation.ThreeThreeOne)
            {
                builder.AddInlineField("\u200B", "\u200B");
                var player8 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[7].PositionName);
                builder.AddInlineField(player8 != null ? player8.Name : availablePlaceholderText, AddPrefix(channel.Positions[7].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player7 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[6].PositionName);
                builder.AddInlineField(player7 != null ? player7.Name : availablePlaceholderText, AddPrefix(channel.Positions[6].PositionName));
                var player6 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[5].PositionName);
                builder.AddInlineField(player6 != null ? player6.Name : availablePlaceholderText, AddPrefix(channel.Positions[5].PositionName));
                var player5 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[4].PositionName);
                builder.AddInlineField(player5 != null ? player5.Name : availablePlaceholderText, AddPrefix(channel.Positions[4].PositionName));
                var player4 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[3].PositionName);
                builder.AddInlineField(player4 != null ? player4.Name : availablePlaceholderText, AddPrefix(channel.Positions[3].PositionName));
                var player3 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[2].PositionName);
                builder.AddInlineField(player3 != null ? player3.Name : availablePlaceholderText, AddPrefix(channel.Positions[2].PositionName));
                var player2 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[1].PositionName);
                builder.AddInlineField(player2 != null ? player2.Name : availablePlaceholderText, AddPrefix(channel.Positions[1].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player1 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[0].PositionName);
                builder.AddInlineField(player1 != null ? player1.Name : availablePlaceholderText, AddPrefix(channel.Positions[0].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
            }
            else if (channel.Positions.Count() == 8 && channel.Formation == Formation.ThreeTwoTwo)
            {
                var player8 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[7].PositionName);
                builder.AddInlineField(player8 != null ? player8.Name : availablePlaceholderText, AddPrefix(channel.Positions[7].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player7 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[6].PositionName);
                builder.AddInlineField(player7 != null ? player7.Name : availablePlaceholderText, AddPrefix(channel.Positions[6].PositionName));
                var player6 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[5].PositionName);
                builder.AddInlineField(player6 != null ? player6.Name : availablePlaceholderText, AddPrefix(channel.Positions[5].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player5 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[4].PositionName);
                builder.AddInlineField(player5 != null ? player5.Name : availablePlaceholderText, AddPrefix(channel.Positions[4].PositionName));
                var player4 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[3].PositionName);
                builder.AddInlineField(player4 != null ? player4.Name : availablePlaceholderText, AddPrefix(channel.Positions[3].PositionName));
                var player3 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[2].PositionName);
                builder.AddInlineField(player3 != null ? player3.Name : availablePlaceholderText, AddPrefix(channel.Positions[2].PositionName));
                var player2 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[1].PositionName);
                builder.AddInlineField(player2 != null ? player2.Name : availablePlaceholderText, AddPrefix(channel.Positions[1].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player1 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[0].PositionName);
                builder.AddInlineField(player1 != null ? player1.Name : availablePlaceholderText, AddPrefix(channel.Positions[0].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
            }
            else if (channel.Positions.Count() == 8 && channel.Formation == Formation.ThreeOneTwoOne)
            {
                builder.AddInlineField("\u200B", "\u200B");
                var player8 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[7].PositionName);
                builder.AddInlineField(player8 != null ? player8.Name : availablePlaceholderText, AddPrefix(channel.Positions[7].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player7 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[6].PositionName);
                builder.AddInlineField(player7 != null ? player7.Name : availablePlaceholderText, AddPrefix(channel.Positions[6].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player6 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[5].PositionName);
                builder.AddInlineField(player6 != null ? player6.Name : availablePlaceholderText, AddPrefix(channel.Positions[5].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player5 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[4].PositionName);
                builder.AddInlineField(player5 != null ? player5.Name : availablePlaceholderText, AddPrefix(channel.Positions[4].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player4 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[3].PositionName);
                builder.AddInlineField(player4 != null ? player4.Name : availablePlaceholderText, AddPrefix(channel.Positions[3].PositionName));
                var player3 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[2].PositionName);
                builder.AddInlineField(player3 != null ? player3.Name : availablePlaceholderText, AddPrefix(channel.Positions[2].PositionName));
                var player2 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[1].PositionName);
                builder.AddInlineField(player2 != null ? player2.Name : availablePlaceholderText, AddPrefix(channel.Positions[1].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player1 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[0].PositionName);
                builder.AddInlineField(player1 != null ? player1.Name : availablePlaceholderText, AddPrefix(channel.Positions[0].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
            }
            else if (channel.Positions.Count() == 8 && channel.Formation == Formation.ThreeOneThree)
            {
                var player8 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[7].PositionName);
                builder.AddInlineField(player8 != null ? player8.Name : availablePlaceholderText, AddPrefix(channel.Positions[7].PositionName));
                var player7 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[6].PositionName);
                builder.AddInlineField(player7 != null ? player7.Name : availablePlaceholderText, AddPrefix(channel.Positions[6].PositionName));
                var player6 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[5].PositionName);
                builder.AddInlineField(player6 != null ? player6.Name : availablePlaceholderText, AddPrefix(channel.Positions[5].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player5 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[4].PositionName);
                builder.AddInlineField(player5 != null ? player5.Name : availablePlaceholderText, AddPrefix(channel.Positions[4].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player4 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[3].PositionName);
                builder.AddInlineField(player4 != null ? player4.Name : availablePlaceholderText, AddPrefix(channel.Positions[3].PositionName));
                var player3 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[2].PositionName);
                builder.AddInlineField(player3 != null ? player3.Name : availablePlaceholderText, AddPrefix(channel.Positions[2].PositionName));
                var player2 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[1].PositionName);
                builder.AddInlineField(player2 != null ? player2.Name : availablePlaceholderText, AddPrefix(channel.Positions[1].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player1 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[0].PositionName);
                builder.AddInlineField(player1 != null ? player1.Name : availablePlaceholderText, AddPrefix(channel.Positions[0].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
            }
            else if (channel.Positions.Count() == 4 && channel.Formation == Formation.TwoOne)
            {
                builder.AddInlineField("\u200B", "\u200B");
                var player4 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[3].PositionName);
                builder.AddInlineField(player4 != null ? player4.Name : availablePlaceholderText, AddPrefix(channel.Positions[3].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player3 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[2].PositionName);
                builder.AddInlineField(player3 != null ? player3.Name : availablePlaceholderText, AddPrefix(channel.Positions[2].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player2 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[1].PositionName);
                builder.AddInlineField(player2 != null ? player2.Name : availablePlaceholderText, AddPrefix(channel.Positions[1].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
                var player1 = team.Players.FirstOrDefault(p => p.Position.PositionName == channel.Positions[0].PositionName);
                builder.AddInlineField(player1 != null ? player1.Name : availablePlaceholderText, AddPrefix(channel.Positions[0].PositionName));
                builder.AddInlineField("\u200B", "\u200B");
            }
            else
            {
                foreach (var position in channel.Positions)
                {
                    var player = team.Players.FirstOrDefault(p => p.Position.PositionName == position.PositionName);
                    builder.AddInlineField(player != null ? player.Name : availablePlaceholderText, AddPrefix(position.PositionName));
                }
                if (channel.Positions.Count() % 3 == 2) // Ensure that two-column fields are three-columns to ugly alignment
                {
                    builder.AddInlineField("\u200B", "\u200B");
                }
            }
            if (teamType == Teams.Team1 && team.Substitutes.Any())
            {
                var subs = new StringBuilder();
                foreach (var sub in team.Substitutes)
                {
                    if (team.Substitutes.Last() != sub)
                    {
                        subs.Append($"{sub.Name}, ");
                    }
                    else
                    {
                        subs.Append($"{sub.Name}");
                    }
                }
                builder.AddField("Subs", subs.ToString());
            }
            return builder.Build();
        }

        public Embed GenerateTeamListVintage(ulong channelId, Teams teamType = Teams.Team1)
        {
            var teamList = new StringBuilder();
            var channel = _configService.Config.Channels.First(c => c.Id == channelId);
            var team = teamType == Teams.Team1 ? channel.Team1 : channel.Team2;
            var sb = new StringBuilder();
            var teamColor = new Color(teamType == Teams.Team1 ? (uint)0x2463b0 : (uint)0xd60e0e);
            var emptyPos = ":grey_question:";

            if (team.Color != null && teamType == Teams.Team1 && team.Color[0] == '#')
            {
                teamColor = new Color(ColorExtensions.FromHex(team.Color).R, ColorExtensions.FromHex(team.Color).G, ColorExtensions.FromHex(team.Color).B);
            }

            var embedBuilder = new EmbedBuilder().WithTitle($"{team.BadgeEmote ?? team.Name} Team List");
            foreach (var position in channel.Positions)
            {
                var player = team.Players.FirstOrDefault(p => p.Position.PositionName == position.PositionName);
                var playerName = player != null ? $"**{player.Name}**" : emptyPos;
                sb.Append($"{position.PositionName}:{playerName} ");
            }
            if (teamType == Teams.Team1 && team.Substitutes.Any())
            {
                var subs = new StringBuilder();
                foreach (var sub in team.Substitutes)
                {
                    if (team.Substitutes.Last() != sub)
                    {
                        subs.Append($"{sub.Name}, ");
                    }
                    else
                    {
                        subs.Append($"{sub.Name}");
                    }
                }
                sb.Append($"*Subs*: **{subs.ToString()}**");
            }
            if (!string.IsNullOrEmpty(channel.Team2.Name) && teamType == Teams.Team1 && !channel.Team2.IsMix)
            {
                sb.AppendLine("");
                sb.Append($"vs {channel.Team2.Name}");
            }

            return embedBuilder.WithColor(teamColor).WithDescription(sb.ToString()).WithCurrentTimestamp().Build();
        }

        public static string AddPrefix(string position)
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
