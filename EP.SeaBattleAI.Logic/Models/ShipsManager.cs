﻿using EP.SeaBattle.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EP.SeaBattle.Logic.Models
{
    public class ShipsManager
    {
        private Dictionary<ShipRank, byte> _shipsCount;
        private readonly Dictionary<ShipRank, byte> shipsRuleCount = new Dictionary<ShipRank, byte>
        {
            { ShipRank.One, SHIP_RANK_ONE_MAX_COUNT },
            { ShipRank.Two, SHIP_RANK_TWO_MAX_COUNT },
            { ShipRank.Three, SHIP_RANK_THREE_MAX_COUNT },
            { ShipRank.Four, SHIP_RANK_FOUR_MAX_COUNT },
        };
        
        private const byte SHIP_RANK_FOUR_MAX_COUNT = 1;
        private const byte SHIP_RANK_THREE_MAX_COUNT = 2;
        private const byte SHIP_RANK_TWO_MAX_COUNT = 3;
        private const byte SHIP_RANK_ONE_MAX_COUNT = 4;

        public const byte MAX_SHIPS_COUNT = 10;

        List<Ship> _ships;
        readonly Game Game;
        readonly Player Player;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="ships">Collection of ships</param>
        public ShipsManager(Game game, Player player, IEnumerable<Ship> ships)
        {
            Game = game;
            Player = player;
            _shipsCount = new Dictionary<ShipRank, byte>(4)
            {
                { ShipRank.One, 0 },
                { ShipRank.Two, 0 },
                { ShipRank.Three, 0 },
                { ShipRank.Four, 0 }
            };

            foreach (var ship in ships)
            {
                _shipsCount[ship.Rank] += 1;
            }
            _ships = new List<Ship>(ships);
        }

        public ICollection<Ship> Ships { get => _ships; }

        /// <summary>
        /// Returns true if all ships are set
        /// </summary>
        public bool IsFull
        {
            get
            {
                if (_ships.Count >= MAX_SHIPS_COUNT)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Inform is all ships destroyed
        /// </summary>
        public bool AllShipsDestroyed { get => !_ships.Any(a => a.IsAlive); }

        /// <summary>
        /// Add ship
        /// </summary>
        /// <param name="ship">Ship</param>
        private bool AddShip(Ship ship)
        {
            //TODO Throw message if cannot add ship
            var rank = ship.Rank;
            if (_shipsCount[rank] < shipsRuleCount[rank])
            {
                _ships.Add(ship);
                _shipsCount[rank] += 1;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add ship
        /// </summary>
        /// <param name="x">x-coordinate of ship start cell</param>
        /// <param name="y">y-coordinate of ship start cell</param>
        /// <param name="shipOrientation">Orientation</param>
        /// <param name="rank">Rank</param>
        /// <returns></returns>
        public bool TryAddShip(byte x, byte y, ShipOrientation shipOrientation, ShipRank rank, out Ship _ship)
        {
            FieldManager fieldManager = new FieldManager(_ships);
            if (fieldManager.AddShip(x, y, shipOrientation, rank))
            {
                var ship = new Ship(Game, Player, GenerateCell(x, y, shipOrientation, rank));
                _ship = ship;
                return AddShip(ship);
            }
            _ship = null;
            return false;
        }

        /// <summary>
        /// Delete ship
        /// </summary>
        /// <param name="ship">Ship</param>
        private bool DeleteShip(Ship ship)
        {
            //TODO Throw message if ship not found
            return _ships.Remove(ship);
        }

        /// <summary>
        /// Generate cell for ship
        /// </summary>
        /// <param name="x">x-coordinate of ship start point</param>
        /// <param name="y">y-coordinate of ship start point</param>
        /// <param name="shipOrientation">Orientation</param>
        /// <param name="rank">Rank</param>
        /// <returns></returns>
        private IEnumerable<Cell> GenerateCell(byte x, byte y, ShipOrientation shipOrientation, ShipRank rank)
        {
            List<Cell> cells = new List<Cell>((byte)rank);
            for (byte i = 0; i < (byte)rank; i++)
            {
                if (shipOrientation == ShipOrientation.Horizontal)
                {
                    var cell = new Cell(Convert.ToByte(x + i), y, Common.Enums.CellStatus.Alive);
                    cells.Add(cell);
                }
                else
                {
                    var cell = new Cell(x, Convert.ToByte(y + i), Common.Enums.CellStatus.Alive);
                    cells.Add(cell);
                }
            }
            return cells;
        }
    }
}
