using System.Collections.Generic;
using System.Linq;

namespace UtaFormatix
{
    public class Vpr
    {
        public VprVersion Version { get; set; }
        public string Vender { get; set; }
        public string Title { get; set; }
        public VprMasterTrack MasterTrack { get; set; }
        public List<VprVoice> Voices { get; set; }
        public List<VprTrack> Tracks { get; set; }

        public class VprVersion
        {
            public int Major { get; set; }
            public int Minor { get; set; }
            public int Revision { get; set; }
        }

        public class VprLoop
        {
            public bool IsEnabled { get; set; }
            public int Begin { get; set; }
            public int End { get; set; }
        }

        public class VprGlobal
        {
            public bool IsEnabled { get; set; }
            public int Value { get; set; }
        }

        public class VprTempoEvent
        {
            public int Pos { get; set; }
            public int Value { get; set; }
        }

        public class VprTempo
        {
            public bool IsFolded { get; set; }
            public double Height { get; set; }
            public VprGlobal Global { get; set; }
            public List<VprTempoEvent> Events { get; set; }
        }

        public class VprTimeSigEvent
        {
            public int Bar { get; set; }
            public int Numer { get; set; }
            public int Denom { get; set; }
        }

        public class VprTimeSig
        {
            public bool IsFolded { get; set; }
            public List<VprTimeSigEvent> Events { get; set; }
        }

        public class VprVolumeEvent
        {
            public int Pos { get; set; }
            public int Value { get; set; }
        }

        public class VprMasterTrackVolume
        {
            public bool IsFolded { get; set; }
            public double Height { get; set; }
            public List<VprVolumeEvent> Events { get; set; }
        }

        public class VprMasterTrack
        {
            public int SamplingRate { get; set; }
            public VprLoop Loop { get; set; }
            public VprTempo Tempo { get; set; }
            public VprTimeSig TimeSig { get; set; }
            public VprMasterTrackVolume Volume { get; set; }
        }

        public class VprVoice
        {
            public string CompID { get; set; }
            public string Name { get; set; }
        }

        public class VprTrackVolumeEvent
        {
            public int Pos { get; set; }
            public int Value { get; set; }
        }

        public class VprTrackVolume
        {
            public bool IsFolded { get; set; }
            public double Height { get; set; }
            public List<VprTrackVolumeEvent> Events { get; set; }
        }

        public class VprPanpotEvent
        {
            public int Pos { get; set; }
            public int Value { get; set; }
        }

        public class VprPanpot
        {
            public bool IsFolded { get; set; }
            public double Height { get; set; }
            public List<VprPanpotEvent> Events { get; set; }
        }

        public class VprPartVoice
        {
            public string CompID { get; set; }
            public int LangID { get; set; }
        }

        public class VprParameter
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }

        public class VprMidiEffect
        {
            public string Id { get; set; }
            public bool IsBypassed { get; set; }
            public bool IsFolded { get; set; }
            public List<VprParameter> Parameters { get; set; }
        }

        public class VprExp
        {
            public int Opening { get; set; }
        }

        public class VprWeight
        {
            public int Pre { get; set; }
            public int Post { get; set; }
        }

        public class VprSingingSkill
        {
            public int Duration { get; set; }
            public VprWeight Weight { get; set; }
        }

        public class VprVibrato
        {
            public int Type { get; set; }
            public int Duration { get; set; }
        }

        public class VprNote
        {
            public string Lyric { get; set; }
            public string Phoneme { get; set; }
            public bool IsProtected { get; set; }
            public int Pos { get; set; }
            public int Duration { get; set; }
            public int Number { get; set; }
            public int Velocity { get; set; }
            public VprExp Exp { get; set; }
            public VprSingingSkill SingingSkill { get; set; }
            public VprVibrato Vibrato { get; set; }

            public VprNote CloneBlank()
            {
                return new VprNote
                {
                    Lyric = Lyric,
                    Phoneme = Phoneme,
                    IsProtected = IsProtected,
                    Pos = Pos,
                    Duration = Duration,
                    Number = Number,
                    Velocity = Velocity,
                    Exp = Exp,
                    SingingSkill = SingingSkill,
                    Vibrato = Vibrato
                };
            }
        }

        public class VprPart
        {
            public int Pos { get; set; }
            public int Duration { get; set; }
            public string StyleName { get; set; }
            public VprPartVoice Voice { get; set; }
            public List<VprMidiEffect> MidiEffects { get; set; }
            public List<VprNote> Notes { get; set; }
        }

        public class VprTrack
        {
            public int Type { get; set; }
            public string Name { get; set; }
            public int Color { get; set; }
            public int BusNo { get; set; }
            public bool IsFolded { get; set; }
            public double Height { get; set; }
            public VprTrackVolume Volume { get; set; }
            public VprPanpot Panpot { get; set; }
            public bool IsMuted { get; set; }
            public bool IsSoloMode { get; set; }
            public List<VprPart> Parts { get; set; }

            public VprTrack CloneBlank()
            {
                return new VprTrack
                {
                    Type = Type,
                    Name = Name,
                    Color = Color,
                    BusNo = BusNo,
                    IsFolded = IsFolded,
                    Height = Height,
                    Volume = Volume,
                    Panpot = Panpot,
                    IsMuted = IsMuted,
                    IsSoloMode = IsSoloMode,
                    Parts = new List<VprPart>
                    {
                        new VprPart()
                        {
                            Pos = Parts.First().Pos,
                            Duration = Parts.First().Duration,
                            StyleName = Parts.First().StyleName,
                            Voice = Parts.First().Voice,
                            MidiEffects = Parts.First().MidiEffects,
                            Notes = new List<VprNote>()
                        }
                    }
                };
            }
        }
    }
}
