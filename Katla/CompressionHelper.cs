using System;
using System.IO.Compression;
using System.Text;
using System.IO;

namespace zanders3.Katla
{
    public class CompressionHelper
    {
        public static byte[] CompressFolderToBytes(string directory)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (GZipStream compress = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    using (BinaryWriter stream = new BinaryWriter(compress, Encoding.UTF8))
                    {
                        foreach (string file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
                        {
                            stream.Write(file.Substring(directory.Length + 1));
                            byte[] contents = File.ReadAllBytes(file);
                            stream.Write(contents.Length);
                            stream.Write(contents);

                            Console.WriteLine(file);
                        }
                        stream.Write(string.Empty);
                    }
                }

                return outputStream.GetBuffer();
            }
        }

        public static void ExtractFolderFromStream(Action<string> logMessage, byte[] input, string targetDirectory)
        {
            using (MemoryStream inputStream = new MemoryStream(input, 0, input.Length))
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

