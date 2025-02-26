using AutoMapper;
using Google.Cloud.Firestore;
using SE.Common.DTO;
using SE.Common;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using SE.Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Models;
using SE.Common.Request;
using SE.Common.Enums;
using Org.BouncyCastle.Ocsp;

namespace SE.Service.Services
{
    public interface IUserLinkService
    {
        Task<IBusinessResult> SendAddFriend(SendAddFriendRequest req);
        Task<IBusinessResult> ResponseAddFriend(ResponseAddFriendRequest req);
        Task<IBusinessResult> GetAllByRequestUserId(int requestUserId);
        Task<IBusinessResult> GetAllByResponseUserId(int responseUserId);
    }

    public class UserLinkService : IUserLinkService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserLinkService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> SendAddFriend(SendAddFriendRequest req)
        {
            try
            {
                var requestUser = await _unitOfWork.AccountRepository.GetByIdAsync(req.RequestUserId);

                if (requestUser == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Request user does not exist!");
                }

                if (requestUser.RoleId != 2 && requestUser.RoleId != 3)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid role!");
                }                
                
                var responseUser = await _unitOfWork.AccountRepository.GetByIdAsync(req.ResponseUserId);

                if (responseUser == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Request user does not exist!");
                }

                if (responseUser.RoleId != 2 && responseUser.RoleId != 3)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid role!");
                }

                var userLink = new UserLink
                {
                    AccountId1 = req.RequestUserId,
                    AccountId2 = req.ResponseUserId,
                    RelationshipType = req.RelationshipType,
                    CreatedAt = DateTime.UtcNow.AddHours(7),
                    UpdatedAt = DateTime.UtcNow.AddHours(7),
                    Status = SD.UserLinkStatus.PENDING,
                };
                var createUserLink = await _unitOfWork.UserLinkRepository.CreateAsync(userLink);

                if (createUserLink > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, "Add friend request sent.");
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> ResponseAddFriend(ResponseAddFriendRequest req)
        {
            try
            {
/*                var responseUser = await _unitOfWork.AccountRepository.GetByIdAsync(req.ResponseUserId);

                if (responseUser == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Request user does not exist!");
                }

                if (responseUser.RoleId != 2 || responseUser.RoleId != 3)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid role!");
                }*/

                var userLink = await _unitOfWork.UserLinkRepository.GetByIdAsync(req.UserLinkId);

                if (userLink == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Request does not exist!");
                }

                if (!userLink.Status.Equals(SD.UserLinkStatus.PENDING))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid status!");
                }                

                if (req.ResponseStatus.Equals(SD.UserLinkStatus.CANCELLED, StringComparison.OrdinalIgnoreCase))
                {
                    userLink.Status = SD.UserLinkStatus.CANCELLED;
                }
                else if (req.ResponseStatus.Equals(SD.UserLinkStatus.REJECTED, StringComparison.OrdinalIgnoreCase))
                {
                    userLink.Status = SD.UserLinkStatus.REJECTED;
                }

                else if (req.ResponseStatus.Equals(SD.UserLinkStatus.ACCEPTED, StringComparison.OrdinalIgnoreCase))
                {
                    userLink.Status = SD.UserLinkStatus.ACCEPTED;
                }
                else
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Status must be CANCELLED, REJECTED, ACCEPTED!");
                }


                var updateUserLink = await _unitOfWork.UserLinkRepository.UpdateAsync(userLink);

                if (updateUserLink > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, $"Add friend request is {userLink.Status}.");
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllByRequestUserId(int requestUserId)
        {
            try
            {
                var requestUser = _unitOfWork.UserLinkRepository.GetAll().Where(u => u.AccountId1 == requestUserId).FirstOrDefault();

                if (requestUser == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Cannot find request of user!");
                }

                var userLink = await _unitOfWork.UserLinkRepository.GetByAccount1Async(requestUserId);

                var result = new UserLinkDTO
                {
                    RequestUserId = userLink.AccountId1,
                    RequestUserName = userLink.AccountId1Navigation.FullName,
                    RequestUserAvatar = userLink.AccountId1Navigation.Avatar,
                    ResponseUserId = userLink.AccountId2,
                    ResponseUserName = userLink.AccountId2Navigation.FullName,
                    ResponseUserAvatar = userLink.AccountId2Navigation.Avatar,
                    CreatedAt = (DateTime)userLink.CreatedAt,
                    Status = userLink.Status,
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllByResponseUserId(int responseUserId)
        {
            try
            {
                var requestUser = _unitOfWork.UserLinkRepository.GetAll().Where(u => u.AccountId2 == responseUserId).FirstOrDefault();

                if (requestUser == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Cannot find request of user!");
                }

                var userLink = await _unitOfWork.UserLinkRepository.GetByAccount2Async(responseUserId);

                var result = new UserLinkDTO
                {
                    RequestUserId = userLink.AccountId1,
                    RequestUserName = userLink.AccountId1Navigation.FullName,
                    RequestUserAvatar = userLink.AccountId1Navigation.Avatar,
                    ResponseUserId = userLink.AccountId2,
                    ResponseUserName = userLink.AccountId2Navigation.FullName,
                    ResponseUserAvatar = userLink.AccountId2Navigation.Avatar,
                    CreatedAt = (DateTime)userLink.CreatedAt,
                    Status = userLink.Status,
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllFamilyMember(int userId)
        {
            try
            {
                var requestUser = _unitOfWork.UserLinkRepository.GetAll().Where(u => u.AccountId2 == userId).FirstOrDefault();

                if (requestUser == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Cannot find request of user!");
                }

                var userLink = await _unitOfWork.UserLinkRepository.GetByAccount2Async(userId);

                var result = new UserLinkDTO
                {
                    RequestUserId = userLink.AccountId1,
                    RequestUserName = userLink.AccountId1Navigation.FullName,
                    RequestUserAvatar = userLink.AccountId1Navigation.Avatar,
                    ResponseUserId = userLink.AccountId2,
                    ResponseUserName = userLink.AccountId2Navigation.FullName,
                    ResponseUserAvatar = userLink.AccountId2Navigation.Avatar,
                    CreatedAt = (DateTime)userLink.CreatedAt,
                    Status = userLink.Status,
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
