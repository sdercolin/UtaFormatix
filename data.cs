using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace UtaFormatix
{
    public class Data
    {
        public Data() { }
        public Data(Data data)
        {
            ProjectName = data.ProjectName;
            files = data.files;
            TrackList = new List<track>();
            foreach (track track in data.TrackList)
            {
                TrackList.Add(new track(track));
            }
            timeSigList = new List<timeSig>();
            foreach (timeSig timeSig in data.timeSigList)
            {
                timeSigList.Add(new timeSig(timeSig));
            }
            tempoList = new List<tempo>();
            foreach (tempo tempo in data.tempoList)
            {
                tempoList.Add(new tempo(tempo));
            }
            PreMeasure = data.PreMeasure;
            lyric = new Lyric(this, false);
        }
        public string ProjectName;
        public List<string> files;
        public List<track> TrackList;
        public List<timeSig> timeSigList;
        public List<tempo> tempoList;
        public int PreMeasure = 0;
        public Lyric lyric;
        public bool Import(List<string> filenames)
        {
            files = new List<string>();
            files.AddRange(filenames);
            UtaFormat format = UtaFormat.none;

            //Determine the format of the project
            if (files[0].Remove(0, files[0].Length - 5) == ".vsqx")
            {
                string readbuf = File.ReadAllText(files[0]);
                if (readbuf.Contains("vsq3 xmlns=\"http://www.yamaha.co.jp/vocaloid/schema/vsq3/\""))
                {
                    format = UtaFormat.vsq3;
                }
                else if (readbuf.Contains("vsq4 xmlns=\"http://www.yamaha.co.jp/vocaloid/schema/vsq4/\""))
                {
                    format = UtaFormat.vsq4;
                }
            }
            else if (files[0].Remove(0, files[0].Length - 4) == ".ust")
            {
                format = UtaFormat.ust;
            }
            else if (files[0].Remove(0, files[0].Length - 4) == ".ccs")
            {
                format = UtaFormat.ccs;
            }
            //else if (filename.Remove(0, filename.Length - 4) == ".vsq")
            //{
            //    format = UtaFormat.vsq2;
            //}
            else
            {
                MessageBox.Show("The format of this file is not supported.", "Import");
                return false;
            }

            switch (format)
            {
                case UtaFormat.vsq2:
                    break;
                case UtaFormat.vsq3:
                    return ImportVsq3(files);
                case UtaFormat.vsq4:
                    return ImportVsq4(files);
                case UtaFormat.ust:
                    return ImportUst(files);
                case UtaFormat.ccs:
                    return ImportCcs(files);
            }
            return false;
        }
        public bool ImportVsq3(List<string> filenames)
        {
            if (filenames.Count != 1)
            {
                MessageBox.Show("Cannot Import more than one vsqx.", "Import");
                return false;
            }
            XmlDocument vsq3 = new XmlDocument();
            vsq3.Load(filenames[0]);
            //Set up tracklist
            XmlNode root = vsq3.FirstChild.NextSibling;
            XmlNode MasterTrack = null;
            ProjectName = Path.GetFileNameWithoutExtension(filenames[0]);
            TrackList = new List<track>();
            timeSigList = new List<timeSig>();
            tempoList = new List<tempo>();
            int TrackNum = 0;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                if (root.ChildNodes[i].Name == "masterTrack")
                {
                    MasterTrack = root.ChildNodes[i];
                    for (int j = 0; j < MasterTrack.ChildNodes.Count; j++)
                    {
                        if (MasterTrack.ChildNodes[j].Name == "preMeasure")
                        {
                            PreMeasure = Convert.ToInt32(MasterTrack.ChildNodes[j].FirstChild.Value);
                        }
                        if (MasterTrack.ChildNodes[j].Name == "timeSig")
                        {
                            timeSig newtimeSig = new timeSig();
                            XmlNode inTimeSig = MasterTrack.ChildNodes[j].FirstChild;
                            newtimeSig.posMes = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            inTimeSig = inTimeSig.NextSibling;
                            newtimeSig.nume = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            inTimeSig = inTimeSig.NextSibling;
                            newtimeSig.denomi = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            timeSigList.Add(newtimeSig);
                        }
                        if (MasterTrack.ChildNodes[j].Name == "tempo")
                        {
                            tempo newtempo = new tempo();
                            XmlNode inTimeSig = MasterTrack.ChildNodes[j].FirstChild;
                            newtempo.posTick = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            inTimeSig = inTimeSig.NextSibling;
                            newtempo.bpm_times100 = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            tempoList.Add(newtempo);
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
                        int NoteNum = 0;
                        XmlNode thisTrack = root.ChildNodes[i];
                        track newTrack = new track();
                        newTrack.TrackNum = TrackNum;
                        //Set up notelist for every track
                        newTrack.NoteList = new List<note>();
                        for (int j = 0; j < thisTrack.ChildNodes.Count; j++)
                        {
                            if (thisTrack.ChildNodes[j].Name == "trackName")
                            {
                                newTrack.TrackName = thisTrack.ChildNodes[j].FirstChild.Value;
                            }
                            if (thisTrack.ChildNodes[j].Name == "musicalPart")
                            {
                                XmlNode thisPart = thisTrack.ChildNodes[j];
                                int PartStartTime = Convert.ToInt32(thisPart.FirstChild.FirstChild.Value);
                                for (int k = 0; k < thisPart.ChildNodes.Count; k++)
                                {
                                    if (thisPart.ChildNodes[k].Name == "note")
                                    {
                                        note newNote = new note();
                                        newNote.NoteNum = NoteNum;
                                        XmlNode thisNote = thisPart.ChildNodes[k];
                                        XmlNode inThisNote = thisNote.FirstChild;
                                        newNote.NoteTimeOn = Convert.ToInt32(inThisNote.FirstChild.Value) + PartStartTime;
                                        inThisNote = inThisNote.NextSibling;
                                        newNote.NoteTimeOff = newNote.NoteTimeOn + Convert.ToInt32(inThisNote.FirstChild.Value);
                                        inThisNote = inThisNote.NextSibling;
                                        newNote.NoteKey = Convert.ToInt32(inThisNote.FirstChild.Value);
                                        inThisNote = inThisNote.NextSibling;
                                        inThisNote = inThisNote.NextSibling;
                                        newNote.NoteLyric = inThisNote.FirstChild.Value;
                                        NoteNum++;
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
                lyric = new Lyric(this, true);
                return true;
            }
            else
            {
                MessageBox.Show("The Vsqx is invalid or empty.", "Import");
                return false;
            }
        }
        public bool ImportVsq4(List<string> filenames)
        {
            if (filenames.Count != 1)
            {
                MessageBox.Show("Cannot Import more than one vsqx.", "Import");
                return false;
            }
            XmlDocument vsq4 = new XmlDocument();
            vsq4.Load(filenames[0]);
            //Set up tracklist
            XmlNode root = vsq4.FirstChild.NextSibling;
            XmlNode MasterTrack = null;
            ProjectName = Path.GetFileNameWithoutExtension(filenames[0]);
            TrackList = new List<track>();
            timeSigList = new List<timeSig>();
            tempoList = new List<tempo>();
            int TrackNum = 0;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                if (root.ChildNodes[i].Name == "masterTrack")
                {
                    MasterTrack = root.ChildNodes[i];
                    for (int j = 0; j < MasterTrack.ChildNodes.Count; j++)
                    {
                        if (MasterTrack.ChildNodes[j].Name == "preMeasure")
                        {
                            PreMeasure = Convert.ToInt32(MasterTrack.ChildNodes[j].FirstChild.Value);
                        }
                        if (MasterTrack.ChildNodes[j].Name == "timeSig")
                        {
                            timeSig newtimeSig = new timeSig();
                            XmlNode inTimeSig = MasterTrack.ChildNodes[j].FirstChild;
                            newtimeSig.posMes = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            inTimeSig = inTimeSig.NextSibling;
                            newtimeSig.nume = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            inTimeSig = inTimeSig.NextSibling;
                            newtimeSig.denomi = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            timeSigList.Add(newtimeSig);
                        }
                        if (MasterTrack.ChildNodes[j].Name == "tempo")
                        {
                            tempo newtempo = new tempo();
                            XmlNode inTimeSig = MasterTrack.ChildNodes[j].FirstChild;
                            newtempo.posTick = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            inTimeSig = inTimeSig.NextSibling;
                            newtempo.bpm_times100 = Convert.ToInt32(inTimeSig.FirstChild.Value);
                            tempoList.Add(newtempo);
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
                        int NoteNum = 0;
                        XmlNode thisTrack = root.ChildNodes[i];
                        track newTrack = new track();
                        newTrack.TrackNum = TrackNum;
                        //Set up notelist for every track
                        newTrack.NoteList = new List<note>();
                        for (int j = 0; j < thisTrack.ChildNodes.Count; j++)
                        {
                            if (thisTrack.ChildNodes[j].Name == "name")
                            {
                                newTrack.TrackName = thisTrack.ChildNodes[j].FirstChild.Value;
                            }
                            if (thisTrack.ChildNodes[j].Name == "vsPart")
                            {
                                XmlNode thisPart = thisTrack.ChildNodes[j];
                                int PartStartTime = Convert.ToInt32(thisPart.FirstChild.FirstChild.Value);
                                for (int k = 0; k < thisPart.ChildNodes.Count; k++)
                                {
                                    if (thisPart.ChildNodes[k].Name == "note")
                                    {
                                        note newNote = new note();
                                        newNote.NoteNum = NoteNum;
                                        XmlNode thisNote = thisPart.ChildNodes[k];
                                        XmlNode inThisNote = thisNote.FirstChild;
                                        newNote.NoteTimeOn = Convert.ToInt32(inThisNote.FirstChild.Value) + PartStartTime;
                                        inThisNote = inThisNote.NextSibling;
                                        newNote.NoteTimeOff = newNote.NoteTimeOn + Convert.ToInt32(inThisNote.FirstChild.Value);
                                        inThisNote = inThisNote.NextSibling;
                                        newNote.NoteKey = Convert.ToInt32(inThisNote.FirstChild.Value);
                                        inThisNote = inThisNote.NextSibling;
                                        inThisNote = inThisNote.NextSibling;
                                        newNote.NoteLyric = inThisNote.FirstChild.Value;
                                        NoteNum++;
                                        newTrack.NoteList.Add(newNote);
                                    }
                                    else if (thisPart.ChildNodes[k].Name == "cc")
                                    {
                                        pit newPit = new pit();
                                        XmlNode thisNote = thisPart.ChildNodes[k];
                                        XmlNode inThisNote = thisNote.FirstChild;
                                        newPit.pos= Convert.ToInt32(inThisNote.FirstChild.Value) + PartStartTime;
                                        inThisNote = inThisNote.NextSibling;
                                        if(((XmlElement)inThisNote).GetAttribute("id")!="P")
                                        {
                                            continue;
                                        }
                                        //值的计算
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
                lyric = new Lyric(this, true);
                return true;
            }
            else
            {
                MessageBox.Show("The Vsqx is invalid or empty.", "Import");
                return false;
            }
        }
        public bool ImportUst(List<string> filenames)
        {
            int trackcount = 0;
            TrackList = new List<track>();
            foreach (string filename in filenames)
            {
                bool USTValid = false;
                StreamReader USTReader = new StreamReader(filename, Encoding.GetEncoding("Shift-JIS"));
                int NoteNum = 0;
                int Time = 0;
                PreMeasure = 1;
                //建立音轨列表
                track newTrack = new track();
                newTrack.TrackNum = 0;
                newTrack.TrackName = Path.GetFileName(filename).Replace(".ust", "");
                if (trackcount == 0)
                {
                    timeSig firsttimeSig = new timeSig();
                    timeSigList = new List<timeSig>();
                    firsttimeSig.denomi = 4;
                    firsttimeSig.nume = 4;
                    firsttimeSig.posMes = 0;
                    timeSigList.Add(firsttimeSig);
                    tempoList = new List<tempo>();
                }
                //建立音符列表，读取音符信息
                for (string ReadBuf = "Starting"; ReadBuf != "[#TRACKEND]" && ReadBuf != null; ReadBuf = USTReader.ReadLine())
                {
                    if (trackcount == 0)
                    {
                        if (ReadBuf.Contains("ProjectName="))
                        {
                            ProjectName = ReadBuf.Remove(0, 12);
                        }
                        if (ReadBuf.Contains("Tempo="))
                        {
                            tempo firstTempo = new tempo();
                            firstTempo.posTick = 0;
                            double bpm;
                            if (double.TryParse(ReadBuf.Remove(0, 6), out bpm))
                            {
                                firstTempo.bpm_times100 = (int)(bpm * 100);
                            }
                            tempoList.Add(firstTempo);
                        }
                    }
                    if (ReadBuf.Contains("[#0000]"))
                    {
                        USTValid = false;
                        newTrack.NoteList = new List<note>();
                        note newNote = new note();
                        newNote.NoteNum = NoteNum;
                        newNote.NoteIDforUTAU = ReadBuf.Substring(2, 4);
                        bool NoteIsValid = false;
                        bool tempoTempFlag = false;
                        for (ReadBuf = USTReader.ReadLine(); ReadBuf != "[#TRACKEND]" && ReadBuf != null; ReadBuf = USTReader.ReadLine())
                        {
                            if (ReadBuf.Contains("[#"))
                            {
                                if (NoteIsValid)
                                {
                                    newTrack.NoteList.Add(newNote);
                                    NoteNum++;
                                }
                                newNote = new note();
                                newNote.NoteNum = NoteNum;
                                newNote.NoteIDforUTAU = ReadBuf.Substring(2, 4);
                                NoteIsValid = false;
                            }
                            if (ReadBuf.Contains("Length="))
                            {
                                newNote.NoteTimeOn = Time;
                                Time += Convert.ToInt32(ReadBuf.Substring(7, ReadBuf.Length - 7));
                                newNote.NoteTimeOff = Time;
                                if (tempoTempFlag)
                                {
                                    tempoList[tempoList.Count - 1].posTick = newNote.NoteTimeOn;
                                }
                            }
                            if (ReadBuf.Contains("Lyric="))
                            {
                                if (ReadBuf.Substring(6, ReadBuf.Length - 6) != "R"&& ReadBuf.Substring(6, ReadBuf.Length - 6) != "r" && newNote.GetNoteLength() != 0)
                                {
                                    NoteIsValid = true;
                                    USTValid = true;
                                    newNote.NoteLyric = ReadBuf.Substring(6, ReadBuf.Length - 6);
                                }
                            }
                            if (trackcount == 0)
                            {
                                if (ReadBuf.Contains("Tempo="))
                                {
                                    tempo newTempo = new tempo();
                                    try
                                    {
                                        newTempo.posTick = newNote.NoteTimeOn;
                                    }
                                    catch
                                    {
                                        tempoTempFlag = true;
                                    }
                                    newTempo.bpm_times100 = (int)(100 * double.Parse(ReadBuf.Substring(6, ReadBuf.Length - 6)));
                                    tempoList.Add(newTempo);
                                }
                            }
                            if (ReadBuf.Contains("NoteNum="))
                            {
                                newNote.NoteKey = Convert.ToInt32(ReadBuf.Substring(8, ReadBuf.Length - 8));
                            }
                        }
                    }
                }
                if (!USTValid)
                {
                    MessageBox.Show("The Ust is invalid or empty.", "Import");
                    return false;
                }
                foreach (note Note in newTrack.NoteList)
                {
                    Note.NoteTimeOn += 1920;
                    Note.NoteTimeOff += 1920;
                }
                if (trackcount == 0)
                {
                    foreach (tempo Tempo in tempoList)
                    {
                        if (tempoList.IndexOf(Tempo) != 0)
                        {
                            Tempo.posTick += 1920;
                        }
                    }
                }
                TrackList.Add(newTrack);
                trackcount++;
            }
            lyric = new Lyric(this, true);
            return true;
        }
        public bool ImportCcs(List<string> filenames)
        {
            if (filenames.Count != 1)
            {
                MessageBox.Show("Cannot Import more than one ccs.", "Import");
                return false;
            }
            XmlDocument ccs = new XmlDocument();
            ccs.Load(filenames[0]);
            PreMeasure = 1;
            //Set up tracklist
            XmlElement Scenario = (XmlElement)ccs.FirstChild.NextSibling;
            XmlElement Scene = (XmlElement)((XmlElement)Scenario.GetElementsByTagName("Sequence")[0]).GetElementsByTagName("Scene")[0];
            XmlElement Units = (XmlElement)Scene.GetElementsByTagName("Units")[0];
            XmlElement Groups = (XmlElement)Scene.GetElementsByTagName("Groups")[0];
            ProjectName = Path.GetFileNameWithoutExtension(filenames[0]);
            bool CcsValid = false;
            TrackList = new List<track>();
            int TrackNum = 0;
            bool TempoFinished = false;
            foreach(XmlElement Unit in Units.GetElementsByTagName("Unit"))
            {
                if (Unit.GetAttribute("Category") == "SingerSong")
                {
                    if(!TempoFinished)
                    {
                        tempoList = new List<tempo>();
                        XmlElement Tempo = (XmlElement)((XmlElement)(Unit.GetElementsByTagName("Song")[0])).GetElementsByTagName("Tempo")[0];
                        foreach (XmlElement Sound in Tempo.GetElementsByTagName("Sound"))
                        {
                            tempo newTempo = new tempo();
                            newTempo.posTick = int.Parse(Sound.GetAttribute("Clock")) / 2;
                            newTempo.bpm_times100 = (int)(double.Parse(Sound.GetAttribute("Tempo")) * 100);
                            tempoList.Add(newTempo);
                        }
                        timeSigList = new List<timeSig>();
                        XmlElement Beat = (XmlElement)((XmlElement)(Unit.GetElementsByTagName("Song")[0])).GetElementsByTagName("Beat")[0];
                        int time = 0;
                        int Mes = 0;
                        int Beats = 4;
                        int BeatType = 4;
                        foreach (XmlElement Time in Beat.GetElementsByTagName("Time"))
                        {
                            timeSig newtimeSig = new timeSig();
                            newtimeSig.posMes = Mes + (int.Parse(Time.GetAttribute("Clock")) - time) / (Beats * 960 * 4 / BeatType);
                            Mes = newtimeSig.posMes;
                            time = int.Parse(Time.GetAttribute("Clock"));
                            newtimeSig.nume = int.Parse(Time.GetAttribute("Beats"));
                            Beats = newtimeSig.nume;
                            newtimeSig.denomi = int.Parse(Time.GetAttribute("BeatType"));
                            BeatType = newtimeSig.denomi;
                            timeSigList.Add(newtimeSig);
                        }
                    }
                    track newTrack = new track();
                    newTrack.TrackNum = TrackNum;
                    string GroupId = Unit.GetAttribute("Group");
                    foreach(XmlElement Group in Groups.GetElementsByTagName("Group"))
                    {
                        if (Group.GetAttribute("Id") == GroupId)
                        {
                            newTrack.TrackName = Group.GetAttribute("Name");
                        }
                    }
                    newTrack.NoteList = new List<note>();
                    int NoteNum = 0;
                    foreach (XmlElement Note in ((XmlElement)((XmlElement)Unit.GetElementsByTagName("Song")[0]).GetElementsByTagName("Score")[0]).GetElementsByTagName("Note"))
                    {
                        note newNote = new note();
                        newNote.NoteNum = NoteNum;
                        newNote.NoteTimeOn = int.Parse(Note.GetAttribute("Clock")) / 2;
                        newNote.NoteTimeOff = newNote.NoteTimeOn + int.Parse(Note.GetAttribute("Duration")) / 2;
                        newNote.NoteKey = int.Parse(Note.GetAttribute("PitchStep")) + (int.Parse(Note.GetAttribute("PitchOctave")) + 1) * 12;
                        newNote.NoteLyric = Note.GetAttribute("Lyric");
                        NoteNum++;
                        newTrack.NoteList.Add(newNote);
                    }
                    if(newTrack.NoteList.Count>0)
                    {
                        TrackList.Add(newTrack);
                        TrackNum++;
                        CcsValid = true; 
                    }
                }
            }
            if(!CcsValid)
            {
                MessageBox.Show("The Ccs is invalid or empty.", "Import");
                return false;
            }
            lyric = new Lyric(this, true);
            return true;
        }
        public void ExportUst(string filepath)
        {
            string OmitList = "";
            for (int tracknum = 0; tracknum < TrackList.Count; tracknum++)
            {
                string ustcontents = "[#VERSION]" + Environment.NewLine;
                ustcontents += "UST Version1.2" + Environment.NewLine;
                ustcontents += "[#SETTING]" + Environment.NewLine;
                foreach (timeSig timesig in timeSigList)
                {
                    if (timesig.denomi != 4 || timesig.nume != 4)
                    {
                        if (tracknum == 0)
                        {
                            OmitList += "Meter change omitted at Measure [" + timesig.posMes + "] : " + timesig.nume + "/" + timesig.denomi + Environment.NewLine;
                        }
                    }
                }
                foreach (tempo tempo in tempoList)
                {
                    if (tempo.posTick != 0)
                    {
                        if (tracknum == 0)
                        {
                            OmitList += "Tempo change omitted at Tick [" + tempo.posTick + "] : " + (((double)(tempo.bpm_times100)) / 100).ToString("F2") + Environment.NewLine;
                        }
                    }
                    else
                    {
                        ustcontents += "Tempo=" + (((double)(tempo.bpm_times100)) / 100).ToString("F2") + Environment.NewLine;
                    }
                }
                int pos = 0;
                int Rcount = 0;
                ustcontents += "Tracks=1" + Environment.NewLine;
                ustcontents += "ProjectName=" + TrackList[tracknum].TrackName + Environment.NewLine;
                ustcontents += "Mode2=True" + Environment.NewLine;
                for (int notenum = 0; notenum < TrackList[tracknum].NoteList.Count; notenum++)
                {
                    note thisnote = TrackList[tracknum].NoteList[notenum];
                    if (pos < thisnote.NoteTimeOn)
                    {
                        ustcontents += "[#" + (notenum + Rcount).ToString("D4") + "]" + Environment.NewLine;
                        ustcontents += "Length=" + (thisnote.NoteTimeOn - pos).ToString() + Environment.NewLine;
                        ustcontents += "Lyric=R" + Environment.NewLine;
                        ustcontents += "NoteNum=60" + Environment.NewLine;
                        ustcontents += "PreUtterance=" + Environment.NewLine;
                        Rcount++;
                    }
                    ustcontents += "[#" + (notenum + Rcount).ToString("D4") + "]" + Environment.NewLine;
                    ustcontents += "Length=" + thisnote.GetNoteLength().ToString() + Environment.NewLine;
                    ustcontents += "Lyric=" + thisnote.NoteLyric + Environment.NewLine;
                    ustcontents += "NoteNum=" + thisnote.NoteKey + Environment.NewLine;
                    ustcontents += "PreUtterance=" + Environment.NewLine;
                    pos = thisnote.NoteTimeOff;
                }
                ustcontents += "[#TRACKEND]";
                File.WriteAllText(filepath + "\\" + ProjectName + "_" + tracknum.ToString() + "_" + TrackList[tracknum].TrackName.Replace("\\","").Replace("/","").Replace(".","") + ".ust", ustcontents, Encoding.GetEncoding("Shift-JIS"));
            }
            MessageBox.Show(OmitList + "Ust is successfully exported.", "Export Ust");
        }
        public void ExportVsq4(string filename)
        {
            XmlDocument vsq4 = new XmlDocument();
            string template = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\r\n<vsq4 xmlns=\"http://www.yamaha.co.jp/vocaloid/schema/vsq4/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.yamaha.co.jp/vocaloid/schema/vsq4/ vsq4.xsd\">\r\n  <vender><![CDATA[Yamaha corporation]]></vender>\r\n  <version><![CDATA[4.0.0.3]]></version>\r\n  <vVoiceTable>\r\n    <vVoice>\r\n      <bs>0</bs>\r\n      <pc>0</pc>\r\n      <id><![CDATA[BCXDC6CZLSZHZCB4]]></id>\r\n      <name><![CDATA[VY2V3]]></name>\r\n      <vPrm>\r\n        <bre>0</bre>\r\n        <bri>0</bri>\r\n        <cle>0</cle>\r\n        <gen>0</gen>\r\n        <ope>0</ope>\r\n      </vPrm>\r\n    </vVoice>\r\n  </vVoiceTable>\r\n  <mixer>\r\n    <masterUnit>\r\n      <oDev>0</oDev>\r\n      <rLvl>0</rLvl>\r\n      <vol>0</vol>\r\n    </masterUnit>\r\n    <vsUnit>\r\n      <tNo>0</tNo>\r\n      <iGin>0</iGin>\r\n      <sLvl>-898</sLvl>\r\n      <sEnable>0</sEnable>\r\n      <m>0</m>\r\n      <s>0</s>\r\n      <pan>64</pan>\r\n      <vol>0</vol>\r\n    </vsUnit>\r\n    <monoUnit>\r\n      <iGin>0</iGin>\r\n      <sLvl>-898</sLvl>\r\n      <sEnable>0</sEnable>\r\n      <m>0</m>\r\n      <s>0</s>\r\n      <pan>64</pan>\r\n      <vol>0</vol>\r\n    </monoUnit>\r\n    <stUnit>\r\n      <iGin>0</iGin>\r\n      <m>0</m>\r\n      <s>0</s>\r\n      <vol>-129</vol>\r\n    </stUnit>\r\n  </mixer>\r\n  <masterTrack>\r\n    <seqName><![CDATA[Untitled0]]></seqName>\r\n    <comment><![CDATA[New VSQ File]]></comment>\r\n    <resolution>480</resolution>\r\n    <preMeasure>4</preMeasure>\r\n    <timeSig>\r\n      <m>0</m>\r\n      <nu>4</nu>\r\n      <de>4</de>\r\n    </timeSig>\r\n    <tempo>\r\n      <t>0</t>\r\n      <v>12000</v>\r\n    </tempo>\r\n  </masterTrack>\r\n  <vsTrack>\r\n    <tNo>0</tNo>\r\n    <name><![CDATA[Track]]></name>\r\n    <comment><![CDATA[Track]]></comment>\r\n    <vsPart>\r\n      <t>7680</t>\r\n      <playTime>61440</playTime>\r\n      <name><![CDATA[NewPart]]></name>\r\n      <comment><![CDATA[New Musical Part]]></comment>\r\n      <sPlug>\r\n        <id><![CDATA[ACA9C502-A04B-42b5-B2EB-5CEA36D16FCE]]></id>\r\n        <name><![CDATA[VOCALOID2 Compatible Style]]></name>\r\n        <version><![CDATA[3.0.0.1]]></version>\r\n      </sPlug>\r\n      <pStyle>\r\n        <v id=\"accent\">50</v>\r\n        <v id=\"bendDep\">8</v>\r\n        <v id=\"bendLen\">0</v>\r\n        <v id=\"decay\">50</v>\r\n        <v id=\"fallPort\">0</v>\r\n        <v id=\"opening\">127</v>\r\n        <v id=\"risePort\">0</v>\r\n      </pStyle>\r\n      <singer>\r\n        <t>0</t>\r\n        <bs>0</bs>\r\n        <pc>0</pc>\r\n      </singer>\r\n      <plane>0</plane>\r\n    </vsPart>\r\n  </vsTrack>\r\n  <monoTrack>\r\n  </monoTrack>\r\n  <stTrack>\r\n  </stTrack>\r\n  <aux>\r\n    <id><![CDATA[AUX_VST_HOST_CHUNK_INFO]]></id>\r\n    <content><![CDATA[VlNDSwAAAAADAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=]]></content>\r\n  </aux>\r\n</vsq4>";
            vsq4.LoadXml(template);
            XmlElement root = (XmlElement)(vsq4.FirstChild.NextSibling);
            XmlElement mixer = (XmlElement)(root.GetElementsByTagName("mixer"))[0];
            XmlElement masterTrack = (XmlElement)(root.GetElementsByTagName("masterTrack"))[0];
            XmlElement emptyTrack = (XmlElement)(root.GetElementsByTagName("vsTrack"))[0];
            XmlElement emptyUnit = (XmlElement)(mixer.GetElementsByTagName("vsUnit"))[0];
            XmlElement preMeasure = (XmlElement)(masterTrack.GetElementsByTagName("preMeasure"))[0];
            preMeasure.InnerText = PreMeasure.ToString();
            XmlElement firstTempo = (XmlElement)(masterTrack.GetElementsByTagName("tempo"))[0];
            firstTempo.GetElementsByTagName("v")[0].FirstChild.Value = tempoList[0].bpm_times100.ToString();
            XmlElement firsttimeSig = (XmlElement)(masterTrack.GetElementsByTagName("timeSig"))[0];
            firsttimeSig.GetElementsByTagName("nu")[0].FirstChild.Value = timeSigList[0].nume.ToString();
            firsttimeSig.GetElementsByTagName("de")[0].FirstChild.Value = timeSigList[0].denomi.ToString();
            if (tempoList.Count > 1)
            {
                for (int i = 1; i < tempoList.Count; i++)
                {
                    XmlElement newTempo = (XmlElement)firstTempo.Clone();
                    newTempo.GetElementsByTagName("t")[0].FirstChild.Value = tempoList[i].posTick.ToString();
                    newTempo.GetElementsByTagName("v")[0].FirstChild.Value = tempoList[i].bpm_times100.ToString();
                    masterTrack.InsertAfter(newTempo, firstTempo);
                    firstTempo = newTempo;
                }
            }
            if (timeSigList.Count > 1)
            {
                for (int i = 1; i < timeSigList.Count; i++)
                {
                    XmlElement newtimeSig = (XmlElement)firsttimeSig.Clone();
                    newtimeSig.GetElementsByTagName("m")[0].FirstChild.Value = timeSigList[i].posMes.ToString();
                    newtimeSig.GetElementsByTagName("nu")[0].FirstChild.Value = timeSigList[i].nume.ToString();
                    newtimeSig.GetElementsByTagName("de")[0].FirstChild.Value = timeSigList[i].denomi.ToString();
                    masterTrack.InsertAfter(newtimeSig, firsttimeSig);
                    firsttimeSig = newtimeSig;
                }
            }
            if (TrackList.Count > 0)
            {
                for (int tracknum = 0; tracknum < TrackList.Count; tracknum++)
                {
                    XmlElement newTrack = (XmlElement)emptyTrack.Clone();
                    newTrack.GetElementsByTagName("tNo")[0].FirstChild.Value = (tracknum + 1).ToString();
                    emptyTrack.GetElementsByTagName("name")[0].FirstChild.Value = TrackList[tracknum].TrackName;
                    XmlElement Part = (XmlElement)emptyTrack.GetElementsByTagName("vsPart")[0];
                    int pos = 0;
                    int mes = 0;
                    int nume = 4;
                    int denomi = 4;
                    foreach (timeSig timesig in timeSigList)
                    {
                        if (timesig.posMes > PreMeasure)
                        {
                            break;
                        }
                        else
                        {
                            pos += (timesig.posMes - mes) * nume * 4 * 480 / denomi;
                            mes = timesig.posMes;
                            nume = timesig.nume;
                            denomi = timesig.denomi;
                        }
                    }
                    pos += (PreMeasure - mes) * nume * 4 * 480 / denomi;
                    Part.GetElementsByTagName("t")[0].InnerText = pos.ToString();
                    int PartStartTime = pos;
                    int time = 0;
                    track thisTrack = TrackList[tracknum];
                    XmlElement lastnote = (XmlElement)Part.GetElementsByTagName("singer")[0];
                    for (int notenum = 0; notenum < thisTrack.NoteList.Count; notenum++)
                    {
                        note thisnote = thisTrack.NoteList[notenum];
                        XmlElement note = vsq4.CreateElement("note", vsq4.DocumentElement.NamespaceURI);
                        XmlElement t = vsq4.CreateElement("t", vsq4.DocumentElement.NamespaceURI);
                        t.InnerText = (thisnote.NoteTimeOn - PartStartTime).ToString();
                        note.AppendChild(t);
                        XmlElement dur = vsq4.CreateElement("dur", vsq4.DocumentElement.NamespaceURI);
                        dur.InnerText = thisnote.GetNoteLength().ToString();
                        note.AppendChild(dur);
                        XmlElement n = vsq4.CreateElement("n", vsq4.DocumentElement.NamespaceURI);
                        n.InnerText = thisnote.NoteKey.ToString();
                        note.AppendChild(n);
                        XmlElement v = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v.InnerText = "64";
                        note.AppendChild(v);
                        XmlElement y = vsq4.CreateElement("y", vsq4.DocumentElement.NamespaceURI);
                        XmlCDataSection y_cdata = vsq4.CreateCDataSection(thisnote.NoteLyric);
                        y.AppendChild(y_cdata);
                        note.AppendChild(y);
                        XmlElement p = vsq4.CreateElement("p", vsq4.DocumentElement.NamespaceURI);
                        XmlCDataSection p_cdata = vsq4.CreateCDataSection("a");
                        p.AppendChild(p_cdata);
                        note.AppendChild(p);
                        XmlElement nStyle = vsq4.CreateElement("nStyle", vsq4.DocumentElement.NamespaceURI);
                        XmlElement v1 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v1.SetAttribute("id", "accent");
                        v1.InnerText = "50";
                        nStyle.AppendChild(v1);
                        XmlElement v2 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v2.SetAttribute("id", "bendDep");
                        v2.InnerText = "0";
                        nStyle.AppendChild(v2);
                        XmlElement v3 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v3.SetAttribute("id", "bendLen");
                        v3.InnerText = "0";
                        nStyle.AppendChild(v3);
                        XmlElement v4 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v4.SetAttribute("id", "decay");
                        v4.InnerText = "50";
                        nStyle.AppendChild(v4);
                        XmlElement v5 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v5.SetAttribute("id", "fallPort");
                        v5.InnerText = "0";
                        nStyle.AppendChild(v5);
                        XmlElement v6 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v6.SetAttribute("id", "opening");
                        v6.InnerText = "127";
                        nStyle.AppendChild(v6);
                        XmlElement v7 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v7.SetAttribute("id", "risePort");
                        v7.InnerText = "0";
                        nStyle.AppendChild(v7);
                        XmlElement v8 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v8.SetAttribute("id", "vibLen");
                        v8.InnerText = "0";
                        nStyle.AppendChild(v8);
                        XmlElement v9 = vsq4.CreateElement("v", vsq4.DocumentElement.NamespaceURI);
                        v9.SetAttribute("id", "vibType");
                        v9.InnerText = "0";
                        nStyle.AppendChild(v9);
                        note.AppendChild(nStyle);
                        Part.InsertAfter(note, lastnote);
                        lastnote = note;
                        time = thisnote.NoteTimeOff;
                    }
                    Part.GetElementsByTagName("playTime")[0].InnerText = time.ToString();
                    root.InsertAfter(newTrack, emptyTrack);
                    emptyTrack = newTrack;
                    XmlElement newUnit = (XmlElement)emptyUnit.Clone();
                    newUnit.GetElementsByTagName("tNo")[0].FirstChild.Value = (tracknum + 1).ToString();
                    mixer.InsertAfter(newUnit, emptyUnit);
                    emptyUnit = newUnit;
                }
                root.RemoveChild(emptyTrack);
                mixer.RemoveChild(emptyUnit);
            }
            vsq4.Save(filename);
            MessageBox.Show("Vsqx is successfully exported." + Environment.NewLine + "If it only pronounces \"a\", please select all notes and run \"Lyrics\"→\"Convert Phonemes\". ", "Export Vsqx");
        }
        public void ExportCcs(string filename)
        {
            XmlDocument ccs = new XmlDocument();
            string template = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Scenario Code=\"7251BC4B6168E7B2992FA620BD3E1E77\">\r\n  <Generation>\r\n    <Author Version=\"3.2.21.2\" />\r\n    <TTS Version=\"3.1.0\">\r\n      <Dictionary Version=\"1.4.0\" />\r\n      <SoundSources />\r\n    </TTS>\r\n    <SVSS Version=\"3.0.5\">\r\n      <Dictionary Version=\"1.0.0\" />\r\n      <SoundSources>\r\n        <SoundSource Version=\"1.0.0\" Id=\"XSV-JPF-W\" Name=\"緑咲 香澄\" />\r\n        <SoundSource Version=\"1.0.0\" Id=\"XSV-JPM-P\" Name=\"赤咲 湊\" />\r\n      </SoundSources>\r\n    </SVSS>\r\n  </Generation>\r\n  <Sequence Id=\"\">\r\n    <Scene Id=\"\">\r\n      <Units>\r\n        <Unit Version=\"1.0\" Id=\"\" Category=\"SingerSong\" Group=\"7de5f694-4b60-493d-b6b0-16f6b56deb1f\" StartTime=\"00:00:00\" Duration=\"00:00:02\" CastId=\"XSV-JPM-P\" Language=\"Japanese\">\r\n          <Song Version=\"1.02\">\r\n            <Tempo>\r\n              <Sound Clock=\"0\" Tempo=\"120\" />\r\n            </Tempo>\r\n            <Beat>\r\n              <Time Clock=\"0\" Beats=\"4\" BeatType=\"4\" />\r\n            </Beat>\r\n            <Score>\r\n              <Key Clock=\"0\" Fifths=\"0\" Mode=\"0\" />\r\n            </Score>\r\n          </Song>\r\n        </Unit>\r\n      </Units>\r\n      <Groups>\r\n        <Group Version=\"1.0\" Id=\"7de5f694-4b60-493d-b6b0-16f6b56deb1f\" Category=\"SingerSong\" Name=\"ソング 1\" Color=\"#FFAF1F14\" Volume=\"0\" Pan=\"0\" IsSolo=\"false\" IsMuted=\"false\" CastId=\"XSV-JPM-P\" Language=\"Japanese\" />\r\n      </Groups>\r\n      <SoundSetting Rhythm=\"4/4\" Tempo=\"78\" />\r\n    </Scene>\r\n  </Sequence>\r\n</Scenario>";
            ccs.LoadXml(template);
            XmlElement Scenario = (XmlElement)ccs.FirstChild.NextSibling;
            XmlElement Scene = (XmlElement)((XmlElement)Scenario.GetElementsByTagName("Sequence")[0]).GetElementsByTagName("Scene")[0];
            XmlElement Units = (XmlElement)Scene.GetElementsByTagName("Units")[0];
            XmlElement EmptyUnit = (XmlElement)Units.GetElementsByTagName("Unit")[0];
            XmlElement Groups = (XmlElement)Scene.GetElementsByTagName("Groups")[0];
            XmlElement EmptyGroup = (XmlElement)Groups.GetElementsByTagName("Group")[0];
            XmlElement AllTempo = (XmlElement)((XmlElement)(EmptyUnit.GetElementsByTagName("Song")[0])).GetElementsByTagName("Tempo")[0];
            XmlElement firstTempo = (XmlElement)AllTempo.GetElementsByTagName("Sound")[0];
            firstTempo.SetAttribute("Tempo", (0.01 * tempoList[0].bpm_times100).ToString());
            for (int i = 1; i < tempoList.Count; i++)
            {
                XmlElement newTempo = (XmlElement)firstTempo.Clone();
                newTempo.SetAttribute("Tempo", (0.01 * tempoList[i].bpm_times100).ToString());
                newTempo.SetAttribute("Clock", (tempoList[i].posTick * 2).ToString());
                AllTempo.InsertAfter(newTempo, firstTempo);
                firstTempo = newTempo;
            }
            XmlElement AllBeat = (XmlElement)((XmlElement)(((XmlElement)Units.GetElementsByTagName("Unit")[0]).GetElementsByTagName("Song")[0])).GetElementsByTagName("Beat")[0];
            XmlElement firstBeat = (XmlElement)AllBeat.GetElementsByTagName("Time")[0];
            firstBeat.SetAttribute("Beats", timeSigList[0].nume.ToString());
            firstBeat.SetAttribute("BeatType", timeSigList[0].denomi.ToString());
            int pos = 0;
            for (int i = 1; i < timeSigList.Count; i++)
            {
                XmlElement newBeat = (XmlElement)firstBeat.Clone();
                pos += (timeSigList[i].posMes - timeSigList[i - 1].posMes) * 960 * 4 * timeSigList[i - 1].nume / timeSigList[i - 1].denomi;
                newBeat.SetAttribute("Clock", pos.ToString());
                newBeat.SetAttribute("Beats", timeSigList[i].nume.ToString());
                newBeat.SetAttribute("BeatType", timeSigList[i].denomi.ToString());
                AllBeat.InsertAfter(newBeat, firstBeat);
                firstBeat = newBeat;
            }
            List<string> IdList = new List<string>();
            Random IDRandom = new Random();
            for (int tracknum = 0; tracknum < TrackList.Count; tracknum++)
            {
                track thisTrack = TrackList[tracknum];
                XmlElement newUnit = (XmlElement)EmptyUnit.Clone();
                XmlElement newGroup = (XmlElement)EmptyGroup.Clone();
                while (IdList.Count <= tracknum)
                {
                    string Id = EmptyUnit.GetAttribute("Group");
                    Id = Id.Remove(30, 6);
                    Id += IDRandom.Next(999999).ToString("D6");
                    if (!IdList.Contains(Id))
                    {
                        IdList.Add(Id);
                    }
                }
                EmptyUnit.SetAttribute("Group", IdList[tracknum]);
                EmptyGroup.SetAttribute("Id", IdList[tracknum]);
                EmptyGroup.SetAttribute("Name", thisTrack.TrackName);
                XmlElement Song = (XmlElement)EmptyUnit.GetElementsByTagName("Song")[0];
                XmlElement Tempo = (XmlElement)Song.GetElementsByTagName("Tempo")[0];
                XmlElement Beat = (XmlElement)Song.GetElementsByTagName("Beat")[0];
                XmlElement Score = (XmlElement)Song.GetElementsByTagName("Score")[0];
                Song.ReplaceChild(AllTempo.Clone(), Tempo);
                Song.ReplaceChild(AllBeat.Clone(), Beat);
                foreach(note Note in thisTrack.NoteList)
                {
                    XmlElement note = ccs.CreateElement("Note", ccs.DocumentElement.NamespaceURI);
                    note.SetAttribute("Clock", (Note.NoteTimeOn * 2).ToString());
                    note.SetAttribute("PitchStep", (Note.NoteKey % 12).ToString());
                    note.SetAttribute("PitchOctave", (Note.NoteKey / 12 - 1).ToString());
                    note.SetAttribute("Duration", (Note.GetNoteLength() * 2).ToString());
                    note.SetAttribute("Lyric", Note.NoteLyric);
                    Score.AppendChild(note);
                }
                int posTick = 0;
                int posSecond = 0;
                int LastTick = thisTrack.NoteList[thisTrack.NoteList.Count - 1].NoteTimeOff;
                for (int j = 1; j < tempoList.Count; j++)
                {
                    posTick = tempoList[j].posTick;
                    posSecond += (int)((tempoList[j].posTick - tempoList[j - 1].posTick) / 8 / (0.01 * (tempoList[j - 1].bpm_times100)) + 1);
                }
                if (posTick < LastTick)
                {
                    posSecond += (int)((LastTick - posTick) / 8 / (0.01 * (tempoList[tempoList.Count - 1].bpm_times100)) + 1);
                }
                TimeSpan timespan = new TimeSpan(0, 0, posSecond);
                EmptyUnit.SetAttribute("Duration", timespan.ToString("c"));
                Units.AppendChild(newUnit);
                Groups.AppendChild(newGroup);
                EmptyUnit = newUnit;
                EmptyGroup = newGroup;
            }
            Units.RemoveChild(EmptyUnit);
            Groups.RemoveChild(EmptyGroup);
            ccs.Save(filename);
            MessageBox.Show("Ccs is successfully exported.","ExportCcs");
        }
        public class timeSig
        {
            public timeSig() { }
            public timeSig(timeSig timeSig)
            {
                posMes = timeSig.posMes;
                nume = timeSig.nume;
                denomi = timeSig.denomi;
            }
            public int posMes;
            public int nume;
            public int denomi;
        }
        public class tempo
        {
            public tempo() { }
            public tempo(tempo tempo)
            {
                posTick = tempo.posTick;
                bpm_times100 = tempo.bpm_times100;
            }
            public int posTick;
            public int bpm_times100;
        }
        public class track
        {
            public track() { }
            public track(track track)
            {
                TrackNum = track.TrackNum;
                TrackName = track.TrackName;
                NoteList = new List<note>();
                foreach(note note in track.NoteList)
                {
                    NoteList.Add(new note(note));
                }
                PitchList = new List<pit>();
                foreach (pit pit in track.PitchList)
                {
                    PitchList.Add(new pit(pit));
                }
            }
            public int TrackNum;
            public string TrackName;
            public List<note> NoteList;
            public List<pit> PitchList;
        }
        public class note
        {
            public note() { }
            public note(note note)
            {
                NoteNum = note.NoteNum;
                NoteKey = note.NoteKey;
                NoteLyric = note.NoteLyric;
                NoteTimeOn = note.NoteTimeOn;
                NoteTimeOff = note.NoteTimeOff;
                NoteValidity = note.NoteValidity;
                NoteIDforUTAU = note.NoteIDforUTAU;
            }
            public int NoteNum;
            public int NoteKey;
            public string NoteLyric;
            public int NoteTimeOn;
            public int NoteTimeOff;
            public bool NoteValidity = true;
            public string NoteIDforUTAU;
            public int GetNoteLength()
            {
                return NoteTimeOff - NoteTimeOn;
            }
        }
        public class pit
        {
            public pit() { }
            public pit(pit pit)
            {
                pos = pit.pos;
                repeat = pit.repeat;
                value = pit.value;
            }
            public int pos;
            public int repeat = 1;
            public double value;
        }
        public enum UtaFormat
        {
            vsq2, //VOCALOID2
            vsq3, //VOCALOID3
            vsq4, //VOCALOID4
            ust, //UTAU
            ccs, //CeVIO
            none
        }
    }
}
