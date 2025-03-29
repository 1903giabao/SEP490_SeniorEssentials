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

        [HttpGet("all-content")]
        public async Task<IActionResult> GetAllContents()
        {
            var result = await _contentService.GetAllContents();
            return Ok(result);
        }         
        
        [HttpGet("all-music/{playlistId}")]
        public async Task<IActionResult> GetAllMusics(int playlistId)
        {
            var result = await _contentService.GetAllMusics(playlistId);
            return Ok(result);
        }           
        
        [HttpGet("all-lesson/{playlistId}")]
        public async Task<IActionResult> GetAllLessons(int playlistId)
        {
            var result = await _contentService.GetAllLessons(playlistId);
            return Ok(result);
        }        
        
        [HttpGet("all-book")]
        public async Task<IActionResult> GetAllBooks()
        {
            var result = await _contentService.GetAllBooks();
            return Ok(result);
        }

        [HttpGet("all-lesson-playlist")]
        public async Task<IActionResult> GetAllLessonPlaylist()
        {
            var result = await _contentService.GetAllLessonPlaylist();
            return Ok(result);
        }

        [HttpGet("all-music-playlist")]
        public async Task<IActionResult> GetAllMusicPlaylist()
        {
            var result = await _contentService.GetAllMusicPlaylist();
            return Ok(result);
        }

        [HttpPost("music")]
        public async Task<IActionResult> CreateMusic([FromForm] CreateMusicRequest req)
        {
            var result = await _contentService.CreateMusic(req);
            return Ok(result);
        }          
        
        [HttpPut("music-status")]
        public async Task<IActionResult> ChangeMusicStatus([FromQuery] int musicId, [FromQuery] string status)
        {
            var result = await _contentService.ChangeMusicStatus(musicId, status);
            return Ok(result);
        }        
        
        [HttpDelete("music-by-admin")]
        public async Task<IActionResult> DeleteMusicByAdmin([FromQuery] int musicId)
        {
            var result = await _contentService.DeleteMusicByAdmin(musicId);
            return Ok(result);
        }

        [HttpPost("book")]
        public async Task<IActionResult> CreateBook(CreateBookRequest req)
        {
            var result = await _contentService.CreateBook(req);
            return Ok(result);
        }

        [HttpPut("book")]
        public async Task<IActionResult> UpdateBook([FromBody] UpdateBookRequest req)
        {
            var result = await _contentService.UpdateBook(req);
            return Ok(result);
        }        
        
        [HttpPut("book-status")]
        public async Task<IActionResult> ChangeBookStatus([FromQuery] int bookId, [FromQuery] string status)
        {
            var result = await _contentService.ChangeBookStatus(bookId, status);
            return Ok(result);
        }

        [HttpDelete("book-by-admin")]
        public async Task<IActionResult> DeleteBookByAdmin([FromQuery] int bookId)
        {
            var result = await _contentService.DeleteBookByAdmin(bookId);
            return Ok(result);
        }

        [HttpPost("lesson")]
        public async Task<IActionResult> CreateLesson([FromForm] CreateLessonRequest req)
        {
            var result = await _contentService.CreateLesson(req);
            return Ok(result);
        }

        [HttpPut("lesson-status")]
        public async Task<IActionResult> ChangeLessonStatus([FromQuery] int lessonId, [FromQuery] string status)
        {
            var result = await _contentService.ChangeLessonStatus(lessonId, status);
            return Ok(result);
        }

        [HttpDelete("lesson-by-admin")]
        public async Task<IActionResult> DeleteLessonByAdmin([FromQuery] int lessonId)
        {
            var result = await _contentService.DeleteLessonByAdmin(lessonId);
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

        [HttpPut("playlist-status")]
        public async Task<IActionResult> ChangePlaylistStatus([FromQuery] int playlistId, [FromQuery] string status)
        {
            var result = await _contentService.ChangePlaylistStatus(playlistId, status);
            return Ok(result);
        }

        [HttpDelete("playlist-by-admin")]
        public async Task<IActionResult> DeletePlaylistByAdmin([FromQuery] int playlistId)
        {
            var result = await _contentService.DeletePlaylistByAdmin(playlistId);
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
