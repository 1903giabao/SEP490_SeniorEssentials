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

namespace SE.Service.Services
{
    public interface ILessonHistoryService
    {

    }
    public class LessonHistoryService : ILessonHistoryService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LessonHistoryService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> CreateLessonHistory(LessonHistoryRequest req)
        {
            try
            {
                var checkElderlyExisted = _unitOfWork.ElderlyRepository.FindByCondition(e => e.ElderlyId == req.ElderlyId).FirstOrDefault();

                if (checkElderlyExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "NGƯỜI DÙNG KHÔNG TỒN TẠI!");
                }

                var checkLessonExisted = _unitOfWork.LessonRepository.FindByCondition(l => l.LessonId == req.LessonId).FirstOrDefault();

                if (checkLessonExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "BÀI HỌC KHÔNG TỒN TẠI!");
                }

                var lessonHistory = _mapper.Map<LessonHistory>(req);
                lessonHistory.Status = SD.GeneralStatus.ACTIVE;

                var result = await _unitOfWork.LessonHistoryRepository.CreateAsync(lessonHistory);

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
    }
}
