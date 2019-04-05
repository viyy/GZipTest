using System;
using System.IO;
using System.Linq;
using GZipTest.Archivist;
using GZipTest.Archivist.Context;
using GZipTest.Archivist.Interfaces;
using GZipTest.Utils;

namespace GZipTest
{
    internal class Program
    {
        private static IApplication _app;

        private static void Main(string[] args)
        {
#if DEBUG
            args = new[] {"compress", "inp.zip", "out.zip.gz"};
            File.Delete("out.zip.gz");
#endif
//#if DEBUG
//            args = new[] { "decompress", "Backups.zip.gz", "BackupsOut.zip" };
//            File.Delete("BackupsOut.zip");
//#endif
            AddUnhandledExceptionHandler();
            AddCancelKeyHandler();

            try
            {
                var par = Parameters.Parse(args);
                _app = new ArchivistApp(par);
                _app.Run();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                _app?.Exit();
#if DEBUG
                Console.ReadKey();
#endif
                Environment.Exit(1);
            }
            MessageSystem.GetMessages().ToList().ForEach(Console.WriteLine);
            _app.Exit();
#if DEBUG
            Console.ReadKey();
#endif
        }

        private static void AddCancelKeyHandler()
        {
            Console.CancelKeyPress += (o, e) =>
            {
                _app.Exit();
                e.Cancel = true;
            };
        }

        private static void AddUnhandledExceptionHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
            {
                Console.Error.WriteLine(e);
                _app.Exit();
            };
        }
    }
}