using System.Collections.Generic;
using System.Threading;

namespace GZipTest.Utils
{
    /// <summary>
    ///     Словарь для работы в многопоточном режиме с механизмом сигналов
    /// </summary>
    /// <typeparam name="T1">Тип ключа</typeparam>
    /// <typeparam name="T2">Тип данных</typeparam>
    public class ArchivistConcurrentDictionary<T1, T2>
    {
        private readonly Dictionary<T1, T2> _dict = new Dictionary<T1, T2>();
        private readonly object _lock = new object();

        /// <summary>
        ///     Объект для сигнала добавления (Monitor.Wait(DictVar.AddWaitObject))
        /// </summary>
        public readonly object AddWaitObject = new object();

        /// <summary>
        ///     Добавляет запись в словарь
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        public void Add(T1 key, T2 value)
        {
            lock (_lock)
            {
                lock (AddWaitObject)
                {
                    _dict.Add(key, value);
                    Monitor.PulseAll(AddWaitObject);
                }
            }
        }

        /// <summary>
        ///     Добавляет запись в словарь
        /// </summary>
        /// <param name="pair">Пара Ключ-Значение</param>
        public void Add(KeyValuePair<T1, T2> pair)
        {
            lock (_lock)
            {
                lock (AddWaitObject)
                {
                    _dict.Add(pair.Key, pair.Value);
                    Monitor.PulseAll(AddWaitObject);
                }
            }
        }

        /// <summary>
        ///     Проверяет наличие ключа
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns></returns>
        public bool ContainsKey(T1 key)
        {
            lock (_lock)
            {
                return _dict.ContainsKey(key);
            }
        }

        /// <summary>
        ///     Пробует забрать значение из словаря и удалить запись из словаря, если ключ не найден, возвращает дефолтное значение
        ///     типа данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Данные</param>
        /// <returns>Получилось ли забрать данные</returns>
        public bool TryTakeValue(T1 key, out T2 value)
        {
            lock (AddWaitObject)
            lock (_lock)
            {
                if (ContainsKey(key))
                {
                    value = _dict[key];
                    _dict.Remove(key);
                    return true;
                }
                value = default(T2);
                return false;
            }
        }

        /// <summary>
        ///     Удаляет запись из словаря
        /// </summary>
        /// <param name="key">Ключ</param>
        public void Remove(T1 key)
        {
            lock (_lock)
            {
                _dict.Remove(key);
            }
        }

        /// <summary>
        ///     Очищает словарь
        /// </summary>
        public void Clear()
        {
            lock (AddWaitObject)
            {
                lock (_lock)
                {
                    _dict.Clear();
                    Monitor.PulseAll(AddWaitObject);
                }
            }
        }
    }
}