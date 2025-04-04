using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SE.Common;
using SE.Common.DTO;
using SE.Common.Enums;
using SE.Common.Request.Report;
using SE.Common.Response.Report;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using SE.Service.Helper;
using static Azure.Core.HttpHeader;

namespace SE.Service.Services
{

    public interface IReportService
    {
        Task<IBusinessResult> CreateReport(CreateReportRequest req);
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetAllReportOfAccountId(int accountId);
        Task<IBusinessResult> UpdateStatusReport(int reportId);


    }
    public class ReportService : IReportService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReportService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<IBusinessResult> CreateReport(CreateReportRequest req)
        {
            try
            {
                var rs = _mapper.Map<SystemReport>(req);
                
                rs.CreatedAt = DateTime.UtcNow.AddHours(7);
                rs.Status = "Đang chờ xử lí";
                rs.PriorityLevel = string.Empty;

                if (req.Attachment != null)
                {
                    var image = await CloudinaryHelper.UploadImageAsync(req.Attachment);
                    rs.AttachmentUrl = image.Url;
                }

                var result = await _unitOfWork.SystemReportRepository.CreateAsync(rs);

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

        public async Task<IBusinessResult> GetAll()
        {
            try
            {
                var report = await _unitOfWork.SystemReportRepository.GetAllIncludeAccount();
                var reportDtos = _mapper.Map<List<GetAllReportResponse>>(report);
                

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, reportDtos);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }
        public async Task<IBusinessResult> GetAllReportOfAccountId(int accountId)
        {
            try
            {
                var report = _unitOfWork.SystemReportRepository.FindByCondition(r=>r.AccountId == accountId).ToList();
                var reportDtos = _mapper.Map<List<GetAllReportResponse>>(report);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, reportDtos);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateStatusReport (int reportId)
        {
            try
            {
                var report = _unitOfWork.SystemReportRepository.GetById(reportId);
                report.Status = "Đã xử lí";
                var rs = await _unitOfWork.SystemReportRepository.UpdateAsync(report);
                if (rs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
                }
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_UPDATE_MSG);

            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }
    }
}
