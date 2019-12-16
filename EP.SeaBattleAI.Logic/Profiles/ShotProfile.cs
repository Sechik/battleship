﻿using System;
using AutoMapper;
using EP.SeaBattle.Data.Models;
using EP.SeaBattle.Logic.Commands;
using EP.SeaBattle.Logic.Models;

namespace EP.SeaBattle.Logic.Profiles
{
    public class ShotProfile : Profile
    {
        public ShotProfile()
        {
            CreateMap<ShotDb, Shot>().ReverseMap();
            CreateMap<ShotDb, AddShotCommand>().ReverseMap();
        }
    }
}
