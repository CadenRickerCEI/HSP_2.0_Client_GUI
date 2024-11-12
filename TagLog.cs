namespace HSPGUI.Resources
{
    public struct TagData
    {
        public DateTime TriggerTime { get; set; }
        public string EntryEPC { get; set; }
        public string EncodeData { get; set; }
        public string TID { get; set; }
        public string ExitEPC { get; set; }
        public Dictionary<string, byte> stats { get; set; }/*
        public bool pass { get; set; }
        public string OPNStat { get; set; }
        public string EPCStat { get; set; }
        public string KILStat { get; set; }
        public string ACCStat { get; set; }
        public string USRStat { get; set; }
        public string PCWStat { get; set; }
        public string LCKSat {  get; set; }
        public int RSSI_I { get; set; }
        public int RSSI_Q { get; set; }
        */

    }
    public class TagLog
    {
        public TagData currentTag { get; private set; }
        public Queue<TagData> tagHist { get; private set; }
        private TagData nextTag;
        public TagLog() {
            currentTag = new TagData();
            tagHist = new Queue<TagData>();
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
            foreach (var line in newLines)
            {
                if (line.Contains("Trigger"))
                {
                    tagHist.Enqueue(nextTag);
                    currentTag = nextTag;
                    nextTag.TriggerTime = DateTime.Parse(line.Substring(0,line.IndexOf("Trigger")-1));
                    nextTag.stats = new Dictionary<string, byte>
                    {
                        {"ALL",0X90 },{"OPN",0X9F },{"TID",0x9F},{"KIL",0X9F},{"USR",0X9F },
                        {"PCW",0X9F },{"LCK",0X9F },{ "RSSI_I",0x0},{ "RSSI_Q",0x0}
                    };
                    while (tagHist.Count > 30)
                    {
                        tagHist.Dequeue();
                    }
                }
                int index = line.IndexOf("=");
                if (index != -1 && index < line.Length)
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
                    else if (line.Contains("TID"))
                    {
                        nextTag.TID = data;
                    }
                    else if (line.Contains("ALL"))
                    {
                        // Split the string into parts
                        var searchString = "RSSI=";
                        var RSSIindex = line.IndexOf(searchString)+searchString.Length;
                        if (RSSIindex > searchString.Length && RSSIindex < line.Length)
                        {
                            var RSSI = line.Substring(line.IndexOf(searchString) + searchString.Length).Split(' ');
                            nextTag.stats["RSSI_I"] = Convert.ToByte(RSSI[0], 16);
                            nextTag.stats["RSSI_Q"] = Convert.ToByte(RSSI[1], 16);
                        }
                        
                        string[] parts = line.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var part in parts)
                        {
                            if (!string.IsNullOrEmpty(part) && part.Contains(":"))
                            {
                                var keyPair = part.Split(':');
                                nextTag.stats[keyPair[0]] = Convert.ToByte(keyPair[1],16);
                            }
                        }
                    }
                }
            }
        }
    }

}
