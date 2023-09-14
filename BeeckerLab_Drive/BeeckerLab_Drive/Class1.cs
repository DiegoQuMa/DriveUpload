using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeckerLab_Drive
{
    public class Drive_CargaDeArchivos
    {


        static async Task Main(string[] args)
        {
            string path = "1nPFSsAY4thQfJnmzzos6zG0pp7pCC2Td";
            string credentialsPath = "credentials.json";
            string FilePaths =  "image.jpg" ;
            //CargadeArchivos(path,credentialsPath,FilePaths);
            // Directory.GetFile - Obtener Path 

            var credential = GoogleCredential.FromFile(credentialsPath)
                .CreateScoped(DriveService.ScopeConstants.Drive);

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = "Test Upload1.jpg",
                Parents = new List<string>() { "1nPFSsAY4thQfJnmzzos6zG0pp7pCC2Td" }
            };

            string uploadedFileId;

              using (var fsSource = new FileStream(FilePaths,FileMode.Open,FileAccess.Read))
            {
                var request = service.Files.Create(fileMetadata, fsSource, "");
                request.Fields = "*";
                var results =  await request.UploadAsync(CancellationToken.None);

                if(results.Status == Google.Apis.Upload.UploadStatus.Failed)
                {
                    Console.WriteLine($"Error uploading file: {results.Exception.Message}");
                }
                uploadedFileId = request.ResponseBody?.Id;
            }

            var uploadedFile = await service.Files.Get(uploadedFileId).ExecuteAsync();
            Console.WriteLine($"{uploadedFile.Id} {uploadedFile.Name}{uploadedFile.MimeType} {uploadedFile.Parents?.FirstOrDefault()}");




        }



        public static Google.Apis.Drive.v3.Data.File CargadeArchivos(string folderID, string credentialsPath, string[] FilePaths)
        {
            try
            {
                // Get Credentials
                GoogleCredential credential = GetCredentials(credentialsPath);

                // Create Drive API service.
                var service =  CreateService(credential);
                


                // Upload File
                foreach (var filePath in FilePaths)
                {
                    // Get File Data
                    var fileMetadata = GetFileData(filePath, folderID);
                    
                    FilesResource.CreateMediaUpload request;
                    // Create a new file on drive.
                    using (var stream = new FileStream(filePath,
                               FileMode.Open))
                    {
                        // Create a new file, with metadata and stream.
                        request = service.Files.Create(
                            fileMetadata, stream, "");
                        request.Fields = "id";
                        request.Upload();
                    }
                    var file = request.ResponseBody;
                    // Prints the uploaded file id.
                    Console.WriteLine("File ID: " + file.Id);
                    return file;

                }
            }

            catch (Exception e)
            {
                
                if (e is AggregateException)
                {
                    Console.WriteLine("Credential Not found");
                }
                else if (e is FileNotFoundException)
                {
                    Console.WriteLine("File not found");
                }
                else if (e is DirectoryNotFoundException)
                {
                    Console.WriteLine("Directory Not found");
                }
                else
                {
                    throw;
                }
            }
            
            return null;






        }

        public static GoogleCredential GetCredentials(string credentialsPath)
        {
            GoogleCredential credential;

            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(new[]
                {
                        DriveService.ScopeConstants.DriveFile
                    });
            }
            return credential;
        }

        public static DriveService CreateService(GoogleCredential credential)
        {
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Drive Upload"
            });
            return service;
        }

        public static Google.Apis.Drive.v3.Data.File GetFileData(string filePath, string folderID)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath),
                Parents = new List<string> { folderID }
            };
            return fileMetadata;
        }

    }
    
    
}
