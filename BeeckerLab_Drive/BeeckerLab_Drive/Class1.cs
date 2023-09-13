using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeckerLab_Drive
{
    public class Drive_CargaDeArchivos
    {


        static void Main()
        {
            string path = "1MdJrBaBT9x8792VEh4fXipXItrTgWJTn";
            string credentialsPath = "credentials.json";
            string[] FilePaths = { @"D:\BeeckerTrabajo\BeeckerLab_Drive\BeeckerLab_Drive\bin\Debug\image.jpg", @"D:\BeeckerTrabajo\BeeckerLab_Drive\BeeckerLab_Drive\bin\Debug\image2.png" };
            Console.WriteLine(FilePaths[1]);
            CargadeArchivos(path,credentialsPath,FilePaths);
        }

        public static Google.Apis.Drive.v3.Data.File CargadeArchivos(string folderID, string credentialsPath, string[] FilePaths)
        {
            try
            {
                // Get Credentials
                GoogleCredential credential;

                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(new[]
                    {
                        DriveService.ScopeConstants.DriveFile
                    });
                }

                

                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Google Drive Upload"
                });

                // Upload File
                foreach (var filePath in FilePaths)
                {
                    // Get File Data
                    var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = Path.GetFileName(filePath),
                        Parents = new List<string> { folderID }
                    };
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

    }
    
    
}
