using System;
using System.IO;

namespace GZipTest.Archivist.Context
{
    public struct Parameters
    {
        public OperationCode Operation { get; }
        public string InputFile { get; }
        public string OutputFile { get; }

        private Parameters(OperationCode oc, string input, string output)
        {
            Operation = oc;
            InputFile = input;
            OutputFile = output;
        }

        /// <summary>
        ///     Распознает входные аргументы.
        ///     Ожидается три аргумента:
        ///     1. режим работы (по умолчанию compress/decompress)
        ///     2. Входной файл
        ///     3. Выходной файл
        /// </summary>
        /// <param name="args">Массив строковых аргументов</param>
        /// <returns>Параметры работы</returns>
        public static Parameters Parse(string[] args)
        {
            if (args == null || args.Length < 3)
                throw new ArgumentException(
                    $"Необходимо 3 параметра: режим {Constants.OperationCompressString}/{Constants.OperationDecompressString}, имя входного файла, имя выходного файла");
            var operation = OperationParser.Parse(args[0]);
            if (operation == OperationCode.None)
                throw new ArgumentException(
                    $"Возможные режимы работы: {Constants.OperationCompressString}/{Constants.OperationDecompressString}");
            var inpPath = args[1];
            var ipa = Path.GetDirectoryName(inpPath);
            if (ipa != null && ipa.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                throw new ArgumentException($"Неверный путь к файлу: {inpPath}");
            var ifi = Path.GetFileName(inpPath);
            if (ifi != null && ifi.IndexOfAny(Path.GetInvalidFileNameChars())!=-1)
                throw new ArgumentException($"{inpPath} не является легальным именем файла.");
            if (!File.Exists(inpPath))
                throw new ArgumentException($"{inpPath} не существует.");
            var outPath = args[2];
            var opa = Path.GetDirectoryName(outPath);
            if (opa != null && opa.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                throw new ArgumentException($"Неверный путь к файлу: {outPath}");
            var ofi = Path.GetFileName(outPath);
            if (ofi != null && ofi.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                throw new ArgumentException($"{outPath} не является легальным именем файла.");
            if (File.Exists(outPath))
                throw new Exception($"{outPath} уже существует");
            return new Parameters(operation, inpPath, outPath);
        }
    }
}