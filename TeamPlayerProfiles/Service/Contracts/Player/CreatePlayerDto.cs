﻿using Service.Contracts.Hero;
using EnumPosition = Common.Models.Enums.Position;

namespace Service.Contracts.Player
{
    public class CreatePlayerDto
    {
        public Guid UserId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public EnumPosition Position { get; set; }

        public ICollection<HeroDto> Heroes { get; set; }
    }
}
