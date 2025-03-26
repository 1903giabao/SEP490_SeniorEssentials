using AutoMapper;
using SE.Common.DTO;
using SE.Common.DTO.Content;
using SE.Common.DTO.Emergency;
using SE.Common.DTO.HealthIndicator;
using SE.Common.Request;
using SE.Common.Request.Content;
using SE.Common.Request.HealthIndicator;
using SE.Common.Request.SE.Common.Request;
using SE.Common.Response.Professor;
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
        private List<string> SplitStringToList(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new List<string>();
            }

            return input.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToList();
        }
        public ApplicationMapper() 
        {
            CreateMap<UserModel, Account>().ReverseMap();
            CreateMap<UserInUserLinkDTO, Account>()
                .ReverseMap();
            CreateMap<GetUserInRoomChatDetailDTO, Account>().ReverseMap();

            CreateMap<Professor, GetProfessorDetail>()
                       .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => SplitStringToList(src.Specialization)))
                       .ForMember(dest => dest.Qualification, opt => opt.MapFrom(src => SplitStringToList(src.Qualification)))
                       .ForMember(dest => dest.Knowledge, opt => opt.MapFrom(src => SplitStringToList(src.Knowledge)))
                       .ForMember(dest => dest.Career, opt => opt.MapFrom(src => SplitStringToList(src.Career)))
                       .ForMember(dest => dest.Achievement, opt => opt.MapFrom(src => SplitStringToList(src.Achievement)));

            CreateMap<LessonModel, Lesson>().ReverseMap();
            CreateMap<CreateLessonRequest, Lesson>().ReverseMap();

            CreateMap<LessonDTO, Lesson>().ReverseMap();
            CreateMap<MusicDTO, Music>().ReverseMap();
            CreateMap<BookDTO, Book>().ReverseMap();
                
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

            CreateMap<EmergencyInformation, GetEmergencyInformationDTO>().ReverseMap();

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
