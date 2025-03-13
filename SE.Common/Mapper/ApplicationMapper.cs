using AutoMapper;
using SE.Common.DTO;
using SE.Common.DTO.HealthIndicator;
using SE.Common.Request;
using SE.Common.Request.HealthIndicator;
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
            CreateMap<UserInUserLinkDTO, Account>()
                .ReverseMap();
            CreateMap<GetUserInRoomChatDetailDTO, Account>().ReverseMap();


            CreateMap<LessonModel, Lesson>().ReverseMap();
            CreateMap<CreateLessonRequest, Lesson>().ReverseMap();

            CreateMap<LessonFeedback, LessonFeedbackModel>()
                .ForMember(dest => dest.LessonName, opt => opt.MapFrom(src => src.Lesson.LessonName))
                .ForMember(dest => dest.ELderlyName, opt => opt.MapFrom(src => src.Elderly.Account.FullName))
                .ReverseMap()
                .ForPath(dest => dest.Elderly.Account.FullName, opt => opt.MapFrom(src => src.ELderlyName));
            CreateMap<LessonFeedbackRequest, LessonFeedback>().ReverseMap();
                
            CreateMap<CreateComboModel, Subscription>().ReverseMap();
            CreateMap<CreateActivityModel, Activity>().ReverseMap();

            CreateMap<Subscription, ComboDto>().ReverseMap();
            
            CreateMap<CreateGroupRequest, Group>().ReverseMap();

            CreateMap<GroupMember, GroupMemberDTO>()
                       .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group.GroupName))
                       .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Account.FullName))
                       .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Account.Avatar))
                       .ReverseMap();

            CreateMap<MedicationModel, Medication>();
            CreateMap<CreateMedicationRequest, Medication>();
            CreateMap<UpdateMedicationRequest, Medication>();

            CreateMap<CreateIotDeviceRequest, Iotdevice>();
            CreateMap<IotDeviceDto, Iotdevice>();

            CreateMap<Subscription, ComboDto>();

            CreateMap<Account, UserDTO>().ReverseMap();
            CreateMap<Account, GetUserPhoneNumberDTO>().ReverseMap();

            CreateMap<BloodPressure, CreateBloodPressureRequest>().ReverseMap();
            CreateMap<BloodGlucose, CreateBloodGlucoseRequest>().ReverseMap();
            CreateMap<Weight, CreateWeightRequest>().ReverseMap();
            CreateMap<Height, CreateHeightRequest>().ReverseMap();
            CreateMap<HeartRate, CreateHeartRateRequest>().ReverseMap();
            CreateMap<LipidProfile, CreateLipidProfileRequest>().ReverseMap();
            CreateMap<LiverEnzyme, CreateLiverEnzymesRequest> ().ReverseMap();
            CreateMap<KidneyFunction, CreateKidneyFunctionRequest> ().ReverseMap();

            CreateMap<BloodPressure, GetBloodPressureDTO>().ReverseMap();
            CreateMap<BloodGlucose, GetBloodGlucoseDTO>().ReverseMap();
            CreateMap<Weight, GetWeightDTO>().ReverseMap();
            CreateMap<Height, GetHeightDTO>().ReverseMap();
            CreateMap<HeartRate, GetHeartRateDTO>().ReverseMap();
            CreateMap<LipidProfile, GetLipidProfileDTO>().ReverseMap();
            CreateMap<LiverEnzyme, GetLiverEnzymesDTO>().ReverseMap();
            CreateMap<KidneyFunction, GetKidneyFunctionDTO>().ReverseMap();
        }
    }
}
