using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SE.Common.Enums;
using SE.Common;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using SE.Common.Request;

namespace SE.Service.Services
{
    public class EmergencyContactService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public EmergencyContactService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> CreateEmergencyContact(CreateEmergencyContactRequest request)
        {
            try
            {
                // Validate the input model
                if (request == null || request.ElderlyId <= 0 || request.AccountIds == null || request.ContactNames == null ||
                    request.AccountIds.Count != request.ContactNames.Count)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Invalid emergency contact data.");
                }

                // Create a list of EmergencyContact objects using LINQ
                var emergencyContacts = request.AccountIds
                    .Select((accountId, index) => new EmergencyContact
                    {
                        ElderlyId = request.ElderlyId,
                        AccountId = accountId,
                        ContactName = request.ContactNames[index],
                        Status = SD.GeneralStatus.ACTIVE // Assuming you want to set the status to active
                    })
                    .ToList();

                // Add all emergency contacts to the repository in one go
                await _unitOfWork.EmergencyContactRepository.CreateRangeAsync(emergencyContacts);

                return new BusinessResult(Const.SUCCESS_CREATE, "Emergency contacts created successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here)
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }
    }
}
