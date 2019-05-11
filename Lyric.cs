using System.Linq;

namespace UtaFormatix
{
    public class Lyric
    {
        public Lyric(Data data, bool analyse)
        {
            Data = data;
            if (analyse)
            {
                LyricTypeAnalyse();
            }
        }

        private Data Data;
        public LyricType TypeAnalysed = LyricType.None;

        private void LyricTypeAnalyse()
        {
            var projectLyricType = LyricType.None;
            foreach (var track in Data.TrackList)
            {
                var noteList = track.NoteList;
                int[] typeCount = new int[4] { 0, 0, 0, 0 };
                foreach (var note in noteList)
                {
                    if (note.NoteLyric.Contains(" "))
                    {
                        try
                        {
                            string subLyric = note.NoteLyric.Substring(note.NoteLyric.IndexOf(" ") + 1);
                            for (int i = 1; i < 4; i++)
                            {
                                if (FindKana(subLyric.Substring(0, i)) != -1)
                                {
                                    typeCount[3]++;
                                    break;
                                }
                                else
                                {
                                    if (FindRomaji(subLyric.Substring(0, i)) != -1)
                                    {
                                        typeCount[1]++;
                                        break;
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        try
                        {
                            for (int i = 1; i < 4; i++)
                            {
                                if (FindKana(note.NoteLyric.Substring(0, i)) != -1)
                                {
                                    typeCount[2]++;
                                    break;
                                }
                                else
                                {
                                    if (FindRomaji(note.NoteLyric.Substring(0, i)) != -1)
                                    {
                                        typeCount[0]++;
                                        break;
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
                var trackLyricType = LyricType.None;
                for (int i = 0; i < 4; i++)
                {
                    if (100 * typeCount[i] / noteList.Count > 50)
                    {
                        if (trackLyricType == LyricType.None)
                        {
                            trackLyricType = (LyricType)(i + 1);
                        }
                        else
                        {
                            trackLyricType = LyricType.None;
                            break;
                        }
                    }
                }
                if (trackLyricType > 0)
                {
                    if (projectLyricType == LyricType.None)
                    {
                        projectLyricType = trackLyricType;
                    }
                    else
                    {
                        if (projectLyricType != trackLyricType)
                        {
                            return;
                        }
                    }
                }
            }
            TypeAnalysed = projectLyricType;
        }

        public void Transform(LyricType fromType, LyricType toType)
        {
            if (fromType != LyricType.None && toType != LyricType.None)
            {
                CleanLyric(fromType);
                switch (fromType - toType)
                {
                    case 0: break;
                    case 1:
                        {
                            if (fromType != LyricType.KanaTandoku)
                            {
                                TransLyricsTan2Ren(TransDirection.Reverse);
                            }
                            else
                            {
                                TransLyricsRomaji2Kana(TransDirection.Reverse);
                                TransLyricsTan2Ren(TransDirection.Sequential);
                            }
                            break;
                        }
                    case 2:
                        {
                            TransLyricsRomaji2Kana(TransDirection.Reverse);
                            break;
                        }
                    case 3:
                        {
                            TransLyricsRomaji2Kana(TransDirection.Reverse);
                            TransLyricsTan2Ren(TransDirection.Reverse);
                            break;
                        }
                    case -1:
                        {
                            if (fromType != LyricType.RomajiRenzoku)
                            {
                                TransLyricsTan2Ren(TransDirection.Sequential);
                            }
                            else
                            {
                                TransLyricsRomaji2Kana(TransDirection.Sequential);
                                TransLyricsTan2Ren(TransDirection.Reverse);
                            }
                            break;
                        }
                    case -2:
                        {
                            TransLyricsRomaji2Kana(TransDirection.Sequential);
                            break;
                        }
                    case -3:
                        {
                            TransLyricsRomaji2Kana(TransDirection.Sequential);
                            TransLyricsTan2Ren(TransDirection.Sequential);
                            break;
                        }
                }
            }
        }

        private void CleanLyric(LyricType type)
        {
            foreach (var track in Data.TrackList)
            {
                var noteList = track.NoteList;
                switch (type)
                {
                    case LyricType.RomajiTandoku:
                        {
                            for (int i = 0; i < noteList.Count; i++)
                            {
                                if (noteList[i].NoteLyric != "")
                                {
                                    noteList[i].NoteLyric = noteList[i].NoteLyric.ToLower();
                                    if (noteList[i].NoteLyric.Substring(0, 1) == "?")
                                    {
                                        noteList[i].NoteLyric = noteList[i].NoteLyric.Remove(0, 1);
                                    }
                                    try
                                    {
                                        for (int j = 3; j >= 1; j--)
                                        {
                                            if (FindRomaji(noteList[i].NoteLyric.Substring(0, j)) != -1)
                                            {
                                                noteList[i].NoteLyric = noteList[i].NoteLyric.Substring(0, j);
                                                break;
                                            }
                                        }
                                    }
                                    catch { }
                                }
                            }
                            break;
                        } // Prefix other than “?” is not considered for Romaji Tandoku
                    case LyricType.RomajiRenzoku:
                        {
                            for (int i = 0; i < noteList.Count; i++)
                            {
                                if (noteList[i].NoteLyric != "")
                                {
                                    noteList[i].NoteLyric = noteList[i].NoteLyric.ToLower();
                                    if (noteList[i].NoteLyric.Contains(" "))
                                    {
                                        int blankPos = noteList[i].NoteLyric.IndexOf(" ");
                                        string body = "";
                                        try
                                        {
                                            for (int j = 1; j <= 3; j++)
                                            {
                                                if (FindRomaji(noteList[i].NoteLyric.Substring(blankPos + 1, j)) != -1)
                                                {
                                                    body = noteList[i].NoteLyric.Substring(blankPos + 1, j);
                                                }
                                            }
                                        }
                                        catch { }
                                        if (body != "" && IsEnd(noteList[i].NoteLyric.Substring(blankPos - 1, 1)))
                                        {
                                            noteList[i].NoteLyric = noteList[i].NoteLyric.Substring(blankPos - 1, 1) + " " + body;
                                        }
                                    }
                                    else
                                    {
                                        if (noteList[i].NoteLyric.Substring(0, 1) == "?")
                                        {
                                            noteList[i].NoteLyric = noteList[i].NoteLyric.Remove(0, 1);
                                        }
                                        try
                                        {
                                            for (int j = 3; j >= 1; j--)
                                            {
                                                if (FindRomaji(noteList[i].NoteLyric.Substring(0, j)) != -1)
                                                {
                                                    noteList[i].NoteLyric = noteList[i].NoteLyric.Substring(0, j);
                                                    break;
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            }
                            break;
                        }
                    case LyricType.KanaTandoku:
                        {
                            for (int i = 0; i < noteList.Count; i++)
                            {
                                if (noteList[i].NoteLyric != "")
                                {
                                    string buffer = "";
                                    for (int j = 0; j < noteList[i].NoteLyric.Length; j++)
                                    {
                                        try
                                        {
                                            buffer = noteList[i].NoteLyric.Substring(j, 2);
                                            if (FindKana(buffer) == -1)
                                            {
                                                buffer = noteList[i].NoteLyric.Substring(j, 1);
                                            }
                                        }
                                        catch
                                        {
                                            buffer = noteList[i].NoteLyric.Substring(j, 1);
                                        }
                                        if (FindKana(buffer) != -1)
                                        {
                                            noteList[i].NoteLyric = buffer;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case LyricType.KanaRenzoku:
                        {
                            for (int i = 0; i < noteList.Count; i++)
                            {
                                if (noteList[i].NoteLyric != "")
                                {
                                    if (noteList[i].NoteLyric.Contains(" "))
                                    {
                                        int blankPos = noteList[i].NoteLyric.IndexOf(" ");
                                        string body;
                                        try
                                        {
                                            body = noteList[i].NoteLyric.Substring(blankPos + 1, 2);
                                            if (FindKana(body) == -1)
                                            {
                                                body = noteList[i].NoteLyric.Substring(blankPos + 1, 1);
                                            }
                                        }
                                        catch
                                        {
                                            body = noteList[i].NoteLyric.Substring(blankPos + 1, 1);
                                        }
                                        if (FindKana(body) != -1 && IsEnd(noteList[i].NoteLyric.Substring(blankPos - 1, 1)))
                                        {
                                            noteList[i].NoteLyric = noteList[i].NoteLyric.Substring(blankPos - 1, 1) + " " + body;
                                        }
                                    }
                                    else
                                    {
                                        string buffer = "";
                                        for (int j = 0; j < noteList[i].NoteLyric.Length; j++)
                                        {
                                            try
                                            {
                                                buffer = noteList[i].NoteLyric.Substring(j, 2);
                                                if (FindKana(buffer) == -1)
                                                {
                                                    buffer = noteList[i].NoteLyric.Substring(j, 1);
                                                }
                                            }
                                            catch
                                            {
                                                buffer = noteList[i].NoteLyric.Substring(j, 1);
                                            }
                                            if (FindKana(buffer) != -1)
                                            {
                                                noteList[i].NoteLyric = buffer;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                }
            }
        }

        private void TransLyricsRomaji2Kana(TransDirection direction)
        {
            foreach (var track in Data.TrackList)
            {
                var NoteList = track.NoteList;
                switch (direction)
                {
                    case TransDirection.None:  //Do nothing
                        break;

                    case TransDirection.Sequential:  //Romaji to Kana
                        for (int i = 0; i < NoteList.Count; i++)
                        {
                            if (NoteList[i].NoteLyric.Contains(" ")) //is RenZokuOn
                            {
                                string head = NoteList[i].NoteLyric.Substring(0, 1);
                                string body = NoteList[i].NoteLyric.Remove(0, 2);
                                NoteList[i].NoteLyric = head + " " + ConvertRomajiToKana(body);
                            }
                            else //is TanDokuOn
                            {
                                NoteList[i].NoteLyric = ConvertRomajiToKana(NoteList[i].NoteLyric);
                            }
                        }
                        break;

                    case TransDirection.Reverse:  //Kana to Romaji
                        for (int i = 0; i < NoteList.Count; i++)
                        {
                            if (NoteList[i].NoteLyric.Contains(" ")) //is RenZokuOn
                            {
                                string head = NoteList[i].NoteLyric.Substring(0, 1);
                                string body = NoteList[i].NoteLyric.Remove(0, 2);
                                NoteList[i].NoteLyric = head + " " + ConvertKanaToRomaji(body);
                            }
                            else //is TanDokuOn
                            {
                                NoteList[i].NoteLyric = ConvertKanaToRomaji(NoteList[i].NoteLyric);
                            }
                        }
                        break;
                }
            }
        }

        private void TransLyricsTan2Ren(TransDirection direction)
        {
            foreach (var track in Data.TrackList)
            {
                var noteList = track.NoteList;
                switch (direction)
                {
                    case TransDirection.None:  //Do nothing
                        break;

                    case TransDirection.Sequential:  //TanDoku to RenZoku
                        string tailOfLast = "-";
                        for (int i = 0; i < noteList.Count; i++)
                        {
                            string tail = tailOfLast;
                            if (i > 0 && noteList[i].NoteTimeOn > noteList[i - 1].NoteTimeOff)
                            {
                                tail = "-";
                            }
                            if (FindKana(noteList[i].NoteLyric) != -1)  //is Kana
                            {
                                tailOfLast = ConvertKanaToRomaji(noteList[i].NoteLyric).Substring(ConvertKanaToRomaji(noteList[i].NoteLyric).Length - 1, 1);
                                noteList[i].NoteLyric = tail + " " + noteList[i].NoteLyric;
                            }
                            else if (FindRomaji(noteList[i].NoteLyric) != -1) //is Romaji
                            {
                                tailOfLast = noteList[i].NoteLyric.Substring(noteList[i].NoteLyric.Length - 1, 1);
                                noteList[i].NoteLyric = tail + " " + noteList[i].NoteLyric;
                            }
                            else
                            {
                                tailOfLast = "-";
                            }
                        }
                        break;

                    case TransDirection.Reverse:  //RenZoku to TanDoku
                        for (int i = 0; i < noteList.Count; i++)
                        {
                            if (noteList[i].NoteLyric.Contains(" ") && (FindKana(noteList[i].NoteLyric.Remove(0, 2)) != -1 || FindRomaji(noteList[i].NoteLyric.Remove(0, 2)) != -1))
                            {
                                noteList[i].NoteLyric = noteList[i].NoteLyric.Remove(0, 2);
                            }
                        }
                        break;
                }
            }
        }

        private string ConvertKanaToRomaji(string kana)
        {
            for (int i = 0; i < Kanas.Length; i++)
            {
                if (kana == Kanas[i])
                {
                    return Romajis[i];
                }
            }
            return kana;
        }

        private string ConvertRomajiToKana(string romaji)
        {
            for (int i = 0; i < Kanas.Length; i++)
            {
                if (romaji == Romajis[i])
                {
                    return Kanas[i];
                }
            }
            return romaji;
        }

        private static int FindKana(string kana)
        {
            return Kanas.ToList().IndexOf(kana);
        }

        private static int FindRomaji(string romaji)
        {
            return Romajis.ToList().IndexOf(romaji);
        }

        private static bool IsEnd(string end)
        {
            if (end.Length != 1)
            {
                return false;
            }
            else
            {
                if (end == "a" || end == "i" || end == "u" || end == "e" || end == "o" || end == "n" || end == "-")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public enum LyricType
        {
            None,
            RomajiTandoku,
            RomajiRenzoku,
            KanaTandoku,
            KanaRenzoku,
        }

        private enum TransDirection
        {
            None,
            Sequential,
            Reverse,
        }

        private static readonly string[] Kanas = { "あ", "い", "いぇ", "う", "わ", "うぁ", "うぁ", "うぃ", "うぃ", "うぇ", "え", "お", "か", "が", "き", "きぇ", "きゃ", "きゅ", "きょ", "ぎ", "ぎぇ", "ぎゃ", "ぎゅ", "ぎょ", "く", "くぁ", "くぃ", "くぇ", "くぉ", "ぐ", "ぐぁ", "ぐぃ", "ぐぇ", "ぐぉ", "け", "げ", "こ", "ご", "さ", "ざ", "し", "し", "しぇ", "しぇ", "しゃ", "しゃ", "しゅ", "しゅ", "しょ", "しょ", "じ", "じぇ", "じぇ", "じゃ", "じゃ", "じゅ", "じゅ", "じょ", "じょ", "す", "すぁ", "すぃ", "すぇ", "すぉ", "ず", "ずぁ", "ずぃ", "ずぇ", "ずぉ", "せ", "ぜ", "そ", "ぞ", "た", "だ", "ち", "ちぇ", "ちゃ", "ちゅ", "ちょ", "つ", "つ", "つぁ", "つぁ", "つぃ", "つぃ", "つぇ", "つぇ", "つぉ", "つぉ", "て", "てぃ", "てゅ", "で", "でぃ", "でゅ", "と", "とぅ", "とぅ", "ど", "どぅ", "どぅ", "な", "に", "にぇ", "にゃ", "にゅ", "にょ", "ぬ", "ぬぁ", "ぬぃ", "ぬぇ", "ぬぉ", "ね", "の", "は", "ば", "ぱ", "ひ", "ひぇ", "ひゃ", "ひゅ", "ひょ", "び", "びぇ", "びゃ", "びゅ", "びょ", "ぴ", "ぴぇ", "ぴゃ", "ぴゅ", "ぴょ", "ふ", "ふぁ", "ふぃ", "ふぇ", "ふぉ", "ぶ", "ぶぁ", "ぶぃ", "ぶぇ", "ぶぉ", "ぷ", "ぷぁ", "ぷぃ", "ぷぇ", "ぷぉ", "へ", "べ", "ぺ", "ほ", "ぼ", "ぽ", "ま", "み", "みぇ", "みゃ", "みゅ", "みょ", "む", "むぁ", "むぃ", "むぇ", "むぉ", "め", "も", "や", "ゆ", "よ", "ら", "り", "りぇ", "りゃ", "りゅ", "りょ", "る", "るぁ", "るぃ", "るぇ", "るぉ", "れ", "ろ", "わ", "を", "うぉ", "ん", "ー" };
        private static readonly string[] Romajis = { "a", "i", "ye", "u", "wa", "wa", "ua", "wi", "ui", "we", "e", "o", "ka", "ga", "ki", "kye", "kya", "kyu", "kyo", "gi", "gye", "gya", "gyu", "gyo", "ku", "kua", "kui", "kue", "kuo", "gu", "gua", "gui", "gue", "guo", "ke", "ge", "ko", "go", "sa", "za", "shi", "si", "she", "sye", "sha", "sya", "shu", "syu", "sho", "syo", "ji", "je", "jye", "ja", "jya", "ju", "jyu", "jo", "jyo", "su", "sua", "sui", "sue", "suo", "zu", "zua", "zui", "zue", "zuo", "se", "ze", "so", "zo", "ta", "da", "chi", "che", "cha", "chu", "cho", "tsu", "tu", "tsa", "tua", "tsi", "tui", "tse", "tue", "tso", "tuo", "te", "ti", "tyu", "de", "di", "dyu", "to", "tu", "twu", "do", "du", "dwu", "na", "ni", "nye", "nya", "nyu", "nyo", "nu", "nua", "nui", "nue", "nuo", "ne", "no", "ha", "ba", "pa", "hi", "hye", "hya", "hyu", "hyo", "bi", "bye", "bya", "byu", "byo", "pi", "pye", "pya", "pyu", "pyo", "fu", "fa", "fi", "fe", "fo", "bu", "bua", "bui", "bue", "buo", "pu", "pua", "pui", "pue", "puo", "he", "be", "pe", "ho", "bo", "po", "ma", "mi", "mye", "mya", "myu", "myo", "mu", "mua", "mui", "mue", "muo", "me", "mo", "ya", "yu", "yo", "ra", "ri", "rye", "rya", "ryu", "ryo", "ru", "rua", "rui", "rue", "ruo", "re", "ro", "wa", "o", "wo", "n", "-" };
    }
}
