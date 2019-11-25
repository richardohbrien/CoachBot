﻿using CoachBot.Domain.Services;
using CoachBot.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;

namespace CoachBot.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class MatchStatisticController : Controller
    {
        private readonly MatchService _matchService;
        private readonly MatchStatisticsService _matchStatisticsService;

        public MatchStatisticController(MatchService matchService, MatchStatisticsService matchStatisticsService)
        {
            _matchService = matchService;
            _matchStatisticsService = matchStatisticsService;
        }

        [HttpPost]
        public IActionResult Submit(MatchStatisticsDto matchStatisticsDto)
        {
            var base64EncodedBytes = Convert.FromBase64String(matchStatisticsDto.Access_Token);
            var token = Encoding.UTF8.GetString(base64EncodedBytes);
            var serverAddress = token.Split("_")[0];
            var matchId = int.Parse(token.Split("_")[1]);
            var match = _matchService.GetMatch(matchId);

            if (match.Server.Address != serverAddress)
            {
                return BadRequest();
            }

            if (_matchStatisticsService.GetMatchStatistics(matchId) != null)
            {
                return BadRequest();
            }

            if (serverAddress.Split(":")[0] != Request.HttpContext.Connection.RemoteIpAddress.ToString())
            {
                return Unauthorized();
            }

            _matchStatisticsService.SaveMatchData(matchStatisticsDto.MatchData, matchId);

            return Ok();
        }
    }
}
