namespace GZipTest.Archivist.Context
{
    public static class Constants
    {
        #region Memory we can use

        //Можем использовать всё что есть свыше
        public const float MinMemoryFreeMb = 1024;

        #endregion

        #region GZipDefaultHeader

        //http://www.ietf.org/rfc/rfc1952.txt
        //2.2. File format

        //A gzip file consists of a series of "members" (compressed data
        //sets).  The format of each member is specified in the following
        //section.The members simply appear one after another in the file,
        //    with no additional information before, between, or after them.

        //2.3. Member format


        //Each member has the following structure:

        //+---+---+---+---+---+---+---+---+---+---+
        //|ID1|ID2|CM |FLG|     MTIME     |XFL|OS | (more-->)
        //+---+---+---+---+---+---+---+---+---+---+
        //+=======================+
        //|...compressed blocks...| (more-->)
        //+=======================+

        //  0   1   2   3   4   5   6   7
        //+---+---+---+---+---+---+---+---+
        //|     CRC32     |     ISIZE     |
        //+---+---+---+---+---+---+---+---+
        //                                          ID1    ID2  CM    FLG   MTIME (4 bytes)         XFL   OS
        public static readonly byte[] GZipHeader = {0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00};

        #endregion

        #region Operations

        //комманды для определения режима работы.
        public const string OperationCompressString = "compress";

        public const string OperationDecompressString = "decompress";

        #endregion

        #region Buffer's Sizes

        //Сколько читаем из файла при компрессии
        public const int ChunkSize = 16 * 1024 * 1024;

        //При переносе из стрима в стрим
        public const int ByteBufferSize = 2048 * 1024;

        #endregion
    }
}