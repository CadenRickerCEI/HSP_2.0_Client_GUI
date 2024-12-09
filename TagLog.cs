using System.Text;

namespace HSPGUI.Resources
{
    /// <summary> 
    /// 
    /// </summary>
    public class TagData
    {
        public DateTime TriggerTime { get; set; }
        public string EntryEPC { get; set; }
        public string EncodeData { get; set; }
        public string TID { get; set; }
        public string ExitEPC { get; set; }
        public Dictionary<string, byte> stats { get; set; }
        
        private string[] statTypes = new String[] { "ALL", "OPN", "EPC","TID", "KIL","ACC", "USR", "PCW","LCK","RSSI_I","RSSI_Q" };
        
        public TagData()
        {
            TriggerTime = new DateTime();
            EntryEPC = "";
            EncodeData = "";
            TID = "";
            ExitEPC = "";
            stats = new Dictionary<string, byte>
            {
                {statTypes[0], 0x90},//ALL
                {statTypes[1], 0x9F},//OPN
                {statTypes[2], 0x9F},//EPC
                {statTypes[3], 0x9F},//TID
                {statTypes[4], 0x9F},//KIL
                {statTypes[5], 0x9F},//ACC
                {statTypes[6], 0x9F},//USR
                {statTypes[7], 0x9F},//PCW
                {statTypes[8], 0x9F},//LCK
                {statTypes[9], 0x0},// RSSI_I
                {statTypes[10], 0x0},//RSSI_Q
            };
        }
        public override string ToString()
        {
            
            var sb = new StringBuilder();
            sb.Append($"{this.TriggerTime:HH:mm:ss.fff} Entry = {this.EntryEPC,32} --> Encode = {this.EncodeData,32} --> Exit EPC = {this.ExitEPC,32}");
            sb.Append($"\nStats: ");
            foreach (var statType in statTypes)
            {
                if (this.stats.ContainsKey(statType) && this.stats[statType] != 0x9F && this.stats[statType] != 0x9E)
                {
                    var statCode = this.stats[statType].ToString("X2");
                    if (statCode == "00" && !(statType == "RSSI_I" || statType == "RSSI_Q")) statCode = "Pass";
                    else if (statCode == "90") statCode = "Fail";
                    else if (statCode == "69") statCode = "Buffer Empty or Trigger Timing to close";
                    else statCode = statCode.PadLeft(4);
                    sb.Append($"{statType}:{statCode} ");
                }
                else
                {
                    string tmp = "";
                    sb.Append(tmp.PadLeft(9));
                }
            }
            return sb.ToString().Trim();            
        }

    }
    public class TagLog
    {
        private TagData? _currentTag { get; set; }
        public Queue<TagData> _tagHist { get; private set; }
        public Queue<TagData> _badTags { get; private set; }
        private TagData? nextTag;
        private bool _newTag;
        private string _curTagString;
        private const int _tagHistSize =5;
        public TagLog() {
            _currentTag = new TagData();
            nextTag = null;
            _tagHist = new Queue<TagData>();
            _badTags = new Queue<TagData>();
            _newTag = false;
            _curTagString = "";
        }
        
        public async Task<bool> addData(string data)
        {
            return await Task.Run(() =>
            {
                return parseData(data);
            });
        }
        private bool parseData(string data){
            bool newTag = false;
            var newLines = data.Split(new string[] { "\r\n", "\r\n\r\n" }, StringSplitOptions.None);
            foreach (var line in newLines)
            {
                
                if (line.Contains("TRIGGER"))
                {
                    _newTag = true;
                    newTag = true;
                    if (nextTag != null)
                    {
                        _tagHist.Enqueue(nextTag);
                        _currentTag = nextTag;
                    }
                    newTag = true;
                    nextTag = new TagData();
                    nextTag.TriggerTime = DateTime.TryParse(line.Substring(0, line.IndexOf("TRIGGER") - 1), out DateTime triggerTime)?
                        triggerTime : default(DateTime);
                    while (_tagHist.Count > _tagHistSize)
                    {
                        _tagHist.Dequeue();
                    }
                }
                int index = line.IndexOf("=");
                if (index != -1 && index < line.Length && nextTag != null)
                {
                    data = line.Substring(line.IndexOf("=") + 1);
                    if (line.Contains("ENCDATA="))
                    {
                        nextTag.EncodeData = data;
                    }
                    else if (line.Contains("ENTRYEPC="))
                    {
                        nextTag.EntryEPC = data;
                    }
                    else if (line.Contains("TID="))
                    {
                        nextTag.TID = data;
                    }
                    else if (line.Contains("EXITEPC="))
                    {
                        nextTag.ExitEPC = data;
                    }
                    else if (line.Contains("ALL"))
                    {
                        // Split the string into parts
                        var searchString = "RSSI=";
                        var RSSIindex = line.IndexOf(searchString)+searchString.Length;                        
                        if (RSSIindex > searchString.Length && RSSIindex < line.Length && nextTag.stats != null)
                        {
                            
                            var RSSI = line.Substring(line.IndexOf(searchString) + searchString.Length).Split(' ');
                            nextTag.stats["RSSI_I"] = Convert.ToByte(RSSI[0], 16);
                            nextTag.stats["RSSI_Q"] = Convert.ToByte(RSSI[1], 16);
                        }
                        
                        string[] parts = data.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        bool badTag = false;
                        foreach (var part in parts)
                        {
                            if (!string.IsNullOrEmpty(part) && part.Contains(":") && nextTag.stats != null)
                            {
                                var keyPair = part.Split(':');
                                nextTag.stats[keyPair[0]] = Convert.ToByte(keyPair[1],16);
                                //System.Diagnostics.Debug.WriteLine(nextTag.stats[keyPair[0]].ToString("X2"));
                                if (nextTag.stats[keyPair[0]].ToString("X2") == "90")
                                {
                                    badTag = true;
                                }
                            }
                        }
                        _currentTag = nextTag;
                        newTag = true;
                        _newTag = true;
                        if (badTag)
                        {
                            //System.Diagnostics.Debug.WriteLine("badTag");
                            _badTags.Enqueue(nextTag);
                            while (_badTags.Count > 400)
                            {
                                _badTags.Dequeue();
                            }
                        }
                    }
                }
            }
            return newTag;
        }
        public string getCurrentTag()
        {
            if (_currentTag != null && _newTag)
            {
                //System.Diagnostics.Debug.WriteLine(currentTag.ToString());
                _newTag = false;
                _curTagString = _currentTag.ToString();
            }
            return _curTagString;
        }
        public async Task<String> getHistAsync()
        {
            return await Task.Run(() =>
            {
                var resultBuilder = new StringBuilder(); // Use StringBuilder for better performance
                foreach (var tag in _tagHist)
                {
                    resultBuilder.AppendLine(tag.ToString()); // Append formatted tag
                }
                return resultBuilder.ToString().TrimEnd(); // Convert StringBuilder to string
            });
        }
        public string dequeErrHist()
        {
            var tag = new TagData();
            bool result = _badTags.TryDequeue(out tag);
            return result && tag != null ? tag.ToString():"";
        }
        

    }

}
