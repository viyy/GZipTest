namespace GZipTest.Archivist.Context
{
    public enum OperationCode
    {
        None,
        Compress,
        Decomress
    }

    public static class OperationParser
    {
        /// <summary>
        ///     Преобразует строковый параметр в OperationCode
        /// </summary>
        /// <param name="arg">Строка, определяющая режим</param>
        /// <returns>Режим работы</returns>
        public static OperationCode Parse(string arg)
        {
            switch (arg.ToLowerInvariant())
            {
                case Constants.OperationCompressString:
                    return OperationCode.Compress;
                case Constants.OperationDecompressString:
                    return OperationCode.Decomress;
                default:
                    return OperationCode.None;
            }
        }
    }
}