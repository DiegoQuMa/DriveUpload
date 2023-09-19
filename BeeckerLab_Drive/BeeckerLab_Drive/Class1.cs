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
        static void Main(string[] args)
        {
            string DrivePath = "1nPFSsAY4thQfJnmzzos6zG0pp7pCC2Td";/*"1nPFSsAY4thQfJnmzzos6zG0pp7pCC2Td"*/
            string credentialsPath = "credentials.json";
            string LocalPath = @"D:\TestImages";
            string[] ItemList = {  };
            CargadeArchivos(DrivePath, credentialsPath, ItemList, LocalPath);
        }
        public static void CargadeArchivos(string folderID, string credentialsPath, string[] ItemList, string LocalPath)
        {
            ValidarArguments(folderID, credentialsPath, LocalPath);
            ItemList = EliminarRepetidos(ItemList);
            try
            {
                // Get Credentials
                GoogleCredential credential = GetCredentials(credentialsPath);
                // Create Drive API service.
                var service = CreateService(credential);
                // Upload by Folder
                if (ItemList.Length < 1)
                {
                    // Get Files
                    ItemList = GetFilesbyFolder(LocalPath);
                    ItemList = EliminarRepetidos(ItemList);
                    foreach (var filePath in ItemList)
                    {
                        UpdloadFiles(service, filePath, folderID);
                    }
                    throw new ArgumentException("Ejecucion Exitosa");
                }
                
                
                foreach (var item in ItemList)
                {
                    //Get Files by Extension
                    if (item.StartsWith("."))
                    {
                        ItemList = GetFilesByExtension(LocalPath, item);
                    }
                    
                    // Get specific Files by name
                    else
                    {
                        ItemList = GetFilesByName(LocalPath, item);
                    }
                    // Upload File
                    foreach (var filePath in ItemList)
                    {
                        UpdloadFiles(service, filePath, folderID);
                       
                    }
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e is AggregateException)
                {
                    Console.WriteLine("Credential Not found");
                }
                else if (e is FileNotFoundException)
                {
                    Console.WriteLine("File not found");
                }
                else
                {
                    throw new Exception("La carpeta con el ID " + folderID + " no existe.");
                }
            }
        }

        // Method to Upload Files to drive
        public static void UpdloadFiles(DriveService service, string filePath, string folderID)
        {
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
            Console.WriteLine($"{file.Id} {file.Name}{file.MimeType} {file.Parents?.FirstOrDefault()}");
        }

        // Method to Get Google Credentials
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

        // Method to create an Google Server Instance
        public static DriveService CreateService(GoogleCredential credential)
        {
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Drive Upload"
            });
            return service;
        }

        // Create Drive File Metadata 
        public static Google.Apis.Drive.v3.Data.File GetFileData(string filePath, string folderID)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath),
                Parents = new List<string> { folderID }
            };
            return fileMetadata;
        }

        // Method to Get files from a local folder
        public static string[] GetFilesbyFolder(string LocalPath)
        {
            
            try
            {
                string[] ItemList = Directory.GetFiles(LocalPath);
                return ItemList;
            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException)
                {
                    throw new Exception("La ruta de la carpeta " + LocalPath + " no existe.");
                }
            }
            return null;
        }

        // Method to get files by extension from local folder
        public static string[] GetFilesByExtension(string LocalPath, string extension)
        {
            string[] ItemList = Directory.GetFiles(LocalPath, "*" + extension);
            return ItemList;
        }

        //Method to get files by name from local folder
        public static string[] GetFilesByName(string LocalPath, string Name)
        {
            string[] ItemList = Directory.GetFiles(LocalPath, Name + "*");
            return ItemList;
        }

        // Method to validate arguments data.
        static string ValidarArguments(string DrivePath, string credentialsPath, string LocalPathn)
        {
            string[] args = { DrivePath, credentialsPath, LocalPathn };
            for (int i = 0; i < 3; i++)
            {
                if (string.IsNullOrEmpty(args[i]))
                {
                    throw new ArgumentException("Los argumentos Requeridos son: folderID, credentialsPath y LocalPath");
                }
            }
            return null;
        }

        // Method to delete duplicated files
        static T[] EliminarRepetidos<T>(T[] Itemlist)
        {
            List<T> listaSinRepetidos = new List<T>();
            HashSet<T> conjunto = new HashSet<T>();
            foreach (var elemento in Itemlist)
            {
                if (conjunto.Add(elemento))
                {
                    listaSinRepetidos.Add(elemento);
                }
            }
            return listaSinRepetidos.ToArray();
        }

        

    }

}