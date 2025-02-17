using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SE.Service.Helper
{
    public static class CloudinaryHelper
    {
        private static readonly Cloudinary _cloudinary;

        static CloudinaryHelper()
        {
            _cloudinary = CloudinaryConfig.GetCloudinary();
        }

        public const string IMAGE_FOLDER = "IMAGES";
        public const string AUDIO_FOLDER = "AUDIOS";
        public const string VIDEO_FOLDER = "VIDEOS";

        public const string DOCUMENT_FOLDER = "DOCUMENTS";

        public static async Task<(string PublicId, string Url)> UploadImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("Invalid file: File is empty or null.");
            }

            using var fileStream = imageFile.OpenReadStream();
            var uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{imageFile.FileName}";

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(uniqueFileName, fileStream),
                Folder = IMAGE_FOLDER,
                Overwrite = true,
                UseFilename = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new Exception($"Upload failed: {result.Error.Message}");
            }

            return (result.PublicId, result.SecureUrl.ToString());
        }

        public static async Task<(string PublicId, string Url)> UploadVideoAsync(IFormFile videoFile)
        {
            if (videoFile == null || videoFile.Length == 0)
            {
                throw new ArgumentException("Invalid file: File is empty or null.");
            }

            using var fileStream = videoFile.OpenReadStream();
            var uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{videoFile.FileName}";

            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(uniqueFileName, fileStream),
                Folder = VIDEO_FOLDER,
                Overwrite = true,
                UseFilename = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new Exception($"Upload failed: {result.Error.Message}");
            }

            return (result.PublicId, result.SecureUrl.ToString());
        }

        public static async Task<(string PublicId, string Url)> UploadAudioAsync(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                throw new ArgumentException("Invalid file: File is empty or null.");
            }

            using var fileStream = audioFile.OpenReadStream();
            var uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{audioFile.FileName}";

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(uniqueFileName, fileStream),
                Folder = AUDIO_FOLDER,
                Overwrite = true,
                UseFilename = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new Exception($"Upload failed: {result.Error.Message}");
            }

            return (result.PublicId, result.SecureUrl.ToString());
        }

        public static async Task<(string PublicId, string Url)> UploadDocumentAsync(IFormFile documentFile)
        {
            if (documentFile == null || documentFile.Length == 0)
            {
                throw new ArgumentException("Invalid file: File is empty or null.");
            }

            using var fileStream = documentFile.OpenReadStream();
            var uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{documentFile.FileName}";

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(uniqueFileName, fileStream),
                Folder = DOCUMENT_FOLDER,
                Overwrite = false,
                UseFilename = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new Exception($"Upload failed: {result.Error.Message}");
            }

            return (result.PublicId, result.SecureUrl.ToString());
        }

        public static async Task<List<(string PublicId, string Url)>> UploadMultipleImagesAsync(List<IFormFile> imageFiles)
        {
            var uploadResults = new List<(string PublicId, string Url)>();

            foreach (var imageFile in imageFiles)
            {
                var (publicId, url) = await UploadImageAsync(imageFile);
                uploadResults.Add((publicId, url));
            }

            return uploadResults;
        }

        public static async Task<List<(string PublicId, string Url)>> UploadMultipleDocumentsAsync(List<IFormFile> documentFiles)
        {
            var uploadResults = new List<(string PublicId, string Url)>();

            foreach (var documentFile in documentFiles)
            {
                var (publicId, url) = await UploadDocumentAsync(documentFile);
                uploadResults.Add((publicId, url));
            }

            return uploadResults;
        }

        public static async Task<bool> DeleteImageAsync(string imagePublicId)
        {
            try
            {
                var deletionParams = new DeletionParams(imagePublicId)
                {
                    ResourceType = ResourceType.Image
                };

                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (result.Result == "ok")
                {
                    return true;
                }
                else
                {
                    throw new Exception($"Delete failed: {result.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deleting image: {ex.Message}");
            }
        }

        public static async Task<string> UpdateImageAsync(IFormFile newImageFile, string existingPublicId)
        {
            if (newImageFile == null || newImageFile.Length == 0)
            {
                throw new ArgumentException("Invalid file: File is empty or null.");
            }

            using var fileStream = newImageFile.OpenReadStream();
            var uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{newImageFile.FileName}";

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(uniqueFileName, fileStream),
                PublicId = existingPublicId,
                Overwrite = true,
                UseFilename = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new Exception($"Upload failed: {result.Error.Message}");
            }

            return result.SecureUrl.ToString();
        }
    }
}

