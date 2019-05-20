using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Teatteri
{
    public static class GetVideoFile
    {
        private static string[] extensions = { ".avi", ".riff", ".mpg", ".vob", ".mp4", ".m2ts", ".mov", ".3gp", ".mkv" };
        public static string Get(string path)
        {
            string[] fileList = Directory.GetFiles(path);
            foreach (string file in fileList)
            {
                foreach (string ext in extensions)
                {
                    if (file.Contains(ext))
                    {
                        return file;
                    }
                }
            }
            return null;
        }
    }
}
