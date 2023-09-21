using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace BeeckerLab_Drive
{
    public class Drive_CargaDeArchivos
    {
        

        /* Metodo principal para cargar archivos a drive.
            Folder_ID - ID de la carpeta de drive.
            JSON_PATH - Ruta local del archivo JSON.
            ITEM_LIST - Nombre de archivos especificos.
            LOCAL_PATH - Ruta de la carpeta local.
         */
        public string Carga_De_Archivos(string FOLDER_ID, string JSON_PATH, string[] ITEM_LIST, string LOCAL_PATH)
        {
            // Validar si los argumentos estan completos.
            Validar_Arguments(FOLDER_ID, JSON_PATH, LOCAL_PATH);
            
            try
            {
                ITEM_LIST = Eliminar_Repetidos(ITEM_LIST);
                int FILES_COUNT = 0;
                // Obtener Credenciales con el archivo JSON.
                GoogleCredential CREDENTIAL = Get_Credentials(JSON_PATH);
                // Crear un servicio de Drive.
                var SERVICE = Create_Service(CREDENTIAL);
                // Cargar archivos por ruta de Folder
                if (ITEM_LIST.Length < 1)
                {
                    // Obtener archivos por ruta de folder
                    ITEM_LIST = Get_Files_by_Folder(LOCAL_PATH);
                    // Eliminar archivos repetidos
                    ITEM_LIST = Eliminar_Repetidos(ITEM_LIST);
                    // Cargar archivos a drive.
                    foreach (var FILEPATH in ITEM_LIST)
                    {
                        Upload_Files(SERVICE, FILEPATH, FOLDER_ID);
                        FILES_COUNT++;
                    }
                    
                }
                
                
                foreach (var ITEM in ITEM_LIST)
                {
                    // Obtener archivos por extension.
                    if (ITEM.StartsWith("."))
                    {
                        ITEM_LIST = Get_Files_By_Extension(LOCAL_PATH, ITEM);
                        
                    }
                    
                    // Obtener archivos por nombre
                    else
                    {
                        ITEM_LIST = Get_Files_By_Name(LOCAL_PATH, ITEM);
                        
                    }
                    // Eliminar archivos repetidos
                    ITEM_LIST = Eliminar_Repetidos(ITEM_LIST);
                    // Cargar archivos a Drive
                    foreach (var FILEPATH in ITEM_LIST)
                    {
                        Upload_Files(SERVICE, FILEPATH, FOLDER_ID);
                        FILES_COUNT++;
                    }
                }
                // Notificacion de cuantos archivos se cargaron, y ejecucion exitosa.
                return $"Se descargaron {FILES_COUNT} archivos \n Ejecucion Exitosa";
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e is AggregateException)
                {
                    throw new Exception("Credenciales/Json no encontradas");
                }
                else if (e is FileNotFoundException)
                {
                    throw new Exception("Archivo no encontrado");
                }
                else
                {
                    throw new Exception($"La carpeta con el ID  { FOLDER_ID }  no existe.");
                }
            }
        }

        // Metodo para subir archivos a drive.
        public static void Upload_Files(DriveService SERVICE, string FILEPATH, string FOLDER_ID)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(FILEPATH),
                Parents = new List<string> { FOLDER_ID }
            };
            FilesResource.CreateMediaUpload request;
            // Crear nuevo archivo en Drive
            using (var stream = new FileStream(FILEPATH,
                       FileMode.Open))
            {
                // Crear nuevo archivo, con su metadata y stream.
                request = SERVICE.Files.Create(
                    fileMetadata, stream, "");
                request.Fields = "id";
                request.Upload();
            }
            
            
            
            
        }

        // Metodo para obtener las credenciales de Google en base el JSON.
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

        // Metodo para generar un servicio del servidor Google.
        public static DriveService Create_Service(GoogleCredential CREDENTIAL)
        {
            var SERVICE = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = CREDENTIAL,
                ApplicationName = "Google Drive Upload"
            });
            return SERVICE;
        }

        // Metodo para crear Metadata de un archivo para drive.
        public static Google.Apis.Drive.v3.Data.File Get_File_Data(string FILEPATH, string FOLDER_ID)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(FILEPATH),
                Parents = new List<string> { FOLDER_ID }
            };
            return fileMetadata;
        }

        // Metodo para obtener archivos de un folder local.
        public static string[] Get_Files_by_Folder(string LOCAL_PATH)
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

        // Metodo para obtener archivos por extension de una carpeta local.
        public static string[] Get_Files_By_Extension(string LOCAL_PATH, string EXTENSION)
        {
            string[] ITEM_LIST = Directory.GetFiles(LOCAL_PATH, "*" + EXTENSION);
            return ITEM_LIST;
        }

        // Metodo para obtener archivos por nombre de una carpeta local.
        public static string[] Get_Files_By_Name(string LOCAL_PATH, string NAME)
        {
            if (!Directory.EnumerateFileSystemEntries(LOCAL_PATH).Any())
            {
                throw new Exception($"La carpeta: { LOCAL_PATH } está vacía");

            }
            string[] ITEM_LIST = Directory.GetFiles(LOCAL_PATH, NAME + "*");
            return ITEM_LIST;

            
        }

        // Metodo para validar los argumentos.
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

        // Metodo para eliminar archivos duplicados.
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