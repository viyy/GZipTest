using System.Collections.Generic;
using System.Threading;

namespace GZipTest.Utils
{
    /// <summary>
    ///     Очередь для многопоточной работы
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    public class SimpleConcurrentQueue<T>
    {
        private readonly object _lock = new object();
        private readonly Queue<T> _queue = new Queue<T>();

        /// <summary>
        ///     Объект для ожидания взятия из очереди
        /// </summary>
        public object DequeueWaitObject = new object();

        /// <summary>
        ///     Объект для ожидания добавления в очередь
        /// </summary>
        public object EnqueueWaitObject = new object();

        /// <summary>
        ///     Количество элементво в очереди
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        /// <summary>
        ///     Добавляет элемент в очередь
        /// </summary>
        /// <param name="item">Добавляемый элемент</param>
        public void Enqueu(T item)
        {
            lock (_lock)
            {
                lock (DequeueWaitObject)
                lock (EnqueueWaitObject)
                {
                    _queue.Enqueue(item);
                    Monitor.PulseAll(EnqueueWaitObject);
                }
            }
        }

        /// <summary>
        ///     Берет элемент из очереди, или возвращает дефолтное значение типа
        /// </summary>
        /// <returns></returns>
        public T Dequeu()
        {
            lock (_lock)
            {
                lock (EnqueueWaitObject)
                lock (DequeueWaitObject)
                {
                    var t = _queue.Count > 0 ? _queue.Dequeue() : default(T);
                    Monitor.PulseAll(DequeueWaitObject);
                    return t;
                }
            }
        }

        /// <summary>
        ///     Очищает очередь
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _queue.Clear();
                lock (EnqueueWaitObject)
                {
                    Monitor.PulseAll(EnqueueWaitObject);
                }
                lock (DequeueWaitObject)
                {
                    Monitor.PulseAll(DequeueWaitObject);
                }
                
            }
        }
    }
}