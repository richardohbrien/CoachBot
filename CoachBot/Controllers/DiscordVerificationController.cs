﻿using CoachBot.Domain.Services;
using CoachBot.Extensions;
using CoachBot.Services;
using CoachBot.Tools;
using Effortless.Net.Encryption;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CoachBot.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class DiscordVerificationController : Controller
    {
        private const string PROFILE_EDITOR_PATH = "/edit-profile";
        private readonly PlayerService _playerService;
        private readonly ConfigService _configService;
        private readonly CacheService _cacheService;

        public DiscordVerificationController(PlayerService playerService, ConfigService configService, CacheService cacheService)
        {
            _playerService = playerService;
            _configService = configService;
            _cacheService = cacheService;
        }

        [Authorize]
        [HttpGet("/verify-discord")]
        public IActionResult Verify()
        {
            var steamId = User.GetSteamId();
            var token = Guid.NewGuid().ToString();

            _cacheService.Set(CacheService.CacheItemType.DiscordVerificationSessionExpiry, steamId.ToString(), DateTime.Now.AddMinutes(5));
            _cacheService.Set(CacheService.CacheItemType.DiscordVerificationSessionToken, steamId.ToString(), token);

            return Challenge(new AuthenticationProperties { RedirectUri = "/verification-complete?steamId=" + steamId + "&token=" + token }, Discord.OAuth2.DiscordDefaults.AuthenticationScheme);
        }

        [HttpGet("/verification-complete")]
        public IActionResult VerificationComplete(ulong steamId, string token)
        {
            var verificationSessionExpiry = _cacheService.Get(CacheService.CacheItemType.DiscordVerificationSessionExpiry, steamId.ToString()) as DateTime?;
            var verificationSessionToken = _cacheService.Get(CacheService.CacheItemType.DiscordVerificationSessionToken, steamId.ToString()) as string;
            if (verificationSessionExpiry != null && verificationSessionExpiry.Value > DateTime.Now && !string.IsNullOrEmpty(verificationSessionToken) && verificationSessionToken == token)
            {
                _playerService.UpdateDiscordUserId(User.GetDiscordUserId(), steamId);
                _cacheService.Remove(CacheService.CacheItemType.DiscordVerificationSessionExpiry, steamId.ToString());
            }

            HttpContext.SignOutAsync("Cookies").Wait();

            return new RedirectResult(_configService.Config.ClientUrl + PROFILE_EDITOR_PATH);
        }
    }
}