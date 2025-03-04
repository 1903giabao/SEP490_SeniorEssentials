using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Execution;
using CloudinaryDotNet;
using Firebase.Auth;
using Google.Cloud.Firestore;
using Org.BouncyCastle.Ocsp;
using SE.Common;
using SE.Common.DTO;
using SE.Common.Enums;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using SE.Service.Helper;

namespace SE.Service.Services
{
    public interface IAccountService
    {
        Task<IBusinessResult> CreateNewTempAccount(CreateNewAccountDTO req);
        Task<IBusinessResult> GetAllUsers();
        Task<IBusinessResult> GetUserById(int id);
        Task<IBusinessResult> GetUserByPhoneNumber(string phoneNumber, int userId);
    }

    public class AccountService : IAccountService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly FirestoreDb _firestoreDb;

        public AccountService(UnitOfWork unitOfWork, IMapper mapper, FirestoreDb firestoreDb)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firestoreDb = firestoreDb;
        }

        public async Task<IBusinessResult> CreateNewTempAccount(CreateNewAccountDTO req)
        {
            try
            {
                var existedAcc = _unitOfWork.AccountRepository.FindByCondition(e => e.Email.Equals(req.Account)).FirstOrDefault();

                int rs;
                var newAccount = new Data.Models.Account();
                if (existedAcc != null)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, "Email already existed!", existedAcc);
                }

                if (FunctionCommon.IsValidPhoneNumber(req.Account))
                {
                    newAccount = new Data.Models.Account
                    {
                        RoleId = req.RoleId,
                        PhoneNumber = req.Account,
                        Otp = req.OTP,
                        Password = SecurityUtil.Hash(req.Password),
                        IsVerified = false
                    };
                    rs = await _unitOfWork.AccountRepository.CreateAsync(newAccount);
                }
                else
                {
                    newAccount = new Data.Models.Account
                    {
                        RoleId = req.RoleId,
                        Email = req.Account,
                        Otp = req.OTP,
                        Password = SecurityUtil.Hash(req.Password),
                        IsVerified = false
                    };
                    rs = await _unitOfWork.AccountRepository.CreateAsync(newAccount);
                }


                if (rs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, newAccount);
                }
                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, newAccount);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllUsers()
        {
            var users = _unitOfWork.AccountRepository.GetAll();

            var rs = _mapper.Map<List<UserDTO>>(users);

            CollectionReference onlineRef = _firestoreDb.Collection("OnlineMembers");

            foreach (var user in rs)
            {
                DocumentSnapshot onlineSnapshot = await onlineRef.Document(user.AccountId.ToString()).GetSnapshotAsync();

                var onlineData = onlineSnapshot.ToDictionary();

                if (onlineData != null)
                {
                    user.IsOnline = onlineData.ContainsKey("IsOnline") && (bool)onlineData["IsOnline"];
                }
                else
                {
                    user.IsOnline = false;
                }

            }

            return new BusinessResult (Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);
        }

        public async Task<IBusinessResult> GetUserById(int id)
        {
            try
            {
                var user = await _unitOfWork.AccountRepository.GetByIdAsync(id);

                if (user == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                }

                DocumentReference onlineRef = _firestoreDb.Collection("OnlineMembers").Document(id.ToString());

                DocumentSnapshot onlineSnapshot = await onlineRef.GetSnapshotAsync();

                var onlineData = onlineSnapshot.ToDictionary();

                var rs = _mapper.Map<UserDTO>(user);

                rs.IsOnline = onlineData.ContainsKey("IsOnline") && (bool)onlineData["IsOnline"];

                return new BusinessResult (Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetUserByPhoneNumber(string phoneNumber, int userId)
        {
            try
            {
                if (!FunctionCommon.IsValidPhoneNumber(phoneNumber))
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Wrong phone number format!");
                }

                var userPhone = await _unitOfWork.AccountRepository.GetByPhoneNumberAsync(phoneNumber);

                if (userPhone == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                }

                if (userPhone.AccountId == userId)
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, new { isMe = true });
                }

                var userLink = await _unitOfWork.UserLinkRepository.GetByUserIdsAsync(userId, userPhone.AccountId);

                if (userLink != null)
                {
                    if (userLink.RelationshipType.Equals("Friend") && userLink.Status.Equals(SD.UserLinkStatus.ACCEPTED))
                    {
                        return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, new { isFriend = true });
                    }

                    if (userLink.RelationshipType.Equals("Family") && userLink.Status.Equals(SD.UserLinkStatus.ACCEPTED))
                    {
                        return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, new { isFamily = true });
                    }

                    if (userLink.Status.Equals(SD.UserLinkStatus.PENDING))
                    {
                        var result = _mapper.Map<GetUserPhoneNumberDTO>(userPhone);
                        result.RequestUserId = userLink.AccountId1;
                        
                        return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
                    }
                }

                var rs = _mapper.Map<UserDTO>(userPhone);


                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
