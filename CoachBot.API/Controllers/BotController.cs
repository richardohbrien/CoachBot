﻿using AspNetCore.Proxy;
using CoachBot.Domain.Model;
using CoachBot.Model;
using CoachBot.Services.Matchmaker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static CoachBot.Attributes.HubRoleAuthorizeAttribute;

namespace CoachBot.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize]
    public class BotController : Controller
    {
        private readonly Config _config;

        public BotController(Config config)
        {
            _config = config;
        }

        [HubRolePermission(HubRole = PlayerHubRole.Administrator)]
        [HttpGet("state")]
        public Task Get()
        {
            var url = $"{this.Request.Scheme}://{this.Request.Host.Host}:{_config.BotApiPort}/api/bot/state";
            return this.ProxyAsync($"{this.Request.Scheme}://{this.Request.Host.Host}:{_config.BotApiPort}/api/bot/state");
        }

        [HubRolePermission(HubRole = PlayerHubRole.Administrator)]
        [HttpPost("reconnect")]
        public Task Reconnect()
        {
            return this.ProxyAsync($"{this.Request.Scheme}://{this.Request.Host.Host}:{_config.BotApiPort}/api/bot/reconnect");
        }
    }
}
