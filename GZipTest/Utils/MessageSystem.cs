using System.Collections.Generic;

namespace GZipTest.Utils
{
    /// <summary>
    ///     Позволяет обмениваться сообщениями между потоками, ядром и пользовательским интерфейсом и тому подобное...
    /// </summary>
    public static class MessageSystem
    {
        private static readonly Queue<string> Messages = new Queue<string>();
        private static readonly object Locker = new object();

        /// <summary>
        ///     Добавляет сообщение в очередь
        /// </summary>
        /// <param name="text">Текст сообщения</param>
        public static void AddMessage(string text)
        {
            lock (Locker)
            {
                Messages.Enqueue(text);
            }
        }

        /// <summary>
        ///     Возвращает первое сообщение в очереди или null, если очередь пуста
        /// </summary>
        /// <returns>Текст сообщения или null</returns>
        public static string GetMessage()
        {
            lock (Locker)
            {
                return Messages.Count > 0 ? Messages.Dequeue() : null;
            }
        }

        /// <summary>
        ///     Возвращает все сообщения в очереди
        /// </summary>
        /// <returns>Тексты сообщений</returns>
        public static string[] GetMessages()
        {
            lock (Locker)
            {
                var res = new List<string>();
                while (Messages.Count > 0)
                    res.Add(Messages.Dequeue());
                return res.ToArray();
            }
        }
    }
}