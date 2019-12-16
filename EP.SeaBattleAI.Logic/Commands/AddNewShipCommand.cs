﻿using CSharpFunctionalExtensions;
using EP.SeaBattle.Common.Enums;
using EP.SeaBattle.Logic.Models;
using MediatR;
using System.Collections.Generic;

namespace EP.SeaBattle.Logic.Commands
{
    public class AddNewShipCommand : IRequest<Maybe<IEnumerable<Ship>>>
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public ShipOrientation Orientation { get; set; }
        public ShipRank Rank { get; set; }
        public string PlayerId { get; set; }
        public string GameId { get; set; }
    }
}
