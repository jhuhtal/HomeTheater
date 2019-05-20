using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Teatteri
{                      
    public class LogWriter
    {
        private StringBuilder sb = new StringBuilder();

        public void Write(string line)
        {
            sb.Append(line+System.Environment.NewLine);
        }
        public void CreateLogFile(string filepath)
        {
            using (var sw = new StreamWriter(filepath, true))
            {
                sw.Write(sb.ToString());
            }
        }
    }
}
