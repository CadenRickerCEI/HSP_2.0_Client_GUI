namespace HSPGUI.Resources
{
    /// <summary>
    /// The CSVReader class provides functionality to read CSV files and return their contents as a list of string arrays.
    /// </summary>
    public class CSVReader
    {
        /// <summary>
        /// Gets or sets the file path of the CSV file to be read.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CSVReader"/> class with the specified file path.
        /// </summary>
        /// <param name="filePath">The path of the CSV file to be read.</param>
        public CSVReader(string filePath) => FilePath = filePath;

        /// <summary>
        /// Reads the CSV file and returns its contents as a list of string arrays.
        /// Each array represents a row in the CSV file, with each element in the array representing a cell.
        /// </summary>
        /// <returns>
        /// A list of string arrays representing the rows of the CSV file, or null if an IOException occurs.
        /// </returns>
        public List<string[]>? ReadCSV()
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