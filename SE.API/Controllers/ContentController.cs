using Microsoft.AspNetCore.Mvc;
using SE.Common.Request.Content;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("content-management")]
    [ApiController]
    public class ContentController : Controller
    {
        private readonly IContentService _contentService;

        public ContentController(IContentService contentService)
        {
            _contentService = contentService;
        }

        [HttpGet("all-music")]
        public async Task<IActionResult> GetAllMusics()
        {
            var result = await _contentService.GetAllMusics();
            return Ok(result);
        }

        [HttpPost("music")]
        public async Task<IActionResult> CreateMusic(CreateMusicRequest req)
        {
            var result = await _contentService.CreateMusic(req);
            return Ok(result);
        }        
        
        [HttpDelete("music")]
        public async Task<IActionResult> DeleteMusic([FromQuery] int musicId)
        {
            var result = await _contentService.DeleteMusic(musicId);
            return Ok(result);
        }

        [HttpPost("book")]
        public async Task<IActionResult> CreateBook(CreateBookRequest req)
        {
            var result = await _contentService.CreateBook(req);
            return Ok(result);
        }

        [HttpDelete("book")]
        public async Task<IActionResult> DeleteBook([FromQuery] int bookId)
        {
            var result = await _contentService.DeleteBook(bookId);
            return Ok(result);
        }

        [HttpPost("lesson")]
        public async Task<IActionResult> CreateLesson(CreateLessonRequest req)
        {
            var result = await _contentService.CreateLesson(req);
            return Ok(result);
        }

        [HttpDelete("lesson")]
        public async Task<IActionResult> DeleteLesson([FromQuery] int lessonId)
        {
            var result = await _contentService.DeleteLesson(lessonId);
            return Ok(result);
        }

        [HttpPost("playlist")]
        public async Task<IActionResult> CreatePlaylist(CreatePlaylistRequest req)
        {
            var result = await _contentService.CreatePlaylist(req);
            return Ok(result);
        }        
        
        [HttpPut("playlist")]
        public async Task<IActionResult> UpdatePlaylist(UpdatePlaylistRequest req)
        {
            var result = await _contentService.UpdatePlaylist(req);
            return Ok(result);
        }

        [HttpDelete("playlist")]
        public async Task<IActionResult> DeletePlaylist([FromQuery] int playlistId)
        {
            var result = await _contentService.DeletePlaylist(playlistId);
            return Ok(result);
        }

        [HttpPut("playlist/lesson")]
        public async Task<IActionResult> AddLessonToPlayList([FromQuery] int playlistId, [FromQuery] int lessonId)
        {
            var result = await _contentService.AddLessonToPlayList(playlistId, lessonId);
            return Ok(result);
        }

        [HttpPut("playlist/music")]
        public async Task<IActionResult> AddMusicToPlayList([FromQuery] int playlistId, [FromQuery] int musicId)
        {
            var result = await _contentService.AddMusicToPlayList(playlistId, musicId);
            return Ok(result);
        }
    }
}
