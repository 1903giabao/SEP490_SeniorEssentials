using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Storage;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SE.Common.Setting;

namespace SE.Service.Services
{
    public interface IFirebaseService
    {
        Task<string> UploadImageToFirebaseAsync(IFormFile imageFile, string basePath);
        Task<bool> DeleteFileFromFirebase(string basePath);

    }

    public class FirebaseService : IFirebaseService
    {
        private readonly FirebaseConfiguration _firebaseConfig;

        public FirebaseService(IOptions<FirebaseConfiguration> firebaseConfig)
        {
            _firebaseConfig = firebaseConfig.Value;
        }


        public async Task<string> UploadImageToFirebaseAsync(IFormFile imageFile, string basePath)
        {
            if (imageFile.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
                
                var config = new FirebaseAuthConfig
                {
                    ApiKey = _firebaseConfig.ApiKey,
                    AuthDomain = _firebaseConfig.AuthDomain,
                    Providers = new FirebaseAuthProvider[]
    {
                new EmailProvider(),
                new GoogleProvider()
    }
                };

                // Define the bucket and the object name
                var authClient = new FirebaseAuthClient(config);


                var userCens = await authClient.SignInWithEmailAndPasswordAsync(_firebaseConfig.AuthEmail, _firebaseConfig.AuthPassword);

                if (userCens == null) throw new FirebaseAuthException("Can not ss", new AuthErrorReason());

                var storage = new FirebaseStorage(
                    _firebaseConfig.Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = async () =>
                        {
                            var idToken = await userCens.User.GetIdTokenAsync();
                            return idToken;
                        },
                        ThrowOnCancel = true
                    });

                var stream = imageFile.OpenReadStream();
                var destinationPath = $"{basePath}/{imageFile.FileName}";

                var downloadUrl = await storage.Child(destinationPath).PutAsync(stream);
                return downloadUrl;
            }

            return null;
        }


        public async Task<bool> DeleteFileFromFirebase(string basePath)
        {

            try
            {

                var filePath = ExtractFilePathFromUrl(basePath);


                var config = new FirebaseAuthConfig
                {
                    ApiKey = _firebaseConfig.ApiKey,
                    AuthDomain = _firebaseConfig.AuthDomain,
                    Providers = new FirebaseAuthProvider[]
    {
                new EmailProvider(),
                new GoogleProvider()
    }
                };


                var authClient = new FirebaseAuthClient(config);


                var userCens = await authClient.SignInWithEmailAndPasswordAsync(_firebaseConfig.AuthEmail, _firebaseConfig.AuthPassword);

                if (userCens == null) throw new FirebaseAuthException("Can not ss", new AuthErrorReason());

                var storage = new FirebaseStorage(
                     _firebaseConfig.Bucket,
                     new FirebaseStorageOptions
                     {
                         AuthTokenAsyncFactory = async () =>
                         {
                             var idToken = await userCens.User.GetIdTokenAsync();
                             return idToken;
                         },
                         ThrowOnCancel = true
                     });
                await storage.Child(filePath).DeleteAsync();
                return true;
            }
            catch (FirebaseStorageException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }
        private string ExtractFilePathFromUrl(string fileUrl)
        {
            // The path starts after '/o/' and ends before '?'
            var startIndex = fileUrl.IndexOf("/o/") + 3; // Skipping '/o/'
            var endIndex = fileUrl.IndexOf("?");

            if (startIndex >= 0 && endIndex > startIndex)
            {
                // Extract the path and replace any encoded characters
                var filePath = fileUrl.Substring(startIndex, endIndex - startIndex);
                return Uri.UnescapeDataString(filePath); // Decode URL-encoded characters (like %2F)
            }

            throw new ArgumentException("Invalid file URL format.");
        }


    }
}
