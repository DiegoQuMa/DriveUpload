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
        /* Metodo para cargar archivos a drive.
            FolderName = Nombre de la carpeta en Drive.
            JsonPath = Ruta del archivo JSON.
            ItemList = Array para nombres de archivos CON EXTENSION.
            LocalPath = Ruta de la carpeta local.
        */

        public string Carga_De_Archivos(string FolderName, string JsonPath, string[] ItemList, string LocalPath)
        {
            // Validar si los argumentos estan completos.
            Validar_Arguments(FolderName, JsonPath, LocalPath);
            string FilesLog=null;

            try
            {
                // Eliminar los archivos repetidos.
                ItemList = Eliminar_Repetidos(ItemList);
                
                // Obtener Credenciales con el archivo JSON.
                GoogleCredential Credential = Get_Credentials(JsonPath);

                // Crear un servicio de Drive.
                var Service = Create_Service(Credential,FolderName);
                FolderName = Get_Drive_Id(Service, FolderName);

                
                int FilesCount = 0;
                
                // Cargar archivos por ruta de Folder
                if (ItemList.Length < 1)
                {

                    // Obtener archivos por ruta de folder
                    ItemList = Get_Files_by_Folder(LocalPath);

                    // Eliminar archivos repetidos
                    ItemList = Eliminar_Repetidos(ItemList);

                    // Cargar archivos a drive.
                    foreach (var FilePath in ItemList)
                    {
                        Upload_Files(Service, FilePath, FolderName);
                        FilesCount++;
                    }

                }
                else
                {
                    foreach (var Item in ItemList)
                    {
                        // Obtener archivos por extension.
                        if (Item.StartsWith("."))
                        {

                            ItemList = Get_Files_By_Extension(LocalPath, Item);
                            FilesLog = Check_Item(LocalPath, Item, FilesLog, true);

                        }

                        // Obtener archivos por nombre
                        else
                        {

                            ItemList = Get_Files_By_Name(LocalPath, Item);
                            FilesLog = Check_Item(LocalPath, Item, FilesLog, false);


                        }

                        // Eliminar archivos repetidos
                        ItemList = Eliminar_Repetidos(ItemList);

                        // Cargar archivos a Drive
                        foreach (var FilePath in ItemList)
                        {
                            Upload_Files(Service, FilePath, FolderName);
                            FilesCount++;
                        }
                    }
                }

                
                // Notificacion de cuantos archivos se cargaron, y ejecucion exitosa.
                return $"Se cargaron {FilesCount} archivos. \n{FilesLog} \nEjecucion Exitosa";

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e is AggregateException)
                {
                    throw new Exception("Credenciales/Json no encontradas");
                }
                else if (e is DirectoryNotFoundException)
                {
                    throw new Exception($"La ruta de la carpeta {LocalPath}  no existe.");
                }
                else if (e is FileNotFoundException)
                {
                    throw new Exception($"Archivo no encontrado. {FilesLog}");
                }
                else
                {
                    throw;
                }
            }
        }
        // Metodo para subir archivos a drive.
        private static void Upload_Files(DriveService Service, string FilePath, string FolderName)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(FilePath),
                Parents = new List<string> { FolderName }
            };
            FilesResource.CreateMediaUpload request;
            // Crear nuevo archivo en Drive
            using (var stream = new FileStream(FilePath,
                       FileMode.Open))
            {
                // Crear nuevo archivo, con su metadata y stream.
                request = Service.Files.Create(
                    fileMetadata, stream, "");
                request.Fields = "id";
                request.Upload();
            }




        }
        // Metodo para obtener las credenciales de Google en base el JSON.
        private static GoogleCredential Get_Credentials(string JsonPath)
        {
            GoogleCredential Credential;
            using (var stream = new FileStream(JsonPath, FileMode.Open, FileAccess.Read))
            {
                Credential = GoogleCredential.FromStream(stream).CreateScoped(DriveService.ScopeConstants.Drive);
            }
            return Credential;
        }

        // Metodo para generar un servicio del servidor Google.
        private static DriveService Create_Service(GoogleCredential Credential,string FolderName)
        {
            var Service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = Credential,
                ApplicationName = "Google Drive Upload"
            });

            return Service;
        }

        
        // Metodo para crear Metadata de un archivo para drive.
        private static Google.Apis.Drive.v3.Data.File Get_File_Data(string FilePath, string FolderName)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(FilePath),
                Parents = new List<string> { FolderName }
            };
            return fileMetadata;
        }
        // Metodo para obtener archivos de un folder local.
        private static string[] Get_Files_by_Folder(string LocalPath)
        {

            
                if (!Directory.EnumerateFileSystemEntries(LocalPath).Any())
                {
                    throw new Exception($"La carpeta: {LocalPath} está vacía");
                }
                string[] ItemList = Directory.GetFiles(LocalPath);
                return ItemList;
            
            
        }
        // Metodo para obtener archivos por extension de una carpeta local.
        private static string[] Get_Files_By_Extension(string LocalPath, string EXTENSION)
        {
            string[] ItemList = Directory.GetFiles(LocalPath, "*" + EXTENSION);

            
            return ItemList;
        }
        // Metodo para obtener archivos por nombre de una carpeta local.
        private static string[] Get_Files_By_Name(string LocalPath, string Name)
        {
            if (!Directory.EnumerateFileSystemEntries(LocalPath).Any())
            {
                throw new Exception($"La carpeta: {LocalPath} está vacía");
            }
            string[] ItemList = Directory.GetFiles(LocalPath, Name);

            
            return ItemList;

        }
        // Metodo para validar los argumentos.
        private static string Validar_Arguments(string FolderName, string JsonPath, string LocalPath)
        {
            string[] args = { FolderName, JsonPath, LocalPath };
            for (int i = 0; i < 3; i++)
            {
                if (string.IsNullOrEmpty(args[i]))
                {
                    throw new ArgumentException("Los argumentos Requeridos son: FolderName, JsonPath y LocalPath");
                }
            }
            return null;
        }
        // Metodo para eliminar archivos duplicados.
        static T[] Eliminar_Repetidos<T>(T[] ItemList)
        {
            List<T> listaSinRepetidos = new List<T>();
            HashSet<T> conjunto = new HashSet<T>();
            foreach (var elemento in ItemList)
            {
                if (conjunto.Add(elemento))
                {
                    listaSinRepetidos.Add(elemento);
                }
            }
            return listaSinRepetidos.ToArray();
        }

        // Metodo para verificar la existencia de la carpeta en drive y obtener su ID para la carga.
        private static string Get_Drive_Id(DriveService Service, string FolderName)
        {
            var Request = Service.Files.List();

            Request.Q = $"name = '{FolderName}' and mimeType = 'application/vnd.google-apps.folder'";
            var Result = Request.Execute();
            var Folder = Result.Files.FirstOrDefault();
            if (Folder != null)
            {
                return Folder.Id;
                
            }
            else
            {
                throw new ArgumentException($"La carpeta  {FolderName}  no existe en Drive.");
            }

        }

        // Funcion para notificar si el archivo que se pidio, se encontro localmente.
        private static string Check_Item(string LocalPath,string Item,string FilesLog,bool ext)
        {
            if(ext == true)
            {
                if (!Directory.GetFiles(LocalPath, "*" + Item).Any())
                {
                    if (FilesLog == null)
                    {
                        FilesLog = "No se encontraron los siguientes archivos o extensiones: " + "\n\t" + Item;
                    }
                    else
                    {
                        FilesLog = FilesLog + "\n\t" + Item;
                    }
                }
            }

            else
            {
                if (!Directory.GetFiles(LocalPath, Item).Any())
                {
                    if (FilesLog == null)
                    {
                        FilesLog = "No se encontraron los siguientes archivos o extensiones: " + "\n\t" + Item;
                    }
                    else
                    {
                        FilesLog = FilesLog + "\n\t" + Item;
                    }
                }
            }
            
            return FilesLog;
        }

        

        

    }
}