using AutoMapper;
using SE.Common.Enums;
using SE.Common.Request;
using SE.Common;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Common.Response.Professor;

namespace SE.Service.Services
{
    public interface IProfessorScheduleService
    {
        Task<IBusinessResult> CreateSchedule(List<ProfessorScheduleRequest> req);
    }

    public class ProfessorService : IProfessorScheduleService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProfessorService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> CreateSchedule(List<ProfessorScheduleRequest> req)
        {
            try
            {
                var professorIds = req.Select(r => r.ProfessorId).ToList();

                var existingProfessors = _unitOfWork.ProfessorRepository
                    .FindByCondition(p => professorIds.Contains(p.ProfessorId))
                    .ToList();

                if (!existingProfessors.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "NGƯỜI DÙNG KHÔNG TỒN TẠI!");
                }

                var scheduleCreateList = new List<ProfessorSchedule>();

                foreach(var scheduleReq in req)
                {
                    var schedule = _mapper.Map<ProfessorSchedule>(scheduleReq);
                    schedule.Status = SD.GeneralStatus.ACTIVE;
                    scheduleCreateList.Add(schedule);
                }

                var result = await _unitOfWork.ProfessorScheduleRepository.CreateRangeAsync(scheduleCreateList);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, req);
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllProfessor()
        {
            try
            {
                var getAllProfessor = _unitOfWork.ProfessorRepository.GetAll();
                var result = new List<GetAllProfessorReponse>();
                foreach (var item in getAllProfessor) {
                    var professor = new GetAllProfessorReponse();
                    var professorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(item.AccountId);
                    
                    
                    
                    professor.ProfessorName = professorInfor.FullName;
                    professor.ProfessorId = professorInfor.Professor.ProfessorId;
                    professor.Major = professorInfor.Professor.Knowledge;
                    professor.Rating = (decimal)professorInfor.Professor.Rating;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

    }
}
