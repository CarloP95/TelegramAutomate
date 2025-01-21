using System;

namespace TelegramAutomate.Abstract
{
    public static class TrustedStrings
    {
        public static string SafePath(this string path)
        {
            // Escape invalid characters
            var invalids = System.IO.Path.GetInvalidPathChars();
            var newPath = String.Join("_", path.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

            // Escape up a level or semi column
            if (newPath.Contains("..") || newPath.Contains(";"))
            {
                return string.Empty;
            }

            return newPath;
        }

        public static string SafeFilename(this string path)
        {
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            var basePath = System.IO.Path.GetDirectoryName(path);
            var filename = System.IO.Path.GetFileName(path);
            if (filename.Equals(path))
            {
                return String.Join("_", filename.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            }

            return System.IO.Path.Combine(basePath.SafePath(), String.Join("_", filename.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.'));
        }
    }
}
