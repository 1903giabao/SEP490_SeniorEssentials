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
using SE.Data.Models;
using SE.Common.Enums;
using SE.Service.Helper;
using SE.Common.Request.Content;
using Org.BouncyCastle.Ocsp;
using SE.Common.DTO.Content;

namespace SE.Service.Services
{
    public interface IContentService
    {
        Task<IBusinessResult> GetAllMusics();
        Task<IBusinessResult> CreateMusic(CreateMusicRequest req);
        Task<IBusinessResult> DeleteMusic(int musicId);
        Task<IBusinessResult> CreateBook(CreateBookRequest req);
        Task<IBusinessResult> DeleteBook(int bookId);
        Task<IBusinessResult> CreateLesson(CreateLessonRequest req);
        Task<IBusinessResult> DeleteLesson(int lessonId);
        Task<IBusinessResult> CreatePlaylist(CreatePlaylistRequest req);
        Task<IBusinessResult> UpdatePlaylist(UpdatePlaylistRequest req);
        Task<IBusinessResult> DeletePlaylist(int playlistId);
        Task<IBusinessResult> AddLessonToPlayList(int lessonId, int playlistId);
        Task<IBusinessResult> AddMusicToPlayList(int musicId, int playlistId);

    }

    public class ContentService : IContentService
    {

        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ContentService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> GetAllMusics()
        {
            try
            {
                var musics = await _unitOfWork.MusicRepository.GetAllAsync();

                var rs = _mapper.Map<List<MusicDTO>>(musics);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateMusic(CreateMusicRequest req)
        {
            try
            {
                var user = await _unitOfWork.AccountRepository.GetByIdAsync(req.AccountId);

                if (user == null || user.RoleId != 5)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                }

                var musicURL = ("", "");

                if (req.MusicFile != null)
                {
                    musicURL = await CloudinaryHelper.UploadImageAsync(req.MusicFile);
                }

                var music = new Music
                {
                    AccountId = req.AccountId,
                    MusicName = req.MusicName,
                    MusicUrl = musicURL.Item2,
                    Singer = req.Singer,
                    CreatedDate = DateTime.UtcNow.AddHours(7),
                    Status = SD.GeneralStatus.ACTIVE,
                };

                var createRs = await _unitOfWork.MusicRepository.CreateAsync(music);

                if (createRs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> DeleteMusic(int musicId)
        {
            try
            {
                var music = await _unitOfWork.MusicRepository.GetByIdAsync(musicId);

                if (music == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Music does not exist!");
                }

                var deleteRs = await _unitOfWork.MusicRepository.RemoveAsync(music);

                if (deleteRs)
                {
                    return new BusinessResult(Const.SUCCESS_DELETE, Const.SUCCESS_DELETE_MSG);
                }

                return new BusinessResult(Const.FAIL_DELETE, Const.FAIL_DELETE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateBook(CreateBookRequest req)
        {
            try
            {
                var user = await _unitOfWork.AccountRepository.GetByIdAsync(req.AccountId);

                if (user == null || user.RoleId != 5)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                }

                var bookURL = ("", "");

                if (req.BookFile != null)
                {
                    bookURL = await CloudinaryHelper.UploadImageAsync(req.BookFile);
                }

                var book = new Book
                {
                    AccountId = req.AccountId,
                    BookName = req.BookName,
                    BookUrl = bookURL.Item2,
                    BookType = req.BookType,
                    PublishDate = req.PublishDate,
                    Author = req.Author,
                    CreatedDate = DateTime.UtcNow.AddHours(7),
                    Status = SD.GeneralStatus.ACTIVE,
                };

                var createRs = await _unitOfWork.BookRepository.CreateAsync(book);

                if (createRs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> DeleteBook(int bookId)
        {
            try
            {
                var book = await _unitOfWork.BookRepository.GetByIdAsync(bookId);

                if (book == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Book does not exist!");
                }

                var deleteRs = await _unitOfWork.BookRepository.RemoveAsync(book);

                if (deleteRs)
                {
                    return new BusinessResult(Const.SUCCESS_DELETE, Const.SUCCESS_DELETE_MSG);
                }

                return new BusinessResult(Const.FAIL_DELETE, Const.FAIL_DELETE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateLesson(CreateLessonRequest req)
        {
            try
            {
                var user = await _unitOfWork.AccountRepository.GetByIdAsync(req.AccountId);

                if (user == null || user.RoleId != 5)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                }

                var playlistExist = await _unitOfWork.PlaylistRepository.GetByIdAsync(req.PlaylistId);

                if (playlistExist == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Playlist does not exist!");
                }

                var lessonURL = ("", "");

                if (req.LessonFile != null)
                {
                    lessonURL = await CloudinaryHelper.UploadImageAsync(req.LessonFile);
                }

                var lesson = new Lesson
                {
                    AccountId = req.AccountId,
                    PlaylistId = playlistExist.PlaylistId,
                    LessonName = req.LessonName,
                    LessonUrl = lessonURL.Item2,
                    CreatedDate = DateTime.UtcNow.AddHours(7),
                    Status = SD.GeneralStatus.ACTIVE,
                };

                var createRs = await _unitOfWork.LessonRepository.CreateAsync(lesson);

                if (createRs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
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
                var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(lessonId);

                if (lesson == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Lesson does not exist!");
                }

                var deleteRs = await _unitOfWork.LessonRepository.RemoveAsync(lesson);

                if (deleteRs)
                {
                    return new BusinessResult(Const.SUCCESS_DELETE, Const.SUCCESS_DELETE_MSG);
                }

                return new BusinessResult(Const.FAIL_DELETE, Const.FAIL_DELETE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreatePlaylist(CreatePlaylistRequest req)
        {
            try
            {
                var user = await _unitOfWork.AccountRepository.GetByIdAsync(req.AccountId);

                if (user == null || user.RoleId != 5)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                }

                var playlist = new Playlist
                {
                    AccountId = req.AccountId,
                    PlaylistName = req.PlaylistName,
                    CreatedDate = DateTime.UtcNow.AddHours(7),
                    Status = SD.GeneralStatus.ACTIVE,
                };

                var createRs = await _unitOfWork.PlaylistRepository.CreateAsync(playlist);

                if (createRs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdatePlaylist(UpdatePlaylistRequest req)
        {
            try
            {
                var playlistExist = await _unitOfWork.PlaylistRepository.GetByIdAsync(req.PlaylistId);

                if (playlistExist == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Playlist does not exist!");
                }

                playlistExist.PlaylistName = req.PlaylistName;

                var updateRs = await _unitOfWork.PlaylistRepository.UpdateAsync(playlistExist);

                if (updateRs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, playlistExist);
                }

                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> DeletePlaylist(int playlistId)
        {
            try
            {
                var playlistExist = await _unitOfWork.PlaylistRepository.GetByIdAsync(playlistId);

                if (playlistExist == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Playlist does not exist!");
                }

                var listLesson = await _unitOfWork.LessonRepository.GetLessonsByPlaylist(playlistId);

                foreach (var lesson in listLesson)
                {
                    lesson.Playlist = null;
                    lesson.PlaylistId = null;
                    var updateRs = await _unitOfWork.LessonRepository.UpdateAsync(lesson);

                    if (updateRs < 1)
                    {
                        return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                    }
                }

                var listMusic = await _unitOfWork.MusicRepository.GetMusicsByPlaylist(playlistId);

                foreach (var music in listMusic)
                {
                    music.Playlist = null;
                    music.PlaylistId = null;
                    var updateRs = await _unitOfWork.MusicRepository.UpdateAsync(music);

                    if (updateRs < 1)
                    {
                        return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                    }
                }

                var deleteRs = await _unitOfWork.PlaylistRepository.RemoveAsync(playlistExist);

                if (deleteRs)
                {
                    return new BusinessResult(Const.SUCCESS_DELETE, Const.SUCCESS_DELETE_MSG);
                }

                return new BusinessResult(Const.FAIL_DELETE, Const.FAIL_DELETE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> AddLessonToPlayList(int lessonId, int playlistId)
        {
            try
            {
                var playlistExist = await _unitOfWork.PlaylistRepository.GetByIdAsync(playlistId);

                if (playlistExist == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Playlist does not exist!");
                }

                var lessonExist = await _unitOfWork.LessonRepository.GetByIdAsync(lessonId);

                if (lessonExist == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Lesson does not exist!");
                }

                lessonExist.PlaylistId = playlistExist.PlaylistId;

                var updateRs = await _unitOfWork.LessonRepository.UpdateAsync(lessonExist);

                if (updateRs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, playlistExist);
                }

                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> AddMusicToPlayList(int musicId, int playlistId)
        {
            try
            {
                var playlistExist = await _unitOfWork.PlaylistRepository.GetByIdAsync(playlistId);

                if (playlistExist == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Playlist does not exist!");
                }

                var musicExist = await _unitOfWork.MusicRepository.GetByIdAsync(musicId);

                if (musicExist == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Music does not exist!");
                }

                musicExist.PlaylistId = playlistExist.PlaylistId;

                var updateRs = await _unitOfWork.MusicRepository.UpdateAsync(musicExist);

                if (updateRs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, playlistExist);
                }

                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
