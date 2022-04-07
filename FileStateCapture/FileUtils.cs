using System.IO;

namespace FileStateCapture {
    public static class FileUtils {
        // Why microsoft... "GetDirectoryName" returning the directory's parent is a bit odd

        /// <summary>
        /// Gets the directory in which the given directory is stored in
        /// </summary>
        public static string GetDirectoryParent(string path) => Path.GetDirectoryName(path);

        /// <summary>
        /// Gets the name of the directory
        /// </summary>
        public static string GetDirectoryName(string path) => Path.GetFileName(path);

        /// <summary>
        /// Gets the path of the directory which this file is stored in
        /// </summary>
        public static string GetFileDirectory(string path) => Path.GetDirectoryName(path);

        /// <summary>
        /// Gets the name of the given file (including the extension)
        /// </summary>
        public static string getFileName(string path) => Path.GetFileName(path);

        /// <summary>
        /// Gets the name of the given file (without the extension)
        /// </summary>
        public static string getFileNameNoExt(string path) => Path.GetFileNameWithoutExtension(path);
    }
}