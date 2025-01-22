using Microsoft.AspNetCore.Mvc;
using SE.Common.Request;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("game-management")]
    [ApiController]
    public class GameController : Controller
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGame()
        {
            var result = await _gameService.GetAllGame();
            return Ok(result);
        }

        [HttpGet("{gameId}")]
        public async Task<IActionResult> GetGameById([FromRoute] int gameId)
        {
            var result = await _gameService.GetGameById(gameId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest req)
        {
            var result = await _gameService.CreateGame(req);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateGame([FromQuery] int gameId, [FromBody] CreateGameRequest req)
        {
            var result = await _gameService.UpdateGame(gameId, req);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteGame([FromQuery] int gameId)
        {
            var result = await _gameService.DeleteGame(gameId);
            return Ok(result);
        }
    }
}
