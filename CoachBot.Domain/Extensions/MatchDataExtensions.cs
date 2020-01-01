﻿using CoachBot.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoachBot.Domain.Extensions
{
    public static class MatchDataExtensions
    {
        public static int GetMatchStatistic(this MatchData matchData, MatchDataStatisticType matchDataStatisticType, MatchDataTeamType matchDataTeamType)
        {
            return matchData.Teams[(int)matchDataTeamType].MatchTotal.Statistics[(int)matchDataStatisticType];
        }
    }
}