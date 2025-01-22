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

            CreateMap<LessonModel, Lesson>().ReverseMap();
            CreateMap<CreateLessonRequest, Lesson>().ReverseMap();

            CreateMap<LessonFeedback, LessonFeedbackModel>()
                .ForMember(dest => dest.LessonName, opt => opt.MapFrom(src => src.Lesson.LessonName))
                .ForMember(dest => dest.ELderlyName, opt => opt.MapFrom(src => src.Elderly.Account.FullName))
                .ReverseMap()
                .ForPath(dest => dest.Elderly.Account.FullName, opt => opt.MapFrom(src => src.ELderlyName));
            CreateMap<LessonFeedbackRequest, LessonFeedback>().ReverseMap();

            CreateMap<VideoCall, VideoCallModel>()
                .ForMember(dest => dest.CallerName, opt => opt.MapFrom(src => src.Caller.FullName))
                .ForMember(dest => dest.ReceiverName, opt => opt.MapFrom(src => src.Receiver.FullName))
                .ReverseMap();
                
            CreateMap<CreateComboModel, Combo>().ReverseMap();
            CreateMap<CreateActivityModel, Activity>();

            CreateMap<Combo, ComboDto>();

        }
    }
}
