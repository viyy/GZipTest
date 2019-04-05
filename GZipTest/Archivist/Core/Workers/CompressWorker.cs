using System;
using System.Threading;
using GZipTest.Archivist.Interfaces;

namespace GZipTest.Archivist.Core.Workers
{
    /// <summary>
    ///     Рабочий класс для компрессии блоков
    /// </summary>
    public class CompressWorker : IWorker
    {
        public void DoWork()
        {
            try
            {
                //Пробуем взять блок
                var t = Reader.InputChunkHolder.Dequeu();
                //если блока нет
                while (t == null && !(Reader.FileReaded && Writer.FileWrited))
                    //ожидаем блок
                    lock (Reader.InputChunkHolder.EnqueueWaitObject)
                    {
                        //и проверяем, а не выполнили ли мы уже все задания
                        //ждем
                        Monitor.Wait(Reader.InputChunkHolder.EnqueueWaitObject);
                        //Пробуем снова взять блок
                        if (Reader.FileReaded && Writer.FileWrited) return;
                        t = Reader.InputChunkHolder.Dequeu();
                    }
                //сжимаем
                var data = new Compressor().Compress(t.Data);
                //отправляем на запись
                Writer.OutputChunkHolder.Add(t.Id, data);
            }
            catch (Exception e)
            {
                throw new Exception("{CompressWorker} ::" +e.Message);
            }
        }
    }
}