using AutoMapper;
using SE.Common.DTO;
using SE.Common.Request;
using SE.Common.Request.SE.Common.Request;
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
            CreateMap<UserModel, Account>().ReverseMap();

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
            CreateMap<CreateActivityModel, Activity>().ReverseMap();

            CreateMap<Combo, ComboDto>().ReverseMap();


            CreateMap<GetEmergencyContactDTO,EmergencyContact>().ReverseMap();
            CreateMap<CreateFamilyTieRequest, FamilyTie>()
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active"));

            CreateMap<FamilyTie, FamilyTieDTO>();
            CreateMap<CreateFamilyTieRequest, FamilyTie>()
                        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active")).ReverseMap();

            CreateMap<CreateGroupRequest, Group>().ReverseMap();

            CreateMap<GroupMember, GroupMemberDTO>()
                       .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group.GroupName))
                       .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Account.FullName))
                       .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Account.Avatar))
                       .ReverseMap();

            CreateMap<CreateHealthIndicatorRequest, HealthIndicator>()
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status ?? "Active"));

            CreateMap<HealthIndicator, CreateHealthIndicatorRequest>().ReverseMap();
            CreateMap<HealthIndicator, UpdateHealthIndicatorRequest>().ReverseMap();

            CreateMap<MedicationModel, Medication>();
            CreateMap<CreateMedicationRequest, Medication>();
            CreateMap<UpdateMedicationRequest, Medication>();

            CreateMap<CreateIotDeviceRequest, Iotdevice>();
            CreateMap<IotDeviceDto, Iotdevice>();

            CreateMap<Combo, ComboDto>();

            CreateMap<Account, UserDTO>().ReverseMap();
        }
    }
}
