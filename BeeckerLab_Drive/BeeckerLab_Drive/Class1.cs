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

            //Console.WriteLine("Insert Drive folder ID");
            string DrivePath = "1nPFSsAY4thQfJnmzzos6zG0pp7pCC2Td";/*"1nPFSsAY4thQfJnmzzos6zG0pp7pCC2Td"*/
            string credentialsPath = "credentials.json";
            //Console.WriteLine("Insert Local Folder");
            string LocalPath = @"D:\TestImages";
            
            

            

            //string[] ItemList = GetFilesByExtension(LocalPath, extension);
            string[] ItemList = {"image2.png",".jpg"};




            CargadeArchivos(DrivePath, credentialsPath, ItemList, LocalPath);
                // Directory.GetFile - Obtener Path 
            









        }



        public static void CargadeArchivos(string folderID, string credentialsPath, string[] ItemList, string LocalPath)
        {
            try
            {
                // Get Credentials
                GoogleCredential credential = GetCredentials(credentialsPath);

                // Create Drive API service.
                var service =  CreateService(credential);



                foreach (var item in ItemList)
                {
                    if (item.StartsWith("."))
                    {
                        ObtenerArchivosPorFiltro(LocalPath, item);
                    }
                    else
                    {
                        ObtenerArchivosPorFiltro(LocalPath,item);
                    }
                }

                        // Upload File
                        foreach (var filePath in ItemList)
                {
                    // Get File Data
                    var fileMetadata =  new Google.Apis.Drive.v3.Data.File()
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

        public static string[] GetFilesbyFolder(string LocalPath )
        {
            //if (Directory.Exists(LocalPath) == false)
            //{
            //    Console.WriteLine("La ruta de la carpeta " + LocalPath + " no existe.");
            //    System.Environment.Exit(0);

            //}
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

        public static string[] GetFilesByExtension(string LocalPath,string extension)
        {

            Console.Write(LocalPath);
            string[] ItemList = Directory.GetFiles(LocalPath, "*"+extension);
            return ItemList;
        }

        public static string[] GetFilesByName(string LocalPath, string Name)
        {
            

            string[] ItemList = Directory.GetFiles(LocalPath,  Name + "*");
            return ItemList;
        }
        static List<string> ObtenerArchivosPorFiltro(string rutaCarpeta, string filtro)
        {

            // Enumera todos los archivos en la carpeta local que coinciden parcialmente con el filtro.
            var archivos = Directory.GetFiles(rutaCarpeta)
                .Where(f => Path.GetFileName(f).IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            return archivos;
        }



        }
    
    
}
