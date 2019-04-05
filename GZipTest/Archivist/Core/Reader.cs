using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using GZipTest.Archivist.Context;
using GZipTest.Utils;

namespace GZipTest.Archivist.Core
{
    /// <summary>
    ///     Класс для чтения блоков файла и хранения их до обработки
    /// </summary>
    public class Reader
    {
        //Хранилище для считанных блоков
        public static SimpleConcurrentQueue<Chunk> InputChunkHolder = new SimpleConcurrentQueue<Chunk>();

        //Количество считанных блоков
        private static int _chunksreaded;

        private static readonly object _lock = new object();

        //Флаг конца считывания
        private static volatile bool _filereaded;

        //Сколько блоков можем держать в памяти (0 - сколько угодно, до ограничения по оперативной памяти)
        private readonly int _maxChunks;

        /// <summary>
        /// </summary>
        /// <param name="maxChunks">Количество одновременно хранимых блоков (0 - ограничение по RAM)</param>
        public Reader(int maxChunks)
        {
            _maxChunks = maxChunks >= 0 ? maxChunks : 0;
        }

        public static int ChunksReaded
        {
            get
            {
                lock (_lock)
                {
                    return _chunksreaded;
                }
            }
            private set
            {
                lock (_lock)
                {
                    _chunksreaded = value;
                }
            }
        }

        public static bool FileReaded
        {
            get
            {
                lock (_lock)
                {
                    return _filereaded;
                }
            }
            set
            {
                lock (_lock)
                {
                    _filereaded = value;
                }
            }
        }

        /// <summary>
        ///     Начинает процесс чтения файла
        /// </summary>
        /// <param name="mode">В каком режиме работает архиватор</param>
        /// <param name="filepath">Входной файл</param>
        /// <param name="bufferSize">Размер блока, если в режиме сжатия</param>
        public void Read(OperationCode mode, string filepath, int bufferSize)
        {
            switch (mode)
            {
                case OperationCode.Compress:
                    //Console.WriteLine("Compress mode detected");
                    ReadForCompress(filepath, bufferSize);
                    break;
                case OperationCode.Decomress:
                    //Console.WriteLine("Decompress mode detected");
                    ReadForDecompress(filepath);
                    break;
                default:
                    throw new ArgumentException("Incorrect Operation");
            }
        }

        /// <summary>
        ///     Читаем блоки архива
        /// </summary>
        /// <param name="filepath">Входной файл</param>
        private void ReadForDecompress(string filepath)
        {
            using (var reader = new BinaryReader(File.Open(filepath, FileMode.Open, FileAccess.Read)))
            {
                var gzipHeader = Constants.GZipHeader;

                var fileLength = new FileInfo(filepath).Length;
                var availableBytes = fileLength;
                while (availableBytes > 0)
                {
                    while (GCUtils.FreeMemoryMb() < Constants.MinMemoryFreeMb)
                        //Console.WriteLine(GCUtils.FreeMemoryMb() + "Mb left!");
                        GCUtils.ForceGc();
                    var gzipBlock = new List<byte>();
                    //Console.WriteLine($"Reading #{ChunksReaded}");
                    // GZip Заголовок
                    if (ChunksReaded == 0) // Получаем первый заголовок в файле. В остальных блоках - такой же
                    {
                        gzipHeader = reader.ReadBytes(gzipHeader.Length);
                        availableBytes -= gzipHeader.Length;
                    }
                    gzipBlock.AddRange(gzipHeader); //добавляем хидер в блок

                    // Данные блока архива
                    var gzipHeaderMatchsCount = 0;
                    while (availableBytes > 0)
                    {
                        while (_maxChunks != 0 && InputChunkHolder.Count >= _maxChunks)
                            lock (InputChunkHolder.DequeueWaitObject)
                            {
                                //Console.WriteLine("Waiting for Dq");
                                Monitor.Wait(InputChunkHolder.DequeueWaitObject);
                            }

                        var curByte = reader.ReadByte();
                        gzipBlock.Add(curByte);
                        availableBytes--;

                        // Проверяем заголовок следующего блока
                        if (curByte == gzipHeader[gzipHeaderMatchsCount])
                        {
                            gzipHeaderMatchsCount++;
                            if (gzipHeaderMatchsCount != gzipHeader.Length)
                                continue;

                            gzipBlock.RemoveRange(gzipBlock.Count - gzipHeader.Length,
                                gzipHeader.Length); // Убираем заголовок следующего блока из текущего
                            break;
                        }

                        gzipHeaderMatchsCount = 0;
                    }
                    //отправляем блок на обработку
                    InputChunkHolder.Enqueu(new Chunk(ChunksReaded, gzipBlock.ToArray()));
                    //номер блока ++
                    ChunksReaded++;
                    //Console.WriteLine($"Readed #{ChunksReaded - 1}");
                }
            }
            FileReaded = true;
            //Console.WriteLine($"Total chunks {ChunksReaded - 1}");
        }

        /// <summary>
        ///     Читаем блоки файла для архивации
        /// </summary>
        /// <param name="filepath">Входной файл</param>
        /// <param name="bufferSize">Размер блока</param>
        private void ReadForCompress(string filepath, int bufferSize)
        {
            var fileLength = new FileInfo(filepath).Length;
            var availableBytes = fileLength;
            while (availableBytes > 0)
            {
                while (_maxChunks != 0 && InputChunkHolder.Count >= _maxChunks)
                    lock (InputChunkHolder.DequeueWaitObject)
                    {
                        Monitor.Wait(InputChunkHolder.DequeueWaitObject);
                    }

                while (GCUtils.FreeMemoryMb() < Constants.MinMemoryFreeMb)
                    //Console.WriteLine(GCUtils.FreeMemoryMb()+"Mb left!");
                    GCUtils.ForceGc();
                //сколько читать
                var readCount = availableBytes < bufferSize
                    ? (int) availableBytes
                    : bufferSize;
                using (var reader =
                    new BinaryReader(File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    reader.BaseStream.Seek(fileLength - availableBytes, SeekOrigin.Begin);
                    var bytes = reader.ReadBytes(readCount);
                    InputChunkHolder.Enqueu(new Chunk(ChunksReaded, bytes));
                }
                //двигаем счётчики
                availableBytes -= readCount;
                ChunksReaded++;
                //Console.WriteLine($"Readed #{ChunksReaded-1}");
            }
            FileReaded = true;
            //Console.WriteLine($"Total chunks {ChunksReaded-1}");
        }
    }
}