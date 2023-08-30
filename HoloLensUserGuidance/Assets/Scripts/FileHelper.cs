using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class FileHelper
    {
        public static void WriteToFile(string directory, string fileName, string Data)
        {
            System.IO.File.WriteAllText(directory + @"\" + fileName, Data);
        }
    }
}
