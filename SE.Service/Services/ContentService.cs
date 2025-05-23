﻿using AutoMapper;
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
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using MediaInfo;
using NAudio.Wave;
using ATL;
using TagLib;
using TagLib.Flac;
using ATL.Playlist;
using SE.Common.Response.Content;

namespace SE.Service.Services
{
    public interface IContentService
    {
        Task<IBusinessResult> GetAllContents();
        Task<IBusinessResult> GetAllMusics(int playlistId);
        Task<IBusinessResult> GetAllLessons(int playlistId);
        Task<IBusinessResult> GetAllBooks();
        Task<IBusinessResult> GetAllLessonPlaylist();
        Task<IBusinessResult> GetAllMusicPlaylist();
        Task<IBusinessResult> CreateMusic(CreateMusicRequest req);
        Task<IBusinessResult> ChangeMusicStatus(int musicId, string status);
        Task<IBusinessResult> DeleteMusicByAdmin(int musicId);
        Task<IBusinessResult> CreateBook(CreateBookRequest req);
        Task<IBusinessResult> ChangeBookStatus(int bookId, string status);
        Task<IBusinessResult> DeleteBookByAdmin(int bookId);
        Task<IBusinessResult> CreateLesson(CreateLessonRequest req);
        Task<IBusinessResult> ChangeLessonStatus(int lessonId, string status);
        Task<IBusinessResult> DeleteLessonByAdmin(int lessonId);
        Task<IBusinessResult> CreatePlaylist(CreatePlaylistRequest req);
        Task<IBusinessResult> UpdatePlaylist(UpdatePlaylistRequest req);
        Task<IBusinessResult> ChangePlaylistStatus(int playlistId, string status);
        Task<IBusinessResult> AddLessonToPlayList(int lessonId, int playlistId);
        Task<IBusinessResult> AddMusicToPlayList(int musicId, int playlistId);
        Task<IBusinessResult> DeletePlaylistByAdmin(int playlistId);
        Task<IBusinessResult> UpdateBook(UpdateBookRequest req);
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

