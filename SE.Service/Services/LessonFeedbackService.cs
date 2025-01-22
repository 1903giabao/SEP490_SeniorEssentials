using AutoMapper;
using SE.Common;
using SE.Common.DTO;
using SE.Common.Enums;
using SE.Common.Request;
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
    public interface ILessonFeedbackService
    {
        Task<IBusinessResult> Feedback(LessonFeedbackRequest req);
        Task<IBusinessResult> GetAllFeedbackByLessonId(int lessonId);
    }

    public class LessonFeedbackService : ILessonFeedbackService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LessonFeedbackService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> GetAllFeedbackByLessonId(int lessonId)
        {
            try
            {
                var lesson = _unitOfWork.LessonRepository.FindByCondition(g => g.LessonId == lessonId).FirstOrDefault();

                if (lesson == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "BÀI HỌC KHÔNG TỒN TẠI!");
                }

                var feedbackList = await _unitOfWork.LessonFeedbackRepository.GetByLessonId(lessonId);

                var result = _mapper.Map<List<LessonFeedbackModel>>(feedbackList);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> Feedback(LessonFeedbackRequest req)
        {
            try
            {
                var checkElderlyExisted = _unitOfWork.ElderlyRepository.FindByCondition(e => e.ElderlyId== req.ElderlyId).FirstOrDefault();

                if (checkElderlyExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "NGƯỜI DÙNG KHÔNG TỒN TẠI!");
                }

                var checkLessonExisted = _unitOfWork.LessonRepository.FindByCondition(l => l.LessonId == req.LessonId).FirstOrDefault();

                if (checkLessonExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "BÀI HỌC KHÔNG TỒN TẠI!");
                }

                var feedback = _mapper.Map<LessonFeedback>(req);
                feedback.Status = SD.GeneralStatus.ACTIVE;

                var result = await _unitOfWork.LessonFeedbackRepository.CreateAsync(feedback);

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
