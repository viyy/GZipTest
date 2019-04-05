using System.Collections.Generic;

namespace GZipTest.Archivist.Core
{
    /// <summary>
    ///     Класс для хранения блока файла с его номером
    /// </summary>
    public class Chunk
    {
        /// <summary>
        ///     Возвращает экземпляр класса для хранения блока файла с его номером
        /// </summary>
        /// <param name="id">Номер блока</param>
        /// <param name="data">Данные блока</param>
        public Chunk(int id, byte[] data)
        {
            Id = id;
            Data = data;
        }

        public int Id { get; }
        public byte[] Data { get; }

        public KeyValuePair<int, byte[]> ToKeyValuePair()
        {
            return new KeyValuePair<int, byte[]>(Id, Data);
        }
    }
}