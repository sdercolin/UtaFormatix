using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace UtaFormatix
{
    public partial class Data
    {
        public Data()
        {
        }

        public Data(Data data)
        {
            ProjectName = data.ProjectName;
            Files = data.Files;
            TrackList = data.TrackList.Select(it => new Track(it)).ToList();
            TimeSigList = data.TimeSigList.Select(it => new TimeSig(it)).ToList();
            TempoList = data.TempoList.Select(it => new Tempo(it)).ToList();
            PreMeasure = data.PreMeasure;
            Lyric = new Lyric(this, false);
        }

        public string ProjectName;
        public List<string> Files;
        public List<Track> TrackList;
        public List<TimeSig> TimeSigList;
        public List<Tempo> TempoList;
        public int PreMeasure = 0;
        public Lyric Lyric;

        public bool Import(List<string> fileNames)
        {
            Files = fileNames.ToList();
            var format = UtaFormat.None;
            var extension = Path.GetExtension(fileNames.First());
            //Determine the format of the project
            if (extension == ".vsqx")
            {
                var content = File.ReadAllText(Files[0]);
                if (content.Contains("vsq3 xmlns=\"http://www.yamaha.co.jp/vocaloid/schema/vsq3/\""))
                {
                    format = UtaFormat.Vsq3;
                }
                else if (content.Contains("vsq4 xmlns=\"http://www.yamaha.co.jp/vocaloid/schema/vsq4/\""))
                {
                    format = UtaFormat.Vsq4;
                }
            }
            else if (extension == ".ust")
            {
                format = UtaFormat.Ust;
            }
            else if (extension == ".ccs")
            {
                format = UtaFormat.Ccs;
            }
            else if (extension == ".vpr")
            {
                format = UtaFormat.Vpr;
            }
            else
            {
                MessageBox.Show("The format of this file is not supported.", "Import");
                return false;
            }

            switch (format)
            {
                case UtaFormat.Vsq2:
                    break;

                case UtaFormat.Vsq3:
                    return ImportVsq3(Files);

                case UtaFormat.Vsq4:
                    return ImportVsq4(Files);

                case UtaFormat.Ust:
                    return ImportUst(Files);

                case UtaFormat.Ccs:
                    return ImportCcs(Files);

                case UtaFormat.Vpr:
                    return ImportVpr(Files);
            }
            return false;
        }

        public bool ImportVsq3(List<string> fileNames)
        {
            if (fileNames.Count != 1)
            {
                MessageBox.Show("Cannot Import more than one vsqx files.", "Import");
                return false;
            }
            var vsq3 = new XmlDocument();
            vsq3.Load(fileNames.First());

            //Setup TrackList
            var root = vsq3.FirstChild.NextSibling;
            XmlNode masterTrack = null;
            ProjectName = Path.GetFileNameWithoutExtension(fileNames[0]);
            TrackList = new List<Track>();
            TimeSigList = new List<TimeSig>();
            TempoList = new List<Tempo>();
            int TrackNum = 0;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                if (root.ChildNodes[i].Name == "masterTrack")
                {
                    masterTrack = root.ChildNodes[i];
                    for (int j = 0; j < masterTrack.ChildNodes.Count; j++)
                    {
                        if (masterTrack.ChildNodes[j].Name == "preMeasure")
                        {
                            PreMeasure = Convert.ToInt32(masterTrack.ChildNodes[j].FirstChild.Value);
                        }
                        if (masterTrack.ChildNodes[j].Name == "timeSig")
                        {
                            var newTimeSig = new TimeSig();
                            var inTimeSig = masterTrack.ChildNodes[j].FirstChild;
                            newTimeSig.PosMes = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            inTimeSig = inTimeSig.NextSibling;
                            newTimeSig.Nume = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            inTimeSig = inTimeSig.NextSibling;
                            newTimeSig.Denomi = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            TimeSigList.Add(newTimeSig);
                        }
                        if (masterTrack.ChildNodes[j].Name == "tempo")
                        {
                            var newTempo = new Tempo();
                            var inTimeSig = masterTrack.ChildNodes[j].FirstChild;
                            newTempo.PosTick = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            inTimeSig = inTimeSig.NextSibling;
                            newTempo.BpmTimes100 = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            TempoList.Add(newTempo);
                        }
                    }
                }
            }
            if (root.HasChildNodes)
            {
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    if (root.ChildNodes[i].Name == "vsTrack")
                    {
                        int noteNum = 0;
                        var thisTrack = root.ChildNodes[i];
                        var newTrack = new Track();
                        newTrack.TrackNum = TrackNum;
                        //Setup NoteList for every track
                        newTrack.NoteList = new List<Note>();
                        for (int j = 0; j < thisTrack.ChildNodes.Count; j++)
                        {
                            if (thisTrack.ChildNodes[j].Name == "trackName")
                            {
                                newTrack.TrackName = thisTrack.ChildNodes[j].FirstChild.Value;
                            }
                            if (thisTrack.ChildNodes[j].Name == "musicalPart")
                            {
                                var thisPart = thisTrack.ChildNodes[j];
                                int partStartTime = Convert.ToInt32(thisPart.FirstChild.FirstChild.Value);
                                for (int k = 0; k < thisPart.ChildNodes.Count; k++)
                                {
                                    if (thisPart.ChildNodes[k].Name == "note")
                                    {
                                        var newNote = new Note();
                                        newNote.NoteNum = noteNum;
                                        var thisNote = thisPart.ChildNodes[k];
                                        var inThisNote = thisNote.FirstChild;
                                        newNote.NoteTimeOn = Convert.ToInt32(inThisNote.FirstChild.Value) + partStartTime;
                                        inThisNote = inThisNote.NextSibling;
                                        newNote.NoteTimeOff = newNote.NoteTimeOn + Convert.ToInt32(inThisNote.FirstChild.Value);
                                        inThisNote = inThisNote.NextSibling;
                                        newNote.NoteKey = Convert.ToInt32(inThisNote.FirstChild.Value);
                                        inThisNote = inThisNote.NextSibling;
                                        inThisNote = inThisNote.NextSibling;
                                        newNote.NoteLyric = inThisNote.FirstChild.Value;
                                        noteNum++;
                                        newTrack.NoteList.Add(newNote);
                                    }
                                }
                            }
                        }
                        if (newTrack.NoteList.Count > 0)
                        {
                            TrackList.Add(newTrack);
                        }
                    }
                }
            }
            if (TrackList.Count > 0)
            {
                Lyric = new Lyric(this, true);
                return true;
            }
            else
            {
                MessageBox.Show("The vsqx is invalid or empty.", "Import");
                return false;
            }
        }

        public bool ImportVsq4(List<string> fileNames)
        {
            if (fileNames.Count != 1)
            {
                MessageBox.Show("Cannot Import more than one vsqx files.", "Import");
                return false;
            }
            var vsq4 = new XmlDocument();
            vsq4.Load(fileNames[0]);
            //Setup TrackList
            var root = vsq4.FirstChild.NextSibling;
            XmlNode masterTrack = null;
            ProjectName = Path.GetFileNameWithoutExtension(fileNames[0]);
            TrackList = new List<Track>();
            TimeSigList = new List<TimeSig>();
            TempoList = new List<Tempo>();
            int trackNum = 0;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                if (root.ChildNodes[i].Name == "masterTrack")
                {
                    masterTrack = root.ChildNodes[i];
                    for (int j = 0; j < masterTrack.ChildNodes.Count; j++)
                    {
                        if (masterTrack.ChildNodes[j].Name == "preMeasure")
                        {
                            PreMeasure = Convert.ToInt32(masterTrack.ChildNodes[j].FirstChild.Value);
                        }
                        if (masterTrack.ChildNodes[j].Name == "timeSig")
                        {
                            var newTimeSig = new TimeSig();
                            var inTimeSig = masterTrack.ChildNodes[j].FirstChild;
                            newTimeSig.PosMes = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            inTimeSig = inTimeSig.NextSibling;
                            newTimeSig.Nume = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            inTimeSig = inTimeSig.NextSibling;
                            newTimeSig.Denomi = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            TimeSigList.Add(newTimeSig);
                        }
                        if (masterTrack.ChildNodes[j].Name == "tempo")
                        {
                            var newTempo = new Tempo();
                            var inTimeSig = masterTrack.ChildNodes[j].FirstChild;
                            newTempo.PosTick = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            inTimeSig = inTimeSig.NextSibling;
                            newTempo.BpmTimes100 = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            TempoList.Add(newTempo);
                        }
                    }
                }
            }
            if (root.HasChildNodes)
            {
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    if (root.ChildNodes[i].Name == "vsTrack")
                    {
                        int noteNum = 0;
                        var thisTrack = root.ChildNodes[i];
                        var newTrack = new Track();
                        newTrack.TrackNum = trackNum;
                        //Setup NoteList for every track
                        newTrack.NoteList = new List<Note>();
                        for (int j = 0; j < thisTrack.ChildNodes.Count; j++)
                        {
                            if (thisTrack.ChildNodes[j].Name == "name")
                            {
                                newTrack.TrackName = thisTrack.ChildNodes[j].FirstChild.Value;
                            }
                            if (thisTrack.ChildNodes[j].Name == "vsPart")
                            {
                                var thisPart = thisTrack.ChildNodes[j];
                                int partStartTime = Convert.ToInt32(thisPart.FirstChild.FirstChild.Value);
                                for (int k = 0; k < thisPart.ChildNodes.Count; k++)
                                {
                                    if (thisPart.ChildNodes[k].Name == "note")
                                    {
                                        var newNote = new Note();
                                        newNote.NoteNum = noteNum;
                                        var thisNote = thisPart.ChildNodes[k];
                                        var inThisNote = thisNote.FirstChild;
                                        newNote.NoteTimeOn = Convert.ToInt32(inThisNote.FirstChild.Value) + partStartTime;
                                        inThisNote = inThisNote.NextSibling;
                                        newNote.NoteTimeOff = newNote.NoteTimeOn + Convert.ToInt32(inThisNote.FirstChild.Value);
                                        inThisNote = inThisNote.NextSibling;
                                        newNote.NoteKey = Convert.ToInt32(inThisNote.FirstChild.Value);
                                        inThisNote = inThisNote.NextSibling;
                                        inThisNote = inThisNote.NextSibling;
                                        newNote.NoteLyric = inThisNote.FirstChild.Value;
                                        noteNum++;
                                        newTrack.NoteList.Add(newNote);
                                    }
                                }
                            }
                        }
                        if (newTrack.NoteList.Count > 0)
                        {
                            TrackList.Add(newTrack);
                        }
                    }
                }
            }
            if (TrackList.Count > 0)
            {
                Lyric = new Lyric(this, true);
                return true;
            }
            else
            {
                MessageBox.Show("The vsqx is invalid or empty.", "Import");
                return false;
            }
        }

        public bool ImportVpr(List<string> fileNames)
        {
            if (fileNames.Count != 1)
            {
                MessageBox.Show("Cannot Import more than one vpr files.", "Import");
                return false;
            }

            var tempDirectory = "temp/";
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
            var directory = Directory.CreateDirectory(tempDirectory);
            var copiedVprTempFileName = tempDirectory + Path.GetFileNameWithoutExtension(fileNames.First()) + ".zip";
            File.Copy(fileNames.First(), copiedVprTempFileName);
            var unzippedDirectory = ZipUtil.Unzip(copiedVprTempFileName);
            var jsonFileName = Path.Combine(unzippedDirectory, "Project", "sequence.json");
            var vpr = Newtonsoft.Json.JsonConvert.DeserializeObject<Vpr>(File.ReadAllText(jsonFileName));

            ProjectName = vpr.Title;
            PreMeasure = 1;
            TimeSigList = vpr.MasterTrack.TimeSig.Events.Select(it => new TimeSig
            {
                PosMes = it.Bar + 1,
                Nume = it.Numer,
                Denomi = it.Denom
            }).ToList();
            TempoList = vpr.MasterTrack.Tempo.Events.Select(it => new Tempo
            {
                PosTick = Constant.TickNumberForOneBar + it.Pos,
                BpmTimes100 = it.Value
            }).ToList();
            TrackList = new List<Track>();

            for (int i = 0; i < vpr.Tracks.Count; i++)
            {
                var vprTrack = vpr.Tracks[i];
                var track = new Track
                {
                    TrackName = vprTrack.Name,
                    TrackNum = i
                };
                track.NoteList = new List<Note>();
                var noteNum = 0;
                foreach (var vprPart in vprTrack.Parts)
                {
                    var partStartTime = vprPart.Pos;
                    foreach (var vprNote in vprPart.Notes)
                    {
                        track.NoteList.Add(new Note
                        {
                            NoteTimeOn = Constant.TickNumberForOneBar + partStartTime + vprNote.Pos,
                            NoteTimeOff = Constant.TickNumberForOneBar + partStartTime + vprNote.Pos + vprNote.Duration,
                            NoteNum = noteNum++,
                            NoteKey = vprNote.Number,
                            NoteLyric = vprNote.Lyric
                        });
                    }
                }

                if (track.NoteList.Count == 0)
                {
                    continue;
                }
                for (int j = 0; j < track.NoteList.Count - 1; j++)
                {
                    var note = track.NoteList[j];
                    var nextNote = track.NoteList[j + 1];
                    if (note.NoteTimeOff > nextNote.NoteTimeOn)
                    {
                        note.NoteTimeOff = nextNote.NoteTimeOn;
                    }
                }

                if (track.NoteList.Count > 0)
                {
                    TrackList.Add(track);
                }
            }

            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }

            if (TrackList.Count > 0)
            {
                Lyric = new Lyric(this, true);
                return true;
            }
            else
            {
                MessageBox.Show("The vpr is invalid or empty.", "Import");
                return false;
            }
        }

        public bool ImportUst(List<string> fileNames)
        {
            int trackCount = 0;
            TrackList = new List<Track>();
            foreach (string fileName in fileNames)
            {
                bool isValid = false;
                var reader = new StreamReader(fileName, Encoding.GetEncoding("Shift-JIS"));
                int noteNum = 0;
                int time = 0;
                PreMeasure = 1;
                //Setup TrackList
                var newTrack = new Track();
                newTrack.TrackNum = 0;
                newTrack.TrackName = Path.GetFileNameWithoutExtension(fileName);
                if (trackCount == 0)
                {
                    var firstTimeSig = new TimeSig();
                    TimeSigList = new List<TimeSig>();
                    firstTimeSig.Denomi = 4;
                    firstTimeSig.Nume = 4;
                    firstTimeSig.PosMes = 0;
                    TimeSigList.Add(firstTimeSig);
                    TempoList = new List<Tempo>();
                }
                //Setup NoteList for every track
                for (string buffer = "Starting"; buffer != "[#TRACKEND]" && buffer != null; buffer = reader.ReadLine())
                {
                    if (trackCount == 0)
                    {
                        if (buffer.Contains("ProjectName="))
                        {
                            ProjectName = buffer.Remove(0, "ProjectName=".Length);
                        }
                        if (buffer.Contains("Tempo="))
                        {
                            var firstTempo = new Tempo();
                            firstTempo.PosTick = 0;
                            if (double.TryParse(buffer.Remove(0, "Tempo=".Length), out double bpm))
                            {
                                firstTempo.BpmTimes100 = (int)(bpm * 100);
                            }
                            TempoList.Add(firstTempo);
                        }
                    }
                    if (buffer.Contains("[#0000]"))
                    {
                        isValid = false;
                        newTrack.NoteList = new List<Note>();
                        var newNote = new Note();
                        newNote.NoteNum = noteNum;
                        bool isNoteValid = false;
                        bool tempoTempFlag = false;
                        for (buffer = reader.ReadLine(); buffer != "[#TRACKEND]" && buffer != null; buffer = reader.ReadLine())
                        {
                            if (buffer.Contains("[#"))
                            {
                                if (isNoteValid)
                                {
                                    newTrack.NoteList.Add(newNote);
                                    noteNum++;
                                }
                                newNote = new Note();
                                newNote.NoteNum = noteNum;
                                isNoteValid = false;
                            }
                            if (buffer.Contains("Length="))
                            {
                                newNote.NoteTimeOn = time;
                                time += Convert.ToInt32(buffer.Substring(7, buffer.Length - 7));
                                newNote.NoteTimeOff = time;
                                if (tempoTempFlag)
                                {
                                    TempoList[TempoList.Count - 1].PosTick = newNote.NoteTimeOn;
                                }
                            }
                            if (buffer.Contains("Lyric="))
                            {
                                if (buffer.Substring(6, buffer.Length - 6) != "R" && buffer.Substring(6, buffer.Length - 6) != "r" && newNote.NoteLength != 0)
                                {
                                    isNoteValid = true;
                                    isValid = true;
                                    newNote.NoteLyric = buffer.Substring(6, buffer.Length - 6);
                                }
                            }
                            if (trackCount == 0)
                            {
                                if (buffer.Contains("Tempo="))
                                {
                                    var newTempo = new Tempo();
                                    try
                                    {
                                        newTempo.PosTick = newNote.NoteTimeOn;
                                    }
                                    catch
                                    {
                                        tempoTempFlag = true;
                                    }
                                    newTempo.BpmTimes100 = (int)(100 * double.Parse(buffer.Substring(6, buffer.Length - 6)));
                                    TempoList.Add(newTempo);
                                }
                            }
                            if (buffer.Contains("NoteNum="))
                            {
                                newNote.NoteKey = Convert.ToInt32(buffer.Substring(8, buffer.Length - 8));
                            }
                        }
                    }
                }
                if (!isValid)
                {
                    MessageBox.Show("The ust is invalid or empty.", "Import");
                    return false;
                }
                foreach (var Note in newTrack.NoteList)
                {
                    Note.NoteTimeOn += Constant.TickNumberForOneBar;
                    Note.NoteTimeOff += Constant.TickNumberForOneBar;
                }
                if (trackCount == 0)
                {
                    foreach (var Tempo in TempoList)
                    {
                        if (TempoList.IndexOf(Tempo) != 0)
                        {
                            Tempo.PosTick += Constant.TickNumberForOneBar;
                        }
                    }
                }
                TrackList.Add(newTrack);
                trackCount++;
            }
            Lyric = new Lyric(this, true);
            return true;
        }

        public bool ImportCcs(List<string> fileNames)
        {
            if (fileNames.Count != 1)
            {
                MessageBox.Show("Cannot Import more than one ccs files.", "Import");
                return false;
            }
            var ccs = new XmlDocument();
            ccs.Load(fileNames[0]);
            PreMeasure = 1;
            //Setup TrackList
            var scenario = (XmlElement)ccs.FirstChild.NextSibling;
            var scene = (XmlElement)((XmlElement)scenario.GetElementsByTagName("Sequence")[0]).GetElementsByTagName("Scene")[0];
            var units = (XmlElement)scene.GetElementsByTagName("Units")[0];
            var groups = (XmlElement)scene.GetElementsByTagName("Groups")[0];
            ProjectName = Path.GetFileNameWithoutExtension(fileNames[0]);
            bool isValid = false;
            TrackList = new List<Track>();
            int trackNum = 0;
            bool tempoFinished = false;
            foreach (XmlElement unit in units.GetElementsByTagName("Unit"))
            {
                if (unit.GetAttribute("Category") == "SingerSong")
                {
                    if (!tempoFinished)
                    {
                        TempoList = new List<Tempo>();
                        var tempo = (XmlElement)((XmlElement)(unit.GetElementsByTagName("Song")[0])).GetElementsByTagName("Tempo")[0];
                        foreach (XmlElement Sound in tempo.GetElementsByTagName("Sound"))
                        {
                            var newTempo = new Tempo();
                            newTempo.PosTick = int.Parse(Sound.GetAttribute("Clock")) / 2;
                            newTempo.BpmTimes100 = (int)(double.Parse(Sound.GetAttribute("Tempo")) * 100);
                            TempoList.Add(newTempo);
                        }
                        TimeSigList = new List<TimeSig>();
                        var beat = (XmlElement)((XmlElement)(unit.GetElementsByTagName("Song")[0])).GetElementsByTagName("Beat")[0];
                        int time = 0;
                        int mes = 0;
                        int beats = 4;
                        int beatType = 4;
                        foreach (XmlElement Time in beat.GetElementsByTagName("Time"))
                        {
                            var newTimeSig = new TimeSig();
                            newTimeSig.PosMes = mes + (int.Parse(Time.GetAttribute("Clock")) - time) / (beats * Constant.TickNumberForOneBeat * 8 / beatType);
                            mes = newTimeSig.PosMes;
                            time = int.Parse(Time.GetAttribute("Clock"));
                            newTimeSig.Nume = int.Parse(Time.GetAttribute("Beats"));
                            beats = newTimeSig.Nume;
                            newTimeSig.Denomi = int.Parse(Time.GetAttribute("BeatType"));
                            beatType = newTimeSig.Denomi;
                            TimeSigList.Add(newTimeSig);
                        }
                    }
                    var newTrack = new Track();
                    newTrack.TrackNum = trackNum;
                    string groupId = unit.GetAttribute("Group");
                    foreach (XmlElement group in groups.GetElementsByTagName("Group"))
                    {
                        if (group.GetAttribute("Id") == groupId)
                        {
                            newTrack.TrackName = group.GetAttribute("Name");
                        }
                    }
                    newTrack.NoteList = new List<Note>();
                    int noteNum = 0;
                    foreach (XmlElement note in ((XmlElement)((XmlElement)unit.GetElementsByTagName("Song")[0]).GetElementsByTagName("Score")[0]).GetElementsByTagName("Note"))
                    {
                        var newNote = new Note();
                        newNote.NoteNum = noteNum;
                        newNote.NoteTimeOn = int.Parse(note.GetAttribute("Clock")) / 2;
                        newNote.NoteTimeOff = newNote.NoteTimeOn + int.Parse(note.GetAttribute("Duration")) / 2;
                        newNote.NoteKey = int.Parse(note.GetAttribute("PitchStep")) + (int.Parse(note.GetAttribute("PitchOctave")) + 1) * Constant.KeyForOneOctave;
                        newNote.NoteLyric = note.GetAttribute("Lyric");
                        noteNum++;
                        newTrack.NoteList.Add(newNote);
                    }
                    if (newTrack.NoteList.Count > 0)
                    {
                        TrackList.Add(newTrack);
                        trackNum++;
                        isValid = true;
                    }
                }
            }
            if (!isValid)
            {
                MessageBox.Show("The ccs is invalid or empty.", "Import");
                return false;
            }
            Lyric = new Lyric(this, true);
            return true;
        }

        public void ExportUst(string fileName)
        {
            string omitText = "";
            for (int trackNum = 0; trackNum < TrackList.Count; trackNum++)
            {
                string ustContents = "[#VERSION]" + Environment.NewLine;
                ustContents += "UST Version1.2" + Environment.NewLine;
                ustContents += "[#SETTING]" + Environment.NewLine;
                foreach (var timeSig in TimeSigList)
                {
                    if (timeSig.Denomi != 4 || timeSig.Nume != 4)
                    {
                        if (trackNum == 0)
                        {
                            omitText += "Meter change omitted at Measure [" + timeSig.PosMes + "] : " + timeSig.Nume + "/" + timeSig.Denomi + Environment.NewLine;
                        }
                    }
                }
                foreach (var tempo in TempoList)
                {
                    if (tempo.PosTick != 0)
                    {
                        if (trackNum == 0)
                        {
                            omitText += "Tempo change omitted at Tick [" + tempo.PosTick + "] : " + tempo.Bpm.ToString("F2") + Environment.NewLine;
                        }
                    }
                    else
                    {
                        ustContents += "Tempo=" + tempo.Bpm.ToString("F2") + Environment.NewLine;
                    }
                }
                int pos = 0;
                int restCount = 0;
                ustContents += "Tracks=1" + Environment.NewLine;
                ustContents += "ProjectName=" + TrackList[trackNum].TrackName + Environment.NewLine;
                ustContents += "Mode2=True" + Environment.NewLine;
                for (int notenum = 0; notenum < TrackList[trackNum].NoteList.Count; notenum++)
                {
                    var thisnote = TrackList[trackNum].NoteList[notenum];
                    if (pos < thisnote.NoteTimeOn)
                    {
                        ustContents += "[#" + (notenum + restCount).ToString("D4") + "]" + Environment.NewLine;
                        ustContents += "Length=" + (thisnote.NoteTimeOn - pos).ToString() + Environment.NewLine;
                        ustContents += "Lyric=R" + Environment.NewLine;
                        ustContents += "NoteNum=60" + Environment.NewLine;
                        ustContents += "PreUtterance=" + Environment.NewLine;
                        restCount++;
                    }
                    ustContents += "[#" + (notenum + restCount).ToString("D4") + "]" + Environment.NewLine;
                    ustContents += "Length=" + thisnote.NoteLength.ToString() + Environment.NewLine;
                    ustContents += "Lyric=" + thisnote.NoteLyric + Environment.NewLine;
                    ustContents += "NoteNum=" + thisnote.NoteKey + Environment.NewLine;
                    ustContents += "PreUtterance=" + Environment.NewLine;
                    pos = thisnote.NoteTimeOff;
                }
                ustContents += "[#TRACKEND]";
                File.WriteAllText(fileName + "\\" + ProjectName + "_" + trackNum.ToString() + "_" + TrackList[trackNum].TrackName.Replace("\\", "").Replace("/", "").Replace(".", "") + ".ust", ustContents, Encoding.GetEncoding("Shift-JIS"));
            }
            MessageBox.Show(omitText + "Ust is successfully exported.", "Export Ust");
        }

        public void ExportVsq4(string fileName)
        {
            var vsq4 = new XmlDocument();
            string template = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\r\n<vsq4 xmlns=\"http://www.yamaha.co.jp/vocaloid/schema/vsq4/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.yamaha.co.jp/vocaloid/schema/vsq4/ vsq4.xsd\">\r\n  <vender><![CDATA[Yamaha corporation]]></vender>\r\n  <version><![CDATA[4.0.0.3]]></version>\r\n  <vVoiceTable>\r\n    <vVoice>\r\n      <bs>0</bs>\r\n      <pc>0</pc>\r\n      <id><![CDATA[BCXDC6CZLSZHZCB4]]></id>\r\n      <name><![CDATA[VY2V3]]></name>\r\n      <vPrm>\r\n        <bre>0</bre>\r\n        <bri>0</bri>\r\n        <cle>0</cle>\r\n        <gen>0</gen>\r\n        <ope>0</ope>\r\n      </vPrm>\r\n    </vVoice>\r\n  </vVoiceTable>\r\n  <mixer>\r\n    <masterUnit>\r\n      <oDev>0</oDev>\r\n      <rLvl>0</rLvl>\r\n      <vol>0</vol>\r\n    </masterUnit>\r\n    <vsUnit>\r\n      <tNo>0</tNo>\r\n      <iGin>0</iGin>\r\n      <sLvl>-898</sLvl>\r\n      <sEnable>0</sEnable>\r\n      <m>0</m>\r\n      <s>0</s>\r\n      <pan>64</pan>\r\n      <vol>0</vol>\r\n    </vsUnit>\r\n    <monoUnit>\r\n      <iGin>0</iGin>\r\n      <sLvl>-898</sLvl>\r\n      <sEnable>0</sEnable>\r\n      <m>0</m>\r\n      <s>0</s>\r\n      <pan>64</pan>\r\n      <vol>0</vol>\r\n    </monoUnit>\r\n    <stUnit>\r\n      <iGin>0</iGin>\r\n      <m>0</m>\r\n      <s>0</s>\r\n      <vol>-129</vol>\r\n    </stUnit>\r\n  </mixer>\r\n  <masterTrack>\r\n    <seqName><![CDATA[Untitled0]]></seqName>\r\n    <comment><![CDATA[New VSQ File]]></comment>\r\n    <resolution>480</resolution>\r\n    <preMeasure>4</preMeasure>\r\n    <timeSig>\r\n      <m>0</m>\r\n      <nu>4</nu>\r\n      <de>4</de>\r\n    </timeSig>\r\n    <tempo>\r\n      <t>0</t>\r\n      <v>12000</v>\r\n    </tempo>\r\n  </masterTrack>\r\n  <vsTrack>\r\n    <tNo>0</tNo>\r\n    <name><![CDATA[Track]]></name>\r\n    <comment><![CDATA[Track]]></comment>\r\n    <vsPart>\r\n      <t>7680</t>\r\n      <playTime>61440</playTime>\r\n      <name><![CDATA[NewPart]]></name>\r\n      <comment><![CDATA[New Musical Part]]></comment>\r\n      <sPlug>\r\n        <id><![CDATA[ACA9C502-A04B-42b5-B2EB-5CEA36D16FCE]]></id>\r\n        <name><![CDATA[VOCALOID2 Compatible Style]]></name>\r\n        <version><![CDATA[3.0.0.1]]></version>\r\n      </sPlug>\r\n      <pStyle>\r\n        <v id=\"accent\">50</v>\r\n        <v id=\"bendDep\">8</v>\r\n        <v id=\"bendLen\">0</v>\r\n        <v id=\"decay\">50</v>\r\n        <v id=\"fallPort\">0</v>\r\n        <v id=\"opening\">127</v>\r\n        <v id=\"risePort\">0</v>\r\n      </pStyle>\r\n      <singer>\r\n        <t>0</t>\r\n        <bs>0</bs>\r\n        <pc>0</pc>\r\n      </singer>\r\n      <plane>0</plane>\r\n    </vsPart>\r\n  </vsTrack>\r\n  <monoTrack>\r\n  </monoTrack>\r\n  <stTrack>\r\n  </stTrack>\r\n  <aux>\r\n    <id><![CDATA[AUX_VST_HOST_CHUNK_INFO]]></id>\r\n    <content><![CDATA[VlNDSwAAAAADAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=]]></content>\r\n  </aux>\r\n</vsq4>";
            vsq4.LoadXml(template);
            var root = (XmlElement)(vsq4.FirstChild.NextSibling);
            var mixer = (XmlElement)(root.GetElementsByTagName("mixer"))[0];
            var masterTrack = (XmlElement)(root.GetElementsByTagName("masterTrack"))[0];
            var emptyTrack = (XmlElement)(root.GetElementsByTagName("vsTrack"))[0];
            var emptyUnit = (XmlElement)(mixer.GetElementsByTagName("vsUnit"))[0];
            var preMeasure = (XmlElement)(masterTrack.GetElementsByTagName("preMeasure"))[0];
            preMeasure.InnerText = PreMeasure.ToString();
            var firstTempo = (XmlElement)(masterTrack.GetElementsByTagName("tempo"))[0];
            firstTempo.GetElementsByTagName("v")[0].FirstChild.Value = TempoList[0].BpmTimes100.ToString();
            var firsttimeSig = (XmlElement)(masterTrack.GetElementsByTagName("timeSig"))[0];
            firsttimeSig.GetElementsByTagName("nu")[0].FirstChild.Value = TimeSigList[0].Nume.ToString();
            firsttimeSig.GetElementsByTagName("de")[0].FirstChild.Value = TimeSigList[0].Denomi.ToString();
            if (TempoList.Count > 1)
            {
                for (int i = 1; i < TempoList.Count; i++)
                {
                    var newTempo = (XmlElement)firstTempo.Clone();
                    newTempo.GetElementsByTagName("t")[0].FirstChild.Value = TempoList[i].PosTick.ToString();
                    newTempo.GetElementsByTagName("v")[0].FirstChild.Value = TempoList[i].BpmTimes100.ToString();
                    masterTrack.InsertAfter(newTempo, firstTempo);
                    firstTempo = newTempo;
                }
            }
            if (TimeSigList.Count > 1)
            {
                for (int i = 1; i < TimeSigList.Count; i++)
                {
                    var newtimeSig = (XmlElement)firsttimeSig.Clone();
                    newtimeSig.GetElementsByTagName("m")[0].FirstChild.Value = TimeSigList[i].PosMes.ToString();
                    newtimeSig.GetElementsByTagName("nu")[0].FirstChild.Value = TimeSigList[i].Nume.ToString();
                    newtimeSig.GetElementsByTagName("de")[0].FirstChild.Value = TimeSigList[i].Denomi.ToString();
                    masterTrack.InsertAfter(newtimeSig, firsttimeSig);
                    firsttimeSig = newtimeSig;
                }
            }
            if (TrackList.Count > 0)
            {
                for (int tracknum = 0; tracknum < TrackList.Count; tracknum++)
                {
                    var newTrack = (XmlElement)emptyTrack.Clone();
                    newTrack.GetElementsByTagName("tNo")[0].FirstChild.Value = (tracknum + 1).ToString();
                    emptyTrack.GetElementsByTagName("name")[0].FirstChild.Value = TrackList[tracknum].TrackName;
                    var part = (XmlElement)emptyTrack.GetElementsByTagName("vsPart")[0];
                    int pos = 0;
                    int mes = 0;
                    int nume = 4;
                    int denomi = 4;
                    foreach (var timesig in TimeSigList)
                    {
                        if (timesig.PosMes > PreMeasure)
                        {
                            break;
                        }
                        else
                        {
                            pos += (timesig.PosMes - mes) * nume * Constant.TickNumberForOneBar / denomi;
                            mes = timesig.PosMes;
                            nume = timesig.Nume;
                            denomi = timesig.Denomi;
                        }
                    }
                    pos += (PreMeasure - mes) * nume * Constant.TickNumberForOneBar / denomi;
                    part.GetElementsByTagName("t")[0].InnerText = pos.ToString();
                    int partStartTime = pos;
                    int time = 0;
                    var thisTrack = TrackList[tracknum];
                    var lastNote = (XmlElement)part.GetElementsByTagName("singer")[0];
                    for (int notenum = 0; notenum < thisTrack.NoteList.Count; notenum++)
                    {
                        var thisNote = thisTrack.NoteList[notenum];
                        var note = vsq4.CreateElement("note", vsq4.DocumentElement.NamespaceURI);
                        var t = vsq4.CreateElement("t", vsq4.DocumentElement.NamespaceURI);
                        t.InnerText = (thisNote.NoteTimeOn - partStartTime).ToString();
                        note.AppendChild(t);
                        var dur = vsq4.CreateElement("dur", vsq4.DocumentElement.NamespaceURI);
                        dur.InnerText = thisNote.NoteLength.ToString();
                        note.AppendChild(dur);
                        var n = vsq4.CreateElement("n", vsq4.DocumentElement.NamespaceURI);
                        n.InnerText = thisNote.NoteKey.ToString();
                        note.AppendChild(n);
                        var v = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v.InnerText = "64";
                        note.AppendChild(v);
                        var y = vsq4.CreateElement("y", vsq4.DocumentElement.NamespaceURI);
                        var y_cdata = vsq4.CreateCDataSection(thisNote.NoteLyric);
                        y.AppendChild(y_cdata);
                        note.AppendChild(y);
                        var p = vsq4.CreateElement("p", vsq4.DocumentElement.NamespaceURI);
                        var p_cdata = vsq4.CreateCDataSection("a");
                        p.AppendChild(p_cdata);
                        note.AppendChild(p);
                        var nStyle = vsq4.CreateElement("nStyle", vsq4.DocumentElement.NamespaceURI);
                        var v1 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v1.SetAttribute("id", "accent");
                        v1.InnerText = "50";
                        nStyle.AppendChild(v1);
                        var v2 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v2.SetAttribute("id", "bendDep");
                        v2.InnerText = "0";
                        nStyle.AppendChild(v2);
                        var v3 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v3.SetAttribute("id", "bendLen");
                        v3.InnerText = "0";
                        nStyle.AppendChild(v3);
                        var v4 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v4.SetAttribute("id", "decay");
                        v4.InnerText = "50";
                        nStyle.AppendChild(v4);
                        var v5 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v5.SetAttribute("id", "fallPort");
                        v5.InnerText = "0";
                        nStyle.AppendChild(v5);
                        var v6 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v6.SetAttribute("id", "opening");
                        v6.InnerText = "127";
                        nStyle.AppendChild(v6);
                        var v7 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v7.SetAttribute("id", "risePort");
                        v7.InnerText = "0";
                        nStyle.AppendChild(v7);
                        var v8 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v8.SetAttribute("id", "vibLen");
                        v8.InnerText = "0";
                        nStyle.AppendChild(v8);
                        var v9 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v9.SetAttribute("id", "vibType");
                        v9.InnerText = "0";
                        nStyle.AppendChild(v9);
                        note.AppendChild(nStyle);
                        part.InsertAfter(note, lastNote);
                        lastNote = note;
                        time = thisNote.NoteTimeOff;
                    }
                    part.GetElementsByTagName("playTime")[0].InnerText = time.ToString();
                    root.InsertAfter(newTrack, emptyTrack);
                    emptyTrack = newTrack;
                    var newUnit = (XmlElement)emptyUnit.Clone();
                    newUnit.GetElementsByTagName("tNo")[0].FirstChild.Value = (tracknum + 1).ToString();
                    mixer.InsertAfter(newUnit, emptyUnit);
                    emptyUnit = newUnit;
                }
                root.RemoveChild(emptyTrack);
                mixer.RemoveChild(emptyUnit);
            }
            vsq4.Save(fileName);
            MessageBox.Show("Vsqx is successfully exported." + Environment.NewLine + "If it only pronounces \"a\", please select all notes and run \"Lyrics\"→\"Convert Phonemes\". ", "Export Vsqx");
        }

        public void ExportCcs(string fileName)
        {
            var ccs = new XmlDocument();
            string template = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Scenario Code=\"7251BC4B6168E7B2992FA620BD3E1E77\">\r\n  <Generation>\r\n    <Author Version=\"3.2.21.2\" />\r\n    <TTS Version=\"3.1.0\">\r\n      <Dictionary Version=\"1.4.0\" />\r\n      <SoundSources />\r\n    </TTS>\r\n    <SVSS Version=\"3.0.5\">\r\n      <Dictionary Version=\"1.0.0\" />\r\n      <SoundSources>\r\n        <SoundSource Version=\"1.0.0\" Id=\"XSV-JPF-W\" Name=\"緑咲 香澄\" />\r\n        <SoundSource Version=\"1.0.0\" Id=\"XSV-JPM-P\" Name=\"赤咲 湊\" />\r\n      </SoundSources>\r\n    </SVSS>\r\n  </Generation>\r\n  <Sequence Id=\"\">\r\n    <Scene Id=\"\">\r\n      <Units>\r\n        <Unit Version=\"1.0\" Id=\"\" Category=\"SingerSong\" Group=\"7de5f694-4b60-493d-b6b0-16f6b56deb1f\" StartTime=\"00:00:00\" Duration=\"00:00:02\" CastId=\"XSV-JPM-P\" Language=\"Japanese\">\r\n          <Song Version=\"1.02\">\r\n            <Tempo>\r\n              <Sound Clock=\"0\" Tempo=\"120\" />\r\n            </Tempo>\r\n            <Beat>\r\n              <Time Clock=\"0\" Beats=\"4\" BeatType=\"4\" />\r\n            </Beat>\r\n            <Score>\r\n              <Key Clock=\"0\" Fifths=\"0\" Mode=\"0\" />\r\n            </Score>\r\n          </Song>\r\n        </Unit>\r\n      </Units>\r\n      <Groups>\r\n        <Group Version=\"1.0\" Id=\"7de5f694-4b60-493d-b6b0-16f6b56deb1f\" Category=\"SingerSong\" Name=\"ソング 1\" Color=\"#FFAF1F14\" Volume=\"0\" Pan=\"0\" IsSolo=\"false\" IsMuted=\"false\" CastId=\"XSV-JPM-P\" Language=\"Japanese\" />\r\n      </Groups>\r\n      <SoundSetting Rhythm=\"4/4\" Tempo=\"78\" />\r\n    </Scene>\r\n  </Sequence>\r\n</Scenario>";
            ccs.LoadXml(template);
            var scenario = (XmlElement)ccs.FirstChild.NextSibling;
            var scene = (XmlElement)((XmlElement)scenario.GetElementsByTagName("Sequence")[0]).GetElementsByTagName("Scene")[0];
            var units = (XmlElement)scene.GetElementsByTagName("Units")[0];
            var emptyUnit = (XmlElement)units.GetElementsByTagName("Unit")[0];
            var groups = (XmlElement)scene.GetElementsByTagName("Groups")[0];
            var emptyGroup = (XmlElement)groups.GetElementsByTagName("Group")[0];
            var allTempo = (XmlElement)((XmlElement)(emptyUnit.GetElementsByTagName("Song")[0])).GetElementsByTagName("Tempo")[0];
            var firstTempo = (XmlElement)allTempo.GetElementsByTagName("Sound")[0];
            firstTempo.SetAttribute("Tempo", TempoList[0].Bpm.ToString("F2"));
            for (int i = 1; i < TempoList.Count; i++)
            {
                var newTempo = (XmlElement)firstTempo.Clone();
                newTempo.SetAttribute("Tempo", TempoList[i].Bpm.ToString("F2"));
                newTempo.SetAttribute("Clock", (TempoList[i].PosTick * 2).ToString());
                allTempo.InsertAfter(newTempo, firstTempo);
                firstTempo = newTempo;
            }
            var allBeat = (XmlElement)((XmlElement)(((XmlElement)units.GetElementsByTagName("Unit")[0]).GetElementsByTagName("Song")[0])).GetElementsByTagName("Beat")[0];
            var firstBeat = (XmlElement)allBeat.GetElementsByTagName("Time")[0];
            firstBeat.SetAttribute("Beats", TimeSigList[0].Nume.ToString());
            firstBeat.SetAttribute("BeatType", TimeSigList[0].Denomi.ToString());
            int pos = 0;
            for (int i = 1; i < TimeSigList.Count; i++)
            {
                var newBeat = (XmlElement)firstBeat.Clone();
                pos += (TimeSigList[i].PosMes - TimeSigList[i - 1].PosMes) * Constant.TickNumberForOneBeat * 8 * TimeSigList[i - 1].Nume / TimeSigList[i - 1].Denomi;
                newBeat.SetAttribute("Clock", pos.ToString());
                newBeat.SetAttribute("Beats", TimeSigList[i].Nume.ToString());
                newBeat.SetAttribute("BeatType", TimeSigList[i].Denomi.ToString());
                allBeat.InsertAfter(newBeat, firstBeat);
                firstBeat = newBeat;
            }
            var idList = new List<string>();
            var idRandom = new Random();
            for (int trackNum = 0; trackNum < TrackList.Count; trackNum++)
            {
                var thisTrack = TrackList[trackNum];
                var newUnit = (XmlElement)emptyUnit.Clone();
                var newGroup = (XmlElement)emptyGroup.Clone();
                while (idList.Count <= trackNum)
                {
                    string id = emptyUnit.GetAttribute("Group");
                    id = id.Remove(30, 6);
                    id += idRandom.Next(999999).ToString("D6");
                    if (!idList.Contains(id))
                    {
                        idList.Add(id);
                    }
                }
                emptyUnit.SetAttribute("Group", idList[trackNum]);
                emptyGroup.SetAttribute("Id", idList[trackNum]);
                emptyGroup.SetAttribute("Name", thisTrack.TrackName);
                var song = (XmlElement)emptyUnit.GetElementsByTagName("Song")[0];
                var tempo = (XmlElement)song.GetElementsByTagName("Tempo")[0];
                var beat = (XmlElement)song.GetElementsByTagName("Beat")[0];
                var score = (XmlElement)song.GetElementsByTagName("Score")[0];
                song.ReplaceChild(allTempo.Clone(), tempo);
                song.ReplaceChild(allBeat.Clone(), beat);
                foreach (var Note in thisTrack.NoteList)
                {
                    var note = ccs.CreateElement("Note", ccs.DocumentElement.NamespaceURI);
                    note.SetAttribute("Clock", (Note.NoteTimeOn * 2).ToString());
                    note.SetAttribute("PitchStep", (Note.NoteKey % Constant.KeyForOneOctave).ToString());
                    note.SetAttribute("PitchOctave", (Note.NoteKey / Constant.KeyForOneOctave - 1).ToString());
                    note.SetAttribute("Duration", (Note.NoteLength * 2).ToString());
                    note.SetAttribute("Lyric", Note.NoteLyric);
                    score.AppendChild(note);
                }
                int posTick = 0;
                int posSecond = 0;
                int lastTick = thisTrack.NoteList[thisTrack.NoteList.Count - 1].NoteTimeOff;
                for (int j = 1; j < TempoList.Count; j++)
                {
                    posTick = TempoList[j].PosTick;
                    posSecond += (int)((TempoList[j].PosTick - TempoList[j - 1].PosTick) / 8 / ((TempoList[j - 1].Bpm)) + 1);
                }
                if (posTick < lastTick)
                {
                    posSecond += (int)((lastTick - posTick) / 8 / ((TempoList[TempoList.Count - 1].Bpm)) + 1);
                }
                var timeSpan = new TimeSpan(0, 0, posSecond);
                emptyUnit.SetAttribute("Duration", timeSpan.ToString("c"));
                units.AppendChild(newUnit);
                groups.AppendChild(newGroup);
                emptyUnit = newUnit;
                emptyGroup = newGroup;
            }
            units.RemoveChild(emptyUnit);
            groups.RemoveChild(emptyGroup);
            ccs.Save(fileName);
            MessageBox.Show("Ccs is successfully exported.", "ExportCcs");
        }
    }
}
