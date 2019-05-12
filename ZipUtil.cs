using Ionic.Zip;
using System.IO;

namespace UtaFormatix
{
    internal static class ZipUtil
    {
        public static string Unzip(string fileName)
        {
            using (var zip = ZipFile.Read(fileName))
            {
                var path = Path.ChangeExtension(fileName, "");
                Directory.CreateDirectory(path);
                zip.ExtractAll(path);
                return path;
            }
        }

        public static void ZipVpr(string targetFileName, string jsonFileName)
        {
            using (var zip = new ZipFile())
            {
                zip.AddFile(jsonFileName, "Project");
                zip.Save(targetFileName);
            }
        }
    }
}
