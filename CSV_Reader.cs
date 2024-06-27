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
            try
            {
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
            catch (IOException)
            {
                return null;
            }


        }
    }
}
