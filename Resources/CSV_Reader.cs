using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Collections.Generic;

namespace HSPGUI.Resources
{
    
    public class CSVReader
    {
        public string FilePath { get; set; }

        public CSVReader(string filePath)
        {
            FilePath = filePath;
        }

        public List<string[]> ReadCSV()
        {
            var rows = new List<string[]>();

            using (var reader = new StreamReader(FilePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line != null)
                    {
                        var values = line.Split(',');
                        rows.Add(values);
                    }
                }
            }
            return rows;
        }
    }
}