        public async Task<IBusinessResult> GetAllContents()
        {
            try
            {
                var musics = _unitOfWork.MusicRepository.GetAll().ToList();

                var musicRs = _mapper.Map<List<MusicDTO>>(musics);

                var lessons = _unitOfWork.LessonRepository.GetAll().ToList();

                var lessonRs = _mapper.Map<List<LessonDTO>>(lessons);

                var books = _unitOfWork.BookRepository.GetAll();

                var bookRs = _mapper.Map<List<BookDTO>>(books);

                var lessonPlaylists = await _unitOfWork.PlaylistRepository.GetAllLessonsPlaylist();

                var listLessonPlaylist = new List<PlaylistDTO>();

                if (lessonPlaylists != null && lessonPlaylists.Any())
                {
                    foreach (var playlist in lessonPlaylists)
                    {
                        var playlistDTO = new PlaylistDTO
                        {
                            PlaylistId = playlist.PlaylistId,
                            PlaylistName = playlist.PlaylistName,
                            ImageUrl = playlist.ImageUrl,
                            NumberOfContent = playlist.Lessons.Count,
                            Status = playlist.Status,
                        };

                        listLessonPlaylist.Add(playlistDTO);
                    }
                }

                var musicPlaylists = await _unitOfWork.PlaylistRepository.GetAllMusicsPlaylist();

                var listMusicPlaylist = new List<PlaylistDTO>();

                if (musicPlaylists != null && musicPlaylists.Any())
                {
                    foreach (var playlist in musicPlaylists)
                    {
                        var playlistDTO = new PlaylistDTO
                        {
                            PlaylistId = playlist.PlaylistId,
                            PlaylistName = playlist.PlaylistName,
                            NumberOfContent = playlist.Musics.Count,
                            Status = playlist.Status,
                        };

                        listMusicPlaylist.Add(playlistDTO);
                    }
                }

                var rs = new GetAllContentResponse
                {
                    Books = bookRs.Count,
                    Musics = musicRs.Count,
                    Lessons = lessonRs.Count,
                    MusicPlaylists = listMusicPlaylist.Count,
                    LessonPlaylists = listLessonPlaylist.Count,
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllMusics(int playlistId)
        {
            try
            {
                var musics = _unitOfWork.MusicRepository.FindByCondition(m => m.PlaylistId == playlistId).ToList();

                var rs = _mapper.Map<List<MusicDTO>>(musics);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }        
        
        public async Task<IBusinessResult> GetAllLessons(int playlistId)
        {
            try
            {
                var lessons = _unitOfWork.LessonRepository.FindByCondition(l => l.PlaylistId == playlistId).ToList();

                var rs = _mapper.Map<List<LessonDTO>>(lessons);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllBooks()
        {
            try
            {
                var books = _unitOfWork.BookRepository.GetAll();

                var rs = _mapper.Map<List<BookDTO>>(books);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }        
        
        public async Task<IBusinessResult> GetAllLessonPlaylist()
        {
            try
            {
                var playlists = await _unitOfWork.PlaylistRepository.GetAllLessonsPlaylist();

                var listLessonPlaylist = new List<PlaylistDTO>();

                if (playlists != null && playlists.Any())
                {
                    foreach (var playlist in playlists)
                    {
                        var playlistDTO = new PlaylistDTO
                        {
                            PlaylistId = playlist.PlaylistId,
                            PlaylistName = playlist.PlaylistName,
                            ImageUrl = playlist.ImageUrl,
                            NumberOfContent = playlist.Lessons.Count,
                            Status = playlist.Status,
                        };

                        listLessonPlaylist.Add(playlistDTO);
                    }
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, listLessonPlaylist);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }        
        
        public async Task<IBusinessResult> GetAllMusicPlaylist()
        {
            try
            {
                var playlists = await _unitOfWork.PlaylistRepository.GetAllMusicsPlaylist();

                var listMusicPlaylist = new List<PlaylistDTO>();

                if (playlists != null && playlists.Any())
                {
                    foreach (var playlist in playlists)
                    {
                        var playlistDTO = new PlaylistDTO
                        {
                            PlaylistId = playlist.PlaylistId,
                            PlaylistName = playlist.PlaylistName,
                            NumberOfContent = playlist.Musics.Count,
                            Status = playlist.Status,
                            ImageUrl = playlist.ImageUrl
                        };

                        listMusicPlaylist.Add(playlistDTO);
                    }
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, listMusicPlaylist);
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

                var playlistExist = await _unitOfWork.PlaylistRepository.GetByIdAsync(req.PlaylistId);

                if (playlistExist == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Playlist does not exist!");
                }

                foreach (var file in req.MusicFiles)
                {
                    using (var stream = file.OpenReadStream())
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        var tagLibFile = TagLib.File.Create(new StreamFileAbstraction(file.FileName, memoryStream, null));
                        var musicName = GetTitleBeforeHyphen(tagLibFile.Tag.Title);
                        var singer = tagLibFile.Tag.FirstPerformer;

                        var musicURL = ("", "");

                        if (file != null)
                        {
                            musicURL = await CloudinaryHelper.UploadAudioAsync(file);
                        }

                        var imageURL = ("", "");

                        var pictures = tagLibFile.Tag.Pictures;
                        if (pictures != null && pictures.Length > 0)
                        {
                            var picture = pictures[0];

                            using (var memoryStreamImage = new MemoryStream(picture.Data.Data))
                            {
                                string fileName = "cover" + GetFileExtension(picture.MimeType);

                                IFormFile formFile = new FormFile(
                                    baseStream: memoryStreamImage,
                                    baseStreamOffset: 0,
                                    length: picture.Data.Data.Length,
                                    name: "file",
                                    fileName: fileName
                                )
                                {
                                    Headers = new HeaderDictionary(),
                                    ContentType = picture.MimeType
                                };

                                if (formFile != null)
                                {
                                    imageURL = await CloudinaryHelper.UploadImageAsync(formFile);
                                }
                            }
                        }

                        var music = new Music
                        {
                            AccountId = req.AccountId,
                            CreatedDate = DateTime.UtcNow.AddHours(7),
                            MusicName = musicName,
                            Singer = singer,
                            ImageUrl = imageURL.Item2,
                            PlaylistId = req.PlaylistId,
                            MusicUrl = musicURL.Item2,
                            Status = SD.ContentStatus.ACTIVE,
                        };

                        var createRs = await _unitOfWork.MusicRepository.CreateAsync(music);

                        if (createRs < 1)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                        }
                    }
                }

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        private string GetFileExtension(string mimeType)
        {
            switch (mimeType)
            {
                case "image/jpeg": return ".jpg";
                case "image/png": return ".png";
                case "image/gif": return ".gif";
                case "image/bmp": return ".bmp";
                default: return ".jpg";
            }
        }

        private string GetTitleBeforeHyphen(string fullTitle)
        {
            if (string.IsNullOrEmpty(fullTitle))
            {
                return fullTitle;
            }

            var parts = fullTitle.Split(new[] { '-' }, 2, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0].Trim() : fullTitle;
        }

        public async Task<IBusinessResult> ChangeMusicStatus(int musicId, string status)
        {
            try
            {
                var music = await _unitOfWork.MusicRepository.GetByIdAsync(musicId);

                if (music == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Music does not exist!");
                }

                if (!status.Equals(SD.GeneralStatus.ACTIVE) && !status.Equals(SD.ContentStatus.INACTIVE))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Status does not support!");
                }

                var playlist = await _unitOfWork.PlaylistRepository.GetPlaylistById((int)music.PlaylistId);

                if (playlist == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Playlist does not exist!");
                }

                music.Status = status;

                var deleteRs = await _unitOfWork.MusicRepository.UpdateAsync(music);

                if (deleteRs > 0)
                {
                    if (status.Equals(SD.GeneralStatus.INACTIVE))
                    {
                        var listMusic = playlist.Musics.ToList();

                        bool allInactive = listMusic.All(musicItem => musicItem.Status.Equals(SD.ContentStatus.INACTIVE) || musicItem.Status.Equals(SD.ContentStatus.ADMINDELETE));

                        if (allInactive)
                        {
                            playlist.Status = SD.ContentStatus.INACTIVE;

                            var inactiveRs = await _unitOfWork.PlaylistRepository.UpdateAsync(playlist);

                            if (inactiveRs < 1)
                            {
                                return new BusinessResult(Const.FAIL_DELETE, Const.FAIL_DELETE_MSG);
                            }
                        }
                    }

                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
                }

                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }        
        
        public async Task<IBusinessResult> DeleteMusicByAdmin(int musicId)
        {
            try
            {
                var music = await _unitOfWork.MusicRepository.GetByIdAsync(musicId);

                if (music == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Music does not exist!");
                }

                var playlist = await _unitOfWork.PlaylistRepository.GetPlaylistById((int)music.PlaylistId);

                if (playlist == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Playlist does not exist!");
                }

                music.Status = SD.ContentStatus.ADMINDELETE;

                var deleteRs = await _unitOfWork.MusicRepository.UpdateAsync(music);

                if (deleteRs > 0)
                {
                    var listMusic = playlist.Musics.ToList();

                    bool allInactive = listMusic.All(musicItem => musicItem.Status.Equals(SD.ContentStatus.INACTIVE) || musicItem.Status.Equals(SD.ContentStatus.ADMINDELETE));

                    if (allInactive)
                    {
                        playlist.Status = SD.ContentStatus.INACTIVE;

                        var inactiveRs = await _unitOfWork.PlaylistRepository.UpdateAsync(playlist);

                        if (inactiveRs < 1)
                        {
                            return new BusinessResult(Const.FAIL_DELETE, Const.FAIL_DELETE_MSG);
                        }
                    }

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

                var currentTime = DateTime.UtcNow.AddHours(7);

                if (req.PublishDate >= currentTime)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Ngày xuất bản phải trước ngày hiện tại!");
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

        public async Task<IBusinessResult> UpdateBook(UpdateBookRequest req)
        {
            try
            {
                var book = await _unitOfWork.BookRepository.GetByIdAsync(req.BookId);

                if (book == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Book does not exist!");
                }

                var currentTime = DateTime.UtcNow.AddHours(7);

                if (req.PublishDate >= currentTime)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Ngày xuất bản phải trước ngày hiện tại!");
                }

                book.BookName = req.BookName;
                book.BookType = req.BookType;
                book.PublishDate = req.PublishDate;
                book.Author = req.Author;

                var updateRs = await _unitOfWork.BookRepository.UpdateAsync(book);

                if (updateRs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
                }

                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }            
        
        public async Task<IBusinessResult> ChangeBookStatus(int bookId, string status)
        {
            try
            {
                var book = await _unitOfWork.BookRepository.GetByIdAsync(bookId);

                if (book == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Book does not exist!");
                }

                if (!status.Equals(SD.GeneralStatus.ACTIVE) && !status.Equals(SD.ContentStatus.INACTIVE))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Status does not support!");
                }

                book.Status = status;

                var deleteRs = await _unitOfWork.BookRepository.UpdateAsync(book);

                if (deleteRs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
                }

                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }        
        
        public async Task<IBusinessResult> DeleteBookByAdmin(int bookId)
        {
            try
            {
                var book = await _unitOfWork.BookRepository.GetByIdAsync(bookId);

                if (book == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Book does not exist!");
                }

                book.Status = SD.ContentStatus.ADMINDELETE;

                var deleteRs = await _unitOfWork.BookRepository.UpdateAsync(book);

                if (deleteRs > 0)
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

                var file = req.LessonFile;

                var lessonURL = ("", "");

                if (file != null)
                {
                    lessonURL = await CloudinaryHelper.UploadVideoAsync(file);
                }

                var imageURL = ("", "");

                if (req.ThumbnailImage != null)
                {
                    imageURL = await CloudinaryHelper.UploadImageAsync(req.ThumbnailImage);
                }

                var lesson = new Lesson
                {
                    AccountId = req.AccountId,
                    PlaylistId = playlistExist.PlaylistId,
                    LessonName = req.LessonName,
                    LessonUrl = lessonURL.Item2,
                    ImageUrl = imageURL.Item2,
                    CreatedDate = DateTime.UtcNow.AddHours(7),
                    Status = SD.ContentStatus.ACTIVE,
                };

                var createRs = await _unitOfWork.LessonRepository.CreateAsync(lesson);

                if (createRs < 1)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> ChangeLessonStatus(int lessonId, string status)
        {
            try
            {
                var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(lessonId);

                if (lesson == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Lesson does not exist!");
                }

                if (!status.Equals(SD.GeneralStatus.ACTIVE) && !status.Equals(SD.ContentStatus.INACTIVE))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Status does not support!");
                }

                var playlist = await _unitOfWork.PlaylistRepository.GetPlaylistById((int)lesson.PlaylistId);

                if (playlist == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Playlist does not exist!");
                }

                lesson.Status = status;

                var deleteRs = await _unitOfWork.LessonRepository.UpdateAsync(lesson);

                if (deleteRs > 0)
                {
                    if (status.Equals(SD.GeneralStatus.INACTIVE))
                    {
                        var listLesson = playlist.Musics.ToList();

                        bool allInactive = listLesson.All(lessonItem => lessonItem.Status.Equals(SD.ContentStatus.INACTIVE) || lessonItem.Status.Equals(SD.ContentStatus.ADMINDELETE));

                        if (allInactive)
                        {
                            playlist.Status = SD.ContentStatus.INACTIVE;

                            var inactiveRs = await _unitOfWork.PlaylistRepository.UpdateAsync(playlist);

                            if (inactiveRs < 1)
                            {
                                return new BusinessResult(Const.FAIL_DELETE, Const.FAIL_DELETE_MSG);
                            }
                        }
                    }

                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
                }

                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }        
        
        public async Task<IBusinessResult> DeleteLessonByAdmin(int lessonId)
        {
            try
            {
                var lesson = await _unitOfWork.LessonRepository.GetLessonsByIdAsync(lessonId);

                if (lesson == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Lesson does not exist!");
                }

                var playlist = await _unitOfWork.PlaylistRepository.GetPlaylistById((int)lesson.PlaylistId);

                if (playlist == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Playlist does not exist!");
                }

                lesson.Status = SD.ContentStatus.ADMINDELETE;

                var deleteRs = await _unitOfWork.LessonRepository.UpdateAsync(lesson);

                if (deleteRs > 0)
                {
                    var listLesson = playlist.Musics.ToList();

                    bool allInactive = listLesson.All(lessonItem => lessonItem.Status.Equals(SD.ContentStatus.INACTIVE) || lessonItem.Status.Equals(SD.ContentStatus.ADMINDELETE));

                    if (allInactive)
                    {
                        playlist.Status = SD.ContentStatus.INACTIVE;

                        var inactiveRs = await _unitOfWork.PlaylistRepository.UpdateAsync(playlist);

                        if (inactiveRs < 1)
                        {
                            return new BusinessResult(Const.FAIL_DELETE, Const.FAIL_DELETE_MSG);
                        }
                    }

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

                var imageURL = ("", "");

                if (req.PlaylistImage != null)
                {
                    imageURL = await CloudinaryHelper.UploadImageAsync(req.PlaylistImage);
                }
                else if (req.PlaylistImage == null && req.IsLesson)
                {
                    imageURL.Item2 = "https://res.cloudinary.com/drtn3fqci/image/upload/v1743231522/q3varo58nxkz_etly2b.webp";
                }                
                else
                {
                    imageURL.Item2 = "https://res.cloudinary.com/drtn3fqci/image/upload/v1743231326/31Fq-eqH4bL._AC_UF1000_1000_QL80__szbqew.jpg";
                }



                var playlist = new Playlist
                {
                    AccountId = req.AccountId,
                    PlaylistName = req.PlaylistName,
                    CreatedDate = DateTime.UtcNow.AddHours(7),
                    ImageUrl = imageURL.Item2,
                    Status = SD.ContentStatus.ACTIVE,
                    IsLesson = req.IsLesson,
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

                var listMusic = playlistExist.Musics.ToList();

                if (listMusic.Any())
                {
                    bool allActiveMusic = listMusic.All(musicItem => musicItem.Status.Equals(SD.ContentStatus.INACTIVE) || musicItem.Status.Equals(SD.ContentStatus.ADMINDELETE));

                    if (allActiveMusic)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "PLAYLIST HIỆN KHÔNG CÓ BÀI HÁT NÀO ĐƯỢC ACTIVE!");
                    }
                }

                var listLesson = playlistExist.Lessons.ToList();

                if (listLesson.Any())
                {
                    bool allActiveLesson = listLesson.All(lessonItem => lessonItem.Status.Equals(SD.ContentStatus.INACTIVE) || lessonItem.Status.Equals(SD.ContentStatus.ADMINDELETE));

                    if (allActiveLesson)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "DANH SÁCH BÀI HỌC HIỆN KHÔNG CÓ BÀI HỌC NÀO ĐƯỢC ACTIVE!");
                    }
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

        public async Task<IBusinessResult> ChangePlaylistStatus(int playlistId, string status)
        {
            try
            {
                var playlistExist = await _unitOfWork.PlaylistRepository.GetPlaylistById(playlistId);

                if (playlistExist == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Playlist does not exist!");
                }

                if (!status.Equals(SD.GeneralStatus.ACTIVE) && !status.Equals(SD.ContentStatus.INACTIVE))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Status does not support!");
                }

                if (status.Equals(SD.GeneralStatus.ACTIVE))
                {
                    var listMusic = playlistExist.Musics.ToList();

                    if (listMusic.Any())
                    {
                        bool allActiveMusic = listMusic.All(musicItem => musicItem.Status.Equals(SD.ContentStatus.INACTIVE) || musicItem.Status.Equals(SD.ContentStatus.ADMINDELETE));

                        if (allActiveMusic)
                        {
                            return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "PLAYLIST HIỆN KHÔNG CÓ BÀI HÁT NÀO ĐƯỢC ACTIVE!");
                        }                        
                    }

                    var listLesson = playlistExist.Lessons.ToList();

                    if (listLesson.Any())
                    {
                        bool allActiveLesson = listLesson.All(lessonItem => lessonItem.Status.Equals(SD.ContentStatus.INACTIVE) || lessonItem.Status.Equals(SD.ContentStatus.ADMINDELETE));

                        if (allActiveLesson)
                        {
                            return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "DANH SÁCH BÀI HỌC HIỆN KHÔNG CÓ BÀI HỌC NÀO ĐƯỢC ACTIVE!");
                        }
                    }
                }

                playlistExist.Status = status;

                var deleteRs = await _unitOfWork.PlaylistRepository.UpdateAsync(playlistExist);

                if (deleteRs > 0)
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

        public async Task<IBusinessResult> DeletePlaylistByAdmin(int playlistId)
        {
            try
            {
                var playlistExist = await _unitOfWork.PlaylistRepository.GetByIdAsync(playlistId);

                if (playlistExist == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Playlist does not exist!");
                }

                playlistExist.Status = SD.ContentStatus.ADMINDELETE;

                var deleteRs = await _unitOfWork.PlaylistRepository.UpdateAsync(playlistExist);

                if (deleteRs > 0)
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
