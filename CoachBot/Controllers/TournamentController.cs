﻿using CoachBot.Domain.Model;
using CoachBot.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoachBot.Controllers
{
    [Produces("application/json")]
    [Route("api/tournaments")]
    [ApiController]
    public class TournamentController : Controller
    {
        private readonly TournamentService _tournamentService;

        public TournamentController(TournamentService tournamentService)
        {
            _tournamentService = tournamentService;
        }

        public List<Tournament> GetTournaments()
        {
            return _tournamentService.GetTournaments();
        }

        [HttpGet("{id}")]
        public Tournament GetTournamentEdition(int id)
        {
            return _tournamentService.GetTournaments().First(t => t.Id == id);
        }
    }
}
