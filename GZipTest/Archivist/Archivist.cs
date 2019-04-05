using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using GZipTest.Archivist.Context;
using GZipTest.Archivist.Core;
using GZipTest.Archivist.Core.Workers;
using GZipTest.Archivist.Interfaces;
using GZipTest.Utils;

namespace GZipTest.Archivist
{
    public class ArchivistApp : IApplication
    {
        private readonly object _lock = new object();

        private readonly List<Thread> _pool = new List<Thread>();
        private readonly int _processorCount = Environment.ProcessorCount; //определяем количество рабочих потоков

        private bool _isTerminated; //флаг прерывания
        private Parameters _parameters; //параметры работы

        private Thread _readerThread;
        private Thread _writerThread;
        private Thread _memoryCheckThread;

        public ArchivistApp(Parameters args)
        {
            if (args.Operation != OperationCode.Compress && args.Operation != OperationCode.Decomress)
                throw new ArgumentException(
                    $"Возможно использовать только следующие режимы: {Constants.OperationCompressString}/{Constants.OperationDecompressString}");
            _parameters = args;
        }

        /// <summary>
        ///     Начинает процесс обработки файла
        /// </summary>
        public void Run()
        {
            var sw = new Stopwatch();
            sw.Start();
            //Создаем поток чтения
            var r = new Reader(0);
            _readerThread = new Thread(() => r.Read(_parameters.Operation, _parameters.InputFile, Constants.ChunkSize))
            {
                Name = "ReaderThread",
                Priority = ThreadPriority.AboveNormal
            };
            //Создаем поток записи
            var w = new Writer();
            _writerThread = new Thread(() => w.Write(_parameters.OutputFile))
            {
                Name = "WriterThread",
                Priority = ThreadPriority.AboveNormal
            };

            _memoryCheckThread = new Thread(() => GCUtils.RuntimeCheck(Constants.MinMemoryFreeMb));
            _memoryCheckThread.Start();
            //Запускаем чтение и запись
            _readerThread.Start();
            _writerThread.Start();
            //Console.WriteLine("Reader and Writer Started");
            var totalTasks = 0;
            //Пока все не считано и не записано
            while (!Writer.FileWrited || !Reader.FileReaded)
            {
                //проверяем на прерывание
                if (_isTerminated)
                {
                    lock (_lock)
                    {
                        for (var i = _pool.Count - 1; i >= 0; i--)
                        {
                            if (!_pool[i].IsAlive) _pool[i].Abort();
                            _pool.RemoveAt(i);
                        }
                    }
                    break;
                }
                //Если весь пул занят
                lock (_lock)
                {
                    if (_pool.Count == _processorCount)
                    {
                        //Проверяем, все ли потоки работают
                        if (_pool.All(th => th.IsAlive)) continue;
                        //Если не все, то убираем отработавшие
                        for (var i = _pool.Count - 1; i >= 0; i--)
                            if (!_pool[i].IsAlive) _pool.RemoveAt(i);
                    }
                }
                //Когда есть свободные места, создаем новый поток
                IWorker worker;
                if (_parameters.Operation == OperationCode.Compress)
                    worker = new CompressWorker();
                else
                    worker = new DecompressWorker();
                totalTasks++;
                var thr = new Thread(() => worker.DoWork())
                {
                    Name = "Worker" + totalTasks,
                    Priority = ThreadPriority.AboveNormal
                };
                lock (_lock)
                {
                    _pool.Add(thr);
                }
                thr.Start();
            }
            sw.Stop();
            MessageSystem.AddMessage(!_isTerminated
                ? $"Операция выполнена за {sw.Elapsed.TotalSeconds:F2} секунд"
                : $"Операция прервана после {sw.Elapsed.TotalSeconds:F2} секунд");
        }

        public void Exit()
        {
            _isTerminated = true;
            if (_readerThread.IsAlive) _readerThread.Abort();
            if (_writerThread.IsAlive) _writerThread.Abort();
            if (_memoryCheckThread.IsAlive)
            {
                GCUtils.StopRuntimeCheck = true;
                _memoryCheckThread.Abort();
            }
            lock (_lock)
            {
                for (var i = _pool.Count - 1; i >= 0; i--)
                    if (!_pool[i].IsAlive) _pool[i].Abort();
                _pool.Clear();
            }
            Reader.InputChunkHolder.Clear();
            Writer.OutputChunkHolder.Clear();
            GCUtils.ForceGc();
        }
    }
}