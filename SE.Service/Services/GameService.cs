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
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SE.Service.Services
{
    public interface IGameService
    {
        Task<IBusinessResult> GetAllGame();
        Task<IBusinessResult> GetGameById(int gameId);
        Task<IBusinessResult> CreateGame(CreateGameRequest req);
        Task<IBusinessResult> UpdateGame(int gameId, UpdateGameRequest req);
        Task<IBusinessResult> DeleteGame(int gameId);
    }

    public class GameService : IGameService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GameService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> GetAllGame()
        {
            try
            {
                var gameList = await _unitOfWork.GameRepository.GetAllAsync();

                var gameListModel = _mapper.Map<List<GameModel>>(gameList);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, gameListModel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> GetGameById(int gameId)
        {
            try
            {
                var game = _unitOfWork.GameRepository.FindByCondition(g => g.GameId == gameId).FirstOrDefault();

                if (game == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                }
                
                var result = _mapper.Map<GameModel>(game);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateGame(CreateGameRequest req)
        {
            try
            {
                var checkContentProviderExisted = _unitOfWork.ContentProviderRepository.FindByCondition(e => e.ContentProviderId == req.ContentProviderId).FirstOrDefault();

                if (checkContentProviderExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "NGƯỜI DÙNG KHÔNG TỒN TẠI!");
                }

                var game = _mapper.Map<Game>(req);
                game.Status = SD.GeneralStatus.ACTIVE;
                var result = await _unitOfWork.GameRepository.CreateAsync(game);  

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

        public async Task<IBusinessResult> UpdateGame(int gameId, UpdateGameRequest req)
        {
            try
            {
                var checkGameExisted = _unitOfWork.GameRepository.FindByCondition(g => g.GameId == gameId).FirstOrDefault();

                if (checkGameExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                }

                checkGameExisted.GameName = req.GameName;
                checkGameExisted.GameDescription = req.GameDescription;

                var result = await _unitOfWork.GameRepository.UpdateAsync(checkGameExisted);

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

        public async Task<IBusinessResult> DeleteGame(int gameId)
        {
            try
            {
                var checkGameExisted = _unitOfWork.GameRepository.FindByCondition(g => g.GameId == gameId).FirstOrDefault();

                if (checkGameExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                }

                checkGameExisted.Status = SD.GeneralStatus.INACTIVE;
                var result = await _unitOfWork.GameRepository.UpdateAsync(checkGameExisted);

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
