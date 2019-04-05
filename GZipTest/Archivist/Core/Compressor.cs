using System;
using System.IO;
using System.IO.Compression;
using GZipTest.Archivist.Context;

namespace GZipTest.Archivist.Core
{
    /// <summary>
    ///     Класс для компрессии и декомпрессии блоков
    /// </summary>
    public class Compressor
    {
        /// <summary>
        ///     Компрессим блок данных
        /// </summary>
        /// <param name="originalBytes">Исходный блок</param>
        /// <returns>Сжатый блок</returns>
        public byte[] Compress(byte[] originalBytes)
        {
            try
            {
                using (var output = new MemoryStream())
                {
                    using (var compressStream = new GZipStream(output, CompressionMode.Compress))
                    {
                        compressStream.Write(originalBytes, 0, originalBytes.Length);
                    }

                    return output.ToArray();
                }
            }
            catch (Exception e)
            {
                throw new Exception("{Compressor->Compress} ::" + e.Message);
            }
        }

        /// <summary>
        ///     Разжимаем блок
        /// </summary>
        /// <param name="compressedBytes">Сжатый блок</param>
        /// <returns>Разжатый блок</returns>
        public byte[] Decompress(byte[] compressedBytes)
        {
            try
            {
                using (var output = new MemoryStream())
                {
                    using (var input = new MemoryStream(compressedBytes))
                    {
                        using (var decompressStream = new GZipStream(input, CompressionMode.Decompress))
                        {
                            var buffer = new byte[Constants.ByteBufferSize];
                            int bytesRead;
                            while ((bytesRead = decompressStream.Read(buffer, 0, buffer.Length)) > 0)
                                output.Write(buffer, 0, bytesRead);
                        }
                        return output.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("{Compressor->Decompress} ::" + e.Message);
            }
        }
    }
}