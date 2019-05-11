using System.Collections.Generic;
using System.Linq;

namespace UtaFormatix
{
    public partial class Data
    {
        public class Track
        {
            public Track()
            {
            }

            public Track(Track track)
            {
                TrackNum = track.TrackNum;
                TrackName = track.TrackName;
                NoteList = track.NoteList.Select(it => new Note(it)).ToList();
            }

            public int TrackNum;
            public string TrackName;
            public List<Note> NoteList;
        }
    }
}