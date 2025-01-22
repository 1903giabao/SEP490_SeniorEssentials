using AutoMapper;
using Org.BouncyCastle.Ocsp;
using SE.Common.DTO;
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
    public interface ILessonService
    {
        Task<IBusinessResult> GetAllLesson();
        Task<IBusinessResult> GetLessonById(int lessonId);
        Task<IBusinessResult> CreateLesson(CreateLessonRequest req);
        Task<IBusinessResult> UpdateLesson(int lessonId, CreateLessonRequest req);
        Task<IBusinessResult> DeleteLesson(int lessonId);
    }
    public class LessonService : ILessonService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LessonService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> GetAllLesson()
        {
            try
            {
                var lessonList = await _unitOfWork.LessonRepository.GetAllAsync();

                var lessonListModel = _mapper.Map<List<LessonModel>>(lessonList);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, lessonListModel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> GetLessonById(int lessonId)
        {
            try
            {
                var lesson = _unitOfWork.LessonRepository.FindByCondition(g => g.LessonId == lessonId).FirstOrDefault();

                if (lesson == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                }

                var result = _mapper.Map<LessonModel>(lesson);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateLesson(CreateLessonRequest req)
        {
            try
            {
                var lesson = _mapper.Map<Lesson>(req);
                var result = await _unitOfWork.LessonRepository.CreateAsync(lesson);

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

        public async Task<IBusinessResult> UpdateLesson(int lessonId, CreateLessonRequest req)
        {
            try
            {
                var checkLessonExisted = _unitOfWork.LessonRepository.FindByCondition(g => g.LessonId == lessonId).FirstOrDefault();

                if (checkLessonExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                }

                checkLessonExisted.LessonName = req.LessonName;
                checkLessonExisted.LessonDescription = req.LessonDescription;

                var result = await _unitOfWork.LessonRepository.UpdateAsync(checkLessonExisted);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, req);
                }

                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> DeleteLesson(int lessonId)
        {
            try
            {
                var checkLessonExisted = _unitOfWork.LessonRepository.FindByCondition(g => g.LessonId == lessonId).FirstOrDefault();

                if (checkLessonExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                }

                checkLessonExisted.Status = SD.GeneralStatus.INACTIVE;
                var result = await _unitOfWork.LessonRepository.UpdateAsync(checkLessonExisted);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_DELETE, Const.SUCCESS_DELETE_MSG);
                }

                return new BusinessResult(Const.FAIL_DELETE, Const.FAIL_DELETE_MSG);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
