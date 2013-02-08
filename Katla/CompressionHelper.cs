using System;
using System.IO.Compression;
using System.Text;
using System.IO;

namespace zanders3.Katla
{
    public class CompressionHelper
    {
        public static string CompressFolderToString(string directory)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (GZipStream compress = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    using (BinaryWriter stream = new BinaryWriter(compress, Encoding.UTF8))
                    {
                        foreach (string file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
                        {
                            stream.Write(file.Substring(directory.Length+1));
                            byte[] contents = File.ReadAllBytes(file);
                            stream.Write(contents.Length);
                            stream.Write(contents);

                            Console.WriteLine(file);
                        }
                        stream.Write(string.Empty);
                    }
                }

                return Convert.ToBase64String(outputStream.GetBuffer());
            }
        }

        public static void ExtractFolderFromString(Action<string> logMessage, string compressedData, string targetDirectory)
        {
            byte[] buffer = Convert.FromBase64String(compressedData);
            using (MemoryStream inputStream = new MemoryStream(buffer))
            {
                using (GZipStream decompress = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    using (BinaryReader reader = new BinaryReader(decompress, Encoding.UTF8))
                    {
                        string fileName = reader.ReadString();
                        while (fileName.Length > 0)
                        {
                            string filePath = Path.Combine(targetDirectory, fileName);
                            logMessage(filePath);
                            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                                Directory.CreateDirectory(filePath);

                            int fileLen = reader.ReadInt32();
                            byte[] fileContent = reader.ReadBytes(fileLen);
                            File.WriteAllBytes(filePath, fileContent);

                            fileName = reader.ReadString();
                        }
                    }
                }
            }
        }
    }
}

