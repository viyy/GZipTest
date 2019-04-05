using System;
using System.Diagnostics;
using System.Threading;

namespace GZipTest.Utils
{
    /// <summary>
    ///     Класс для работы со сборщиком мусора
    /// </summary>
    public static class GCUtils
    {
        private static readonly object _lock = new object();
        private static volatile bool _stopRuntimeCheck;

        private static readonly PerformanceCounter RamCounter = new PerformanceCounter("Memory", "Available MBytes");

        public static bool StopRuntimeCheck
        {
            get
            {
                lock (_lock)
                {
                    return _stopRuntimeCheck;
                }
            }
            set
            {
                lock (_lock)
                {
                    _stopRuntimeCheck = value;
                }
            }
        }

        /// <summary>
        ///     Возвращает количество свободной оперативной памяти в Мб
        /// </summary>
        /// <returns></returns>
        public static float FreeMemoryMb()
        {
            return RamCounter.NextValue();
        }

        /// <summary>
        ///     Вызов Сборщика мусора
        /// </summary>
        /// <param name="message">Выводить ли данные о памяти до и после сбора в консоль</param>
        public static void ForceGc(bool message = false)
        {
            if (message)
            {
                Console.Out.WriteLine($"Память до: {GC.GetTotalMemory(false)} байт.");
                Console.Out.WriteLine($"Память после: {GC.GetTotalMemory(true)} байт.");
            }
            else
            {
                GC.GetTotalMemory(true);
            }
        }

        /// <summary>
        ///     Проверяет состояние памяти во время выполнения и вызывает сборщик мусора
        /// </summary>
        /// <param name="minMemory">Минимальный объем памяти, который надо оставлять свободным</param>
        public static void RuntimeCheck(float minMemory)
        {
            while (!StopRuntimeCheck)
            {
                if (FreeMemoryMb() < minMemory)
                    ForceGc();
                Thread.Sleep(100);
            }
        }
    }
}