using System;
using System.Threading;
using GZipTest.Archivist.Interfaces;

namespace GZipTest.Archivist.Core.Workers
{
    /// <summary>
    ///     Рабочий класс для декомпрессии блоков
    /// </summary>
    public class DecompressWorker : IWorker
    {
        public void DoWork()
        {
            try
            {
                //Пробуем взять блок
                var t = Reader.InputChunkHolder.Dequeu();
                //если блока нет
                while (t == null && ! (Reader.FileReaded && Writer.FileWrited))
                    //ожидаем блок
                    lock (Reader.InputChunkHolder.EnqueueWaitObject)
                    {
                        //и проверяем, а не выполнили ли мы уже все задания

                        //ждем
                        Monitor.Wait(Reader.InputChunkHolder.EnqueueWaitObject);
                        if (Reader.FileReaded && Writer.FileWrited) return;
                        //Пробуем снова взять блок
                        t = Reader.InputChunkHolder.Dequeu();
                    }
                //разжимаем
                var data = new Compressor().Decompress(t.Data);
                //отправляем на запись
                Writer.OutputChunkHolder.Add(t.Id, data);
            }
            catch (Exception e)
            {
                throw new Exception("{DecompressWorker} ::" + e.Message);
            }
        }
    }
}