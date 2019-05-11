namespace UtaFormatix
{
    public partial class Data
    {
        public class Note
        {
            public Note()
            {
            }

            public Note(Note note)
            {
                NoteNum = note.NoteNum;
                NoteKey = note.NoteKey;
                NoteLyric = note.NoteLyric;
                NoteTimeOn = note.NoteTimeOn;
                NoteTimeOff = note.NoteTimeOff;
                NoteValidity = note.NoteValidity;
                NoteIdforUTAU = note.NoteIdforUTAU;
            }

            public int NoteNum;
            public int NoteKey;
            public string NoteLyric;
            public int NoteTimeOn;
            public int NoteTimeOff;
            public bool NoteValidity = true;
            public string NoteIdforUTAU;
            public int NoteLength => NoteTimeOff - NoteTimeOn;
        }
    }
}