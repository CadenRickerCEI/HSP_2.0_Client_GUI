using System.Text;

namespace HSPGUI.Resources
{
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
            sb.Append($" Stats: ");
            foreach (var statType in statTypes)
            {
                if (this.stats.ContainsKey(statType) && this.stats[statType] != 0x9F && this.stats[statType] != 0x9E)
                {
                    sb.Append($"{statType}:{this.stats[statType].ToString("X2")} ");
                }
            }
            return sb.ToString().Trim();            
        }

    }
    public class TagLog
    {
        private TagData? currentTag { get; set; }
        public Queue<TagData> tagHist { get; private set; }
        public Queue<TagData> badTags { get; private set; }
        private TagData? nextTag;
        private bool newTag;
        private string curTagString;
        public TagLog() {
            currentTag = new TagData();
            nextTag = null;
            tagHist = new Queue<TagData>();
            badTags = new Queue<TagData>();
            newTag = false;
            curTagString = "";
        }
        
        public async Task addData(string data)
        {
            await Task.Run(() =>
            {
                parseData(data);
            });
        }
        private void parseData(string data){
            var newLines = data.Split(new string[] { "\r\n", "\r\n\r\n" }, StringSplitOptions.None);
            //System.Diagnostics.Debug.WriteLine(data);
            foreach (var line in newLines)
            {
                
                if (line.Contains("TRIGGER"))
                {
                    if (nextTag != null)
                    {
                        tagHist.Enqueue(nextTag);
                        currentTag = nextTag;
                    }
                    newTag = true;
                    nextTag = new TagData();
                    nextTag.TriggerTime = DateTime.TryParse(line.Substring(0, line.IndexOf("TRIGGER") - 1), out DateTime triggerTime)?
                        triggerTime : default(DateTime);
                    //nextTag.TriggerTime = DateTime.Parse(line.Substring(0,line.IndexOf("TRIGGER")-1));
                    while (tagHist.Count > 30)
                    {
                        tagHist.Dequeue();
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
                        //System.Diagnostics.Debug.WriteLine("stat read"+line);
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
                        currentTag = nextTag;
                        newTag = true;
                        if (badTag)
                        {
                            //System.Diagnostics.Debug.WriteLine("badTag");
                            badTags.Enqueue(nextTag);
                            while (badTags.Count > 400)
                            {
                                badTags.Dequeue();
                            }
                        }
                    }
                }
            }
        }
        public string getCurrentTag()
        {
            if (currentTag != null && newTag)
            {
                //System.Diagnostics.Debug.WriteLine(currentTag.ToString());
                newTag = false;
                curTagString = currentTag.ToString();
            }
            return curTagString;
        }
        public async Task<String> getHistAsync()
        {
            return await Task.Run(() =>
            {
                var resultBuilder = new StringBuilder(); // Use StringBuilder for better performance
                foreach (var tag in tagHist)
                {
                    resultBuilder.AppendLine(tag.ToString()); // Append formatted tag
                }
                return resultBuilder.ToString().TrimEnd(); // Convert StringBuilder to string
            });
        }
        public string dequeErrHist()
        {
            //System.Diagnostics.Debug.WriteLine("dequeue tag");
            var tag = new TagData();
            bool result = badTags.TryDequeue(out tag);
            return result && tag != null ? tag.ToString():"";
        }
        

    }

}
