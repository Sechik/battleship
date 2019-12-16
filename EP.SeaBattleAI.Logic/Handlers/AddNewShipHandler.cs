﻿using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using EP.SeaBattle.Data.Context;
using EP.SeaBattle.Data.Models;
using EP.SeaBattle.Logic.Commands;
using EP.SeaBattle.Logic.Models;
using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;

namespace EP.SeaBattle.Logic.Handlers
{
    public class AddNewShipHandler : IRequestHandler<AddNewShipCommand, Maybe<IEnumerable<Ship>>>
    {
        private readonly SeaBattleDbContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<AddNewShipCommand> _validator;
        private readonly ILogger _logger;

        public AddNewShipHandler(SeaBattleDbContext context, IMapper mapper, IValidator<AddNewShipCommand> validator, ILogger<AddNewShipHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Maybe<IEnumerable<Ship>>> Handle(AddNewShipCommand request, CancellationToken cancellationToken)
        {
            //TODO УЗнать как прокинуть ошибки валидации в таком случае
            var validationResult = await _validator.ValidateAsync(request, ruleSet: "AddShipValidation", cancellationToken: cancellationToken).ConfigureAwait(false);
            if (validationResult.IsValid)
            {
                var game = await _context.Games.FindAsync(request.GameId).ConfigureAwait(false);
                var player = await _context.Players.FindAsync(request.PlayerId).ConfigureAwait(false);
                var ships = await _context.Ships.Where(w => w.Game.Id == request.GameId && w.Player.Id == request.PlayerId)
                    .Include(i => i.Cells).ToArrayAsync().ConfigureAwait(false);
                ShipsManager shipsManager = new ShipsManager(_mapper.Map<Game>(game), _mapper.Map<Player>(player), 
                    _mapper.Map<IEnumerable<Ship>>(ships));
                Ship ship;
                var wasAdded = shipsManager.TryAddShip(request.X, request.Y, request.Orientation, request.Rank, out ship);
                if (wasAdded)
                {
                    await _context.Ships.AddAsync(_mapper.Map<ShipDb>(ship)).ConfigureAwait(false);
                    if (shipsManager.IsFull)
                    {
                        Player playerAI = new Player();
                        var guid = Guid.NewGuid().ToString();
                        playerAI.Id = guid;
                        playerAI.NickName = "AIBot";
                        AIManager aiManager = new AIManager(_mapper.Map<Game>(game), _mapper.Map<Player>(playerAI), 
                            Enumerable.Empty<Ship>(), Enumerable.Empty<Shot>());
                        if (aiManager.GenerateShips())
                        {
                            game.Player2 = _mapper.Map<PlayerDb>(playerAI);
                            game.Status = Common.Enums.GameStatus.Started;
                            //TODO Исправить костыль с двойным сохранением
                            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            await _context.Ships.AddRangeAsync(_mapper.Map<IEnumerable<ShipDb>>(aiManager.ShipsManager.Ships)).ConfigureAwait(false);
                        }
                        else
                        {
                            _logger.LogWarning("Can't generate AI ships for AIPlayer with ID=" + playerAI.Id);
                        }                     
                    }
                    try
                    {
                        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        return Maybe<IEnumerable<Ship>>.From(_mapper.Map<IEnumerable<Ship>>(shipsManager.Ships));
                    }
                    catch (DbUpdateException ex)
                    {
                        _logger.LogError(ex.Message);
                        return Maybe<IEnumerable<Ship>>.None;
                    }
                }
                else
                {
                    _logger.LogInformation($"Ship was not added to the field. " +
                        $"Ship info X: {request.X} Y: {request.Y}, Orientation {request.Orientation}, Rank {request.Rank}, " +
                        $"Game: {request.GameId}, Player {request.PlayerId}");
                    return Maybe<IEnumerable<Ship>>.From(_mapper.Map<IEnumerable<Ship>>(shipsManager.Ships));
                }
            }
            else
            {
                _logger.LogInformation(string.Join(", ", validationResult.Errors));
                return Maybe<IEnumerable<Ship>>.None;
            }
        }
    }
}
