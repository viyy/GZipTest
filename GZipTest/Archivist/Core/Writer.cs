using System;
using System.IO;
using System.Threading;
using GZipTest.Utils;

namespace GZipTest.Archivist.Core
{
    /// <summary>
    ///     Класс для записи блоков файла после обработки
    /// </summary>
    public class Writer
    {
        /// <summary>
        ///     Хранилище блоков для записи
        /// </summary>
        public static ArchivistConcurrentDictionary<int, byte[]> OutputChunkHolder =
            new ArchivistConcurrentDictionary<int, byte[]>();

        private static readonly object _lock = new object();
        private static volatile bool _fileWrited;
        private static volatile int _nextWriteChunk; //счётчик блоков, так как пишем файл последовательно

        public static bool FileWrited
        {
            get
            {
                lock (_lock)
                {
                    return _fileWrited;
                }
            }
            set
            {
                lock (_lock)
                {
                    _fileWrited = value;
                }
            }
        }

        /// <summary>
        ///     Начинаем процесс записи
        /// </summary>
        /// <param name="filepath">Путь к выходному файлу</param>
        public void Write(string filepath)
        {
            //Пока файл не считан полностью или мы не записали все блоки
            while (!Reader.FileReaded || _nextWriteChunk < Reader.ChunksReaded)
            {
                using (var writer =
                    new BinaryWriter(File.Open(filepath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)))
                {
                    writer.BaseStream.Seek(0, SeekOrigin.End); //пишем в конец
                    //ждем следующий блок
                    while (!OutputChunkHolder.ContainsKey(_nextWriteChunk))
                        lock (OutputChunkHolder.AddWaitObject)
                        {
                            //Console.WriteLine($"Waiting #{_nextWriteChunk}");
                            Monitor.Wait(OutputChunkHolder.AddWaitObject);
                        }
                    if (OutputChunkHolder.TryTakeValue(_nextWriteChunk, out var bytes))
                        writer.Write(bytes);
                    else
                        throw new Exception("Can not retrieve chunk #" + _nextWriteChunk);
                }
                _nextWriteChunk++;
                //Console.WriteLine("Writed #"+(_nextWriteChunk-1));
            }
            FileWrited = true;
        }
    }
}