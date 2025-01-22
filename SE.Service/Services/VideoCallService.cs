using AutoMapper;
using SE.Common.DTO;
using SE.Common;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Common.Request;
using SE.Data.Models;
using SE.Common.Enums;

namespace SE.Service.Services
{
    public interface IVideoCallService
    {
        Task<IBusinessResult> GetAllVideoCallHistory();
        Task<IBusinessResult> GetVideoCallHistoryById(int vidCallId);
        Task<IBusinessResult> CreateVideoCall(VideoCallRequest req);
    }

    public class VideoCallService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VideoCallService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> GetAllVideoCallHistory()
        {
            try
            {
                var vidCallHistory = await _unitOfWork.VideoCallRepository.GetAllIncluding();

                var vidCallModel = _mapper.Map<List<VideoCallModel>>(vidCallHistory);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, vidCallModel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> GetVideoCallHistoryById(int vidCallId)
        {
            try
            {
                var vidCallHistory = await _unitOfWork.VideoCallRepository.GetByIdIncluding(vidCallId);

                var vidCallModel = _mapper.Map<VideoCallModel>(vidCallHistory);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, vidCallModel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateVideoCall(VideoCallRequest req)
        {
            try
            {
                var caller = _unitOfWork.AccountRepository.FindByCondition(v => v.AccountId == req.CallerId).FirstOrDefault();

                if (caller == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "NGƯỜI GỌI KHÔNG TỒN TẠI!");
                }

                var receiver = _unitOfWork.AccountRepository.FindByCondition(v => v.AccountId == req.ReceiverId).FirstOrDefault();

                if (receiver == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "NGƯỜI NHẬN KHÔNG TỒN TẠI!");
                }

                if (req.StartTime > req.EndTime)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "THỜI GIAN BẮT ĐẦU PHẢI TRƯỚC THỜI GIAN KẾT THÚC!");
                }

                var videoCall = _mapper.Map<VideoCall>(req);

                videoCall.Status = SD.GeneralStatus.ACTIVE;

                var result = await _unitOfWork.VideoCallRepository.CreateAsync(videoCall);

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
