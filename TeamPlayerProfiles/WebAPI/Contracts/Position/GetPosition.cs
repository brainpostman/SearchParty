﻿using Common.Models.Enums;
using System.Text.Json.Serialization;

namespace WebAPI.Contracts.Position
{
    public static class GetPosition
    {
        public sealed class Response
        {
            public int Id { get; set; }

            public PositionName Name { get; set; }
        }
    }
}
