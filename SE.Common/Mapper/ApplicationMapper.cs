using AutoMapper;
using SE.Common.DTO;
using SE.Common.Request;
using SE.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Mapper
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper() 
        {
            CreateMap<GameModel, Game>().ReverseMap();
            CreateMap<CreateGameRequest, Game>().ReverseMap();

            CreateMap<CreateComboModel, Combo>().ReverseMap();
            CreateMap<CreateActivityModel, Activity>();

            CreateMap<Combo, ComboDto>();

        }
    }
}
