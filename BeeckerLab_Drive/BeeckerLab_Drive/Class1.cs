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
            string FOLDER_ID = "1nPFSsAY4thQfJnmzzos6zG0pp7pCC2Td";
            string JSON_PATH = "credentials.json";
            string LOCAL_PATH = @"D:\TestImages";
            string[] ITEM_LIST = { "image.jpg", "xd.xls", "image2.png" };
            Console.WriteLine(Carga_De_Archivos(FOLDER_ID, JSON_PATH, ITEM_LIST, LOCAL_PATH));
        }
        public static string Carga_De_Archivos(string FOLDER_ID, string JSON_PATH, string[] ITEM_LIST, string LOCAL_PATH)
        {
            Validar_Arguments(FOLDER_ID, JSON_PATH, LOCAL_PATH);
            ITEM_LIST = Eliminar_Repetidos(ITEM_LIST);
            try
            {
                int FILES_COUNT = 0;
                // Get Credentials
                GoogleCredential CREDENTIAL = Get_Credentials(JSON_PATH);
                // Create Drive API SERVICE.
                var SERVICE = Create_Service(CREDENTIAL);
                // Upload by Folder
                if (ITEM_LIST.Length < 1)
                {
                    // Get Files
                    ITEM_LIST = Get_FilesbyFolder(LOCAL_PATH);
                    ITEM_LIST = Eliminar_Repetidos(ITEM_LIST);
                    foreach (var FILEPATH in ITEM_LIST)
                    {
                        Upload_Files(SERVICE, FILEPATH, FOLDER_ID);
                        FILES_COUNT++;
                    }
                    
                }
                
                
                foreach (var ITEM in ITEM_LIST)
                {
                    //Get Files by Extension
                    if (ITEM.StartsWith("."))
                    {
                        ITEM_LIST = Get_FilesByExtension(LOCAL_PATH, ITEM);
                    }
                    
                    // Get specific Files by name
                    else
                    {
                        ITEM_LIST = Get_FilesByName(LOCAL_PATH, ITEM);
                    }
                    // Upload File
                    foreach (var FILEPATH in ITEM_LIST)
                    {
                        Upload_Files(SERVICE, FILEPATH, FOLDER_ID);
                        FILES_COUNT++;
                    }
                }
                return $"Se descargaron {FILES_COUNT} archivos \n Ejecucion Exitosa";
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e is AggregateException)
                {
                    throw new Exception("Credential Not found");
                }
                else if (e is FileNotFoundException)
                {
                    throw new Exception("File not found");
                }
                else
                {
                    throw new Exception($"La carpeta con el ID  { FOLDER_ID }  no existe.");
                }
            }
        }

        // Method to Upload Files to drive
        public static void Upload_Files(DriveService SERVICE, string FILEPATH, string FOLDER_ID)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(FILEPATH),
                Parents = new List<string> { FOLDER_ID }
            };
            FilesResource.CreateMediaUpload request;
            // Create a new file on drive.
            using (var stream = new FileStream(FILEPATH,
                       FileMode.Open))
            {
                // Create a new file, with metadata and stream.
                request = SERVICE.Files.Create(
                    fileMetadata, stream, "");
                request.Fields = "id";
                request.Upload();
            }
            
            // Prints the uploaded file id.
            
            
        }

        // Method to Get Google Credentials
        public static GoogleCredential Get_Credentials(string JSON_PATH)
        {
            GoogleCredential CREDENTIAL;
            using (var stream = new FileStream(JSON_PATH, FileMode.Open, FileAccess.Read))
            {
                CREDENTIAL = GoogleCredential.FromStream(stream).CreateScoped(new[]
                {
                        DriveService.ScopeConstants.DriveFile
                    });
            }
            return CREDENTIAL;
        }

        // Method to create an Google Server Instance
        public static DriveService Create_Service(GoogleCredential CREDENTIAL)
        {
            var SERVICE = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = CREDENTIAL,
                ApplicationName = "Google Drive Upload"
            });
            return SERVICE;
        }

        // Create Drive File Metadata 
        public static Google.Apis.Drive.v3.Data.File Get_FileData(string FILEPATH, string FOLDER_ID)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(FILEPATH),
                Parents = new List<string> { FOLDER_ID }
            };
            return fileMetadata;
        }

        // Method to Get files from a local folder
        public static string[] Get_FilesbyFolder(string LOCAL_PATH)
        {
            
            try
            {
                string[] ITEM_LIST = Directory.GetFiles(LOCAL_PATH);
                return ITEM_LIST;
            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException)
                {
                    throw new Exception($"La ruta de la carpeta { LOCAL_PATH }  no existe.");
                }
            }
            return null;
        }

        // Method to get files by EXTENSION from local folder
        public static string[] Get_FilesByExtension(string LOCAL_PATH, string EXTENSION)
        {
            string[] ITEM_LIST = Directory.GetFiles(LOCAL_PATH, "*" + EXTENSION);
            return ITEM_LIST;
        }

        //Method to get files by name from local folder
        public static string[] Get_FilesByName(string LOCAL_PATH, string NAME)
        {
            if (!Directory.EnumerateFileSystemEntries(LOCAL_PATH).Any())
            {
                throw new Exception($"La carpeta: { LOCAL_PATH } está vacía");

            }
            string[] ITEM_LIST = Directory.GetFiles(LOCAL_PATH, NAME + "*");
            return ITEM_LIST;

            
        }

        // Method to validate arguments data.
        static string Validar_Arguments(string FOLDER_ID, string JSON_PATH, string LocalPathn)
        {
            string[] args = { FOLDER_ID, JSON_PATH, LocalPathn };
            for (int i = 0; i < 3; i++)
            {
                if (string.IsNullOrEmpty(args[i]))
                {
                    throw new ArgumentException("Los argumentos Requeridos son: FOLDER_ID, JSON_PATH y LOCAL_PATH");
                }
            }
            return null;
        }

        // Method to delete duplicated files
        static T[] Eliminar_Repetidos<T>(T[] ITEM_LIST)
        {
            List<T> listaSinRepetidos = new List<T>();
            HashSet<T> conjunto = new HashSet<T>();
            foreach (var elemento in ITEM_LIST)
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