using System.Collections.Generic;
using static UtaFormatix.Data;

namespace UtaFormatix
{
    public class Lyric
    {
        public Lyric(Data data, bool DoAnalyze)
        {
            this.data = data;
            if (DoAnalyze)
            {
                LyricTypeAnalyze();
            }
        }
        Data data;
        public LyricType AnalyzedType = LyricType.None;
        void LyricTypeAnalyze()
        {
            LyricType project_lyrictype = LyricType.None;
            foreach (track track in data.TrackList)
            {
                List<note> NoteList = track.NoteList;
                int[] typecount = new int[4] { 0, 0, 0, 0 };
                foreach (note note in NoteList)
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
                                    typecount[3]++;
                                    break;
                                }
                                else
                                {
                                    if (FindRomaji(subLyric.Substring(0, i)) != -1)
                                    {
                                        typecount[1]++;
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
                                    typecount[2]++;
                                    break;
                                }
                                else
                                {
                                    if (FindRomaji(note.NoteLyric.Substring(0, i)) != -1)
                                    {
                                        typecount[0]++;
                                        break;
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
                LyricType track_lyrictype = LyricType.None;
                for (int i = 0; i < 4; i++)
                {
                    if (100 * typecount[i] / NoteList.Count > 50)
                    {
                        if (track_lyrictype == LyricType.None)
                        {
                            track_lyrictype = (LyricType)(i + 1);
                        }
                        else
                        {
                            track_lyrictype = LyricType.None;
                            break;
                        }
                    }
                }
                if (track_lyrictype > 0)
                {
                    if (project_lyrictype == LyricType.None)
                    {
                        project_lyrictype = track_lyrictype;
                    }
                    else
                    {
                        if (project_lyrictype != track_lyrictype)
                        {
                            return;
                        }
                    }
                }
            }
            AnalyzedType = project_lyrictype;
        }
        public void Transform(LyricType fromtype, LyricType totype)
        {
            if (fromtype != LyricType.None && totype != LyricType.None)
            {
                CleanLyric(fromtype);
                switch (fromtype - totype)
                {
                    case 0: break;
                    case 1:
                        {
                            if (fromtype != LyricType.Kana_Tandoku)
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
                            if (fromtype != LyricType.Romaji_Renzoku)
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


        void CleanLyric(LyricType type)
        {
            foreach (track track in data.TrackList)
            {
                List<note> NoteList = track.NoteList;
                switch (type)
                {
                    case LyricType.Romaji_Tandoku:
                        {
                            for (int i = 0; i < NoteList.Count; i++)
                            {
                                if (NoteList[i].NoteLyric != "")
                                {
                                    NoteList[i].NoteLyric = NoteList[i].NoteLyric.ToLower();
                                    if (NoteList[i].NoteLyric.Substring(0, 1) == "?")
                                    {
                                        NoteList[i].NoteLyric = NoteList[i].NoteLyric.Remove(0, 1);
                                    }
                                    try
                                    {
                                        for (int j = 3; j >= 1; j--)
                                        {
                                            if (FindRomaji(NoteList[i].NoteLyric.Substring(0, j)) != -1)
                                            {
                                                NoteList[i].NoteLyric = NoteList[i].NoteLyric.Substring(0, j);
                                                break;
                                            }
                                        }
                                    }
                                    catch { }
                                }
                            }
                            break;
                        }//未考虑罗马音单独音除“?”以外的前缀
                    case LyricType.Romaji_Renzoku:
                        {
                            for (int i = 0; i < NoteList.Count; i++)
                            {
                                if (NoteList[i].NoteLyric != "")
                                {
                                    NoteList[i].NoteLyric = NoteList[i].NoteLyric.ToLower();
                                    if (NoteList[i].NoteLyric.Contains(" "))
                                    {
                                        int blank_pos = NoteList[i].NoteLyric.IndexOf(" ");
                                        string body = "";
                                        try
                                        {
                                            for (int j = 1; j <= 3; j++)
                                            {
                                                if (FindRomaji(NoteList[i].NoteLyric.Substring(blank_pos + 1, j)) != -1)
                                                {
                                                    body = NoteList[i].NoteLyric.Substring(blank_pos + 1, j);
                                                }
                                            }
                                        }
                                        catch { }
                                        if (body != "" && IsGobi(NoteList[i].NoteLyric.Substring(blank_pos - 1, 1)))
                                        {
                                            NoteList[i].NoteLyric = NoteList[i].NoteLyric.Substring(blank_pos - 1, 1) + " " + body;
                                        }
                                    }
                                    else
                                    {
                                        if (NoteList[i].NoteLyric.Substring(0, 1) == "?")
                                        {
                                            NoteList[i].NoteLyric = NoteList[i].NoteLyric.Remove(0, 1);
                                        }
                                        try
                                        {
                                            for (int j = 3; j >= 1; j--)
                                            {
                                                if (FindRomaji(NoteList[i].NoteLyric.Substring(0, j)) != -1)
                                                {
                                                    NoteList[i].NoteLyric = NoteList[i].NoteLyric.Substring(0, j);
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
                    case LyricType.Kana_Tandoku:
                        {
                            for (int i = 0; i < NoteList.Count; i++)
                            {
                                if (NoteList[i].NoteLyric != "")
                                {
                                    string buf = "";
                                    for (int j = 0; j < NoteList[i].NoteLyric.Length; j++)
                                    {
                                        try
                                        {
                                            buf = NoteList[i].NoteLyric.Substring(j, 2);
                                            if (FindKana(buf) == -1)
                                            {
                                                buf = NoteList[i].NoteLyric.Substring(j, 1);
                                            }
                                        }
                                        catch
                                        {
                                            buf = NoteList[i].NoteLyric.Substring(j, 1);
                                        }
                                        if (FindKana(buf) != -1)
                                        {
                                            NoteList[i].NoteLyric = buf;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case LyricType.Kana_Renzoku:
                        {
                            for (int i = 0; i < NoteList.Count; i++)
                            {
                                if (NoteList[i].NoteLyric != "")
                                {
                                    if (NoteList[i].NoteLyric.Contains(" "))
                                    {
                                        int blank_pos = NoteList[i].NoteLyric.IndexOf(" ");
                                        string body;
                                        try
                                        {
                                            body = NoteList[i].NoteLyric.Substring(blank_pos + 1, 2);
                                            if (FindKana(body) == -1)
                                            {
                                                body = NoteList[i].NoteLyric.Substring(blank_pos + 1, 1);
                                            }
                                        }
                                        catch
                                        {
                                            body = NoteList[i].NoteLyric.Substring(blank_pos + 1, 1);
                                        }
                                        if (FindKana(body) != -1 && IsGobi(NoteList[i].NoteLyric.Substring(blank_pos - 1, 1)))
                                        {
                                            NoteList[i].NoteLyric = NoteList[i].NoteLyric.Substring(blank_pos - 1, 1) + " " + body;
                                        }
                                    }
                                    else
                                    {
                                        string buf = "";
                                        for (int j = 0; j < NoteList[i].NoteLyric.Length; j++)
                                        {
                                            try
                                            {
                                                buf = NoteList[i].NoteLyric.Substring(j, 2);
                                                if (FindKana(buf) == -1)
                                                {
                                                    buf = NoteList[i].NoteLyric.Substring(j, 1);
                                                }
                                            }
                                            catch
                                            {
                                                buf = NoteList[i].NoteLyric.Substring(j, 1);
                                            }
                                            if (FindKana(buf) != -1)
                                            {
                                                NoteList[i].NoteLyric = buf;
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
        void TransLyricsRomaji2Kana(TransDirection mode)
        {
            foreach (track track in data.TrackList)
            {
                List<note> NoteList = track.NoteList;
                switch (mode)
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
        void TransLyricsTan2Ren(TransDirection mode)
        {
            foreach (track track in data.TrackList)
            {
                List<note> NoteList = track.NoteList;
                switch (mode)
                {
                    case TransDirection.None:  //Do nothing 
                        break;
                    case TransDirection.Sequential:  //TanDoku to RenZoku
                        string tail_of_last = "-";
                        for (int i = 0; i < NoteList.Count; i++)
                        {
                            string tail = tail_of_last;
                            if (i > 0 && NoteList[i].NoteTimeOn > NoteList[i - 1].NoteTimeOff)
                            {
                                tail = "-";
                            }
                            if (FindKana(NoteList[i].NoteLyric) != -1)  //is Kana
                            {
                                tail_of_last = ConvertKanaToRomaji(NoteList[i].NoteLyric).Substring(ConvertKanaToRomaji(NoteList[i].NoteLyric).Length - 1, 1);
                                NoteList[i].NoteLyric = tail + " " + NoteList[i].NoteLyric;
                            }
                            else if (FindRomaji(NoteList[i].NoteLyric) != -1) //is Romaji
                            {
                                tail_of_last = NoteList[i].NoteLyric.Substring(NoteList[i].NoteLyric.Length - 1, 1);
                                NoteList[i].NoteLyric = tail + " " + NoteList[i].NoteLyric;
                            }
                            else
                            {
                                tail_of_last = "-";
                            }
                        }
                        break;
                    case TransDirection.Reverse:  //RenZoku to TanDoku
                        for (int i = 0; i < NoteList.Count; i++)
                        {
                            if (NoteList[i].NoteLyric.Contains(" ") && (FindKana(NoteList[i].NoteLyric.Remove(0, 2)) != -1 || FindRomaji(NoteList[i].NoteLyric.Remove(0, 2)) != -1))
                            {
                                NoteList[i].NoteLyric = NoteList[i].NoteLyric.Remove(0, 2);
                            }
                        }
                        break;
                }
            }
        }
        string ConvertKanaToRomaji(string _kana)
        {
            for (int i = 0; i < Kanas.Length; i++)
            {
                if (_kana == Kanas[i])
                {
                    return Romajis[i];
                }
            }
            return _kana;
        }
        string ConvertRomajiToKana(string _romaji)
        {
            for (int i = 0; i < Kanas.Length; i++)
            {
                if (_romaji == Romajis[i])
                {
                    return Kanas[i];
                }
            }
            return _romaji;
        }
        int FindKana(string _kana)
        {
            for (int i = 0; i < Kanas.Length; i++)
            {
                if (_kana == Kanas[i])
                {
                    return i;
                }
            }
            return -1;
        }
        int FindRomaji(string _romaji)
        {
            for (int i = 0; i < Kanas.Length; i++)
            {
                if (_romaji == Romajis[i])
                {
                    return i;
                }
            }
            return -1;
        }
        bool IsGobi(string _gobi)
        {
            if (_gobi.Length != 1)
            {
                return false;
            }
            else
            {
                if (_gobi == "a" || _gobi == "i" || _gobi == "u" || _gobi == "e" || _gobi == "o" || _gobi == "n" || _gobi == "-")
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
            Romaji_Tandoku,
            Romaji_Renzoku,
            Kana_Tandoku,
            Kana_Renzoku,
        }
        enum TransDirection
        {
            None,
            Sequential,
            Reverse,
        }
        string[] Kanas = { "あ", "い", "いぇ", "う", "わ", "うぁ", "うぁ", "うぃ", "うぃ", "うぇ", "え", "お", "か", "が", "き", "きぇ", "きゃ", "きゅ", "きょ", "ぎ", "ぎぇ", "ぎゃ", "ぎゅ", "ぎょ", "く", "くぁ", "くぃ", "くぇ", "くぉ", "ぐ", "ぐぁ", "ぐぃ", "ぐぇ", "ぐぉ", "け", "げ", "こ", "ご", "さ", "ざ", "し", "し", "しぇ", "しぇ", "しゃ", "しゃ", "しゅ", "しゅ", "しょ", "しょ", "じ", "じぇ", "じぇ", "じゃ", "じゃ", "じゅ", "じゅ", "じょ", "じょ", "す", "すぁ", "すぃ", "すぇ", "すぉ", "ず", "ずぁ", "ずぃ", "ずぇ", "ずぉ", "せ", "ぜ", "そ", "ぞ", "た", "だ", "ち", "ちぇ", "ちゃ", "ちゅ", "ちょ", "つ", "つ", "つぁ", "つぁ", "つぃ", "つぃ", "つぇ", "つぇ", "つぉ", "つぉ", "て", "てぃ", "てゅ", "で", "でぃ", "でゅ", "と", "とぅ", "とぅ", "ど", "どぅ", "どぅ", "な", "に", "にぇ", "にゃ", "にゅ", "にょ", "ぬ", "ぬぁ", "ぬぃ", "ぬぇ", "ぬぉ", "ね", "の", "は", "ば", "ぱ", "ひ", "ひぇ", "ひゃ", "ひゅ", "ひょ", "び", "びぇ", "びゃ", "びゅ", "びょ", "ぴ", "ぴぇ", "ぴゃ", "ぴゅ", "ぴょ", "ふ", "ふぁ", "ふぃ", "ふぇ", "ふぉ", "ぶ", "ぶぁ", "ぶぃ", "ぶぇ", "ぶぉ", "ぷ", "ぷぁ", "ぷぃ", "ぷぇ", "ぷぉ", "へ", "べ", "ぺ", "ほ", "ぼ", "ぽ", "ま", "み", "みぇ", "みゃ", "みゅ", "みょ", "む", "むぁ", "むぃ", "むぇ", "むぉ", "め", "も", "や", "ゆ", "よ", "ら", "り", "りぇ", "りゃ", "りゅ", "りょ", "る", "るぁ", "るぃ", "るぇ", "るぉ", "れ", "ろ", "わ", "を", "うぉ", "ん", "ー" };
        string[] Romajis = { "a", "i", "ye", "u", "wa", "wa", "ua", "wi", "ui", "we", "e", "o", "ka", "ga", "ki", "kye", "kya", "kyu", "kyo", "gi", "gye", "gya", "gyu", "gyo", "ku", "kua", "kui", "kue", "kuo", "gu", "gua", "gui", "gue", "guo", "ke", "ge", "ko", "go", "sa", "za", "shi", "si", "she", "sye", "sha", "sya", "shu", "syu", "sho", "syo", "ji", "je", "jye", "ja", "jya", "ju", "jyu", "jo", "jyo", "su", "sua", "sui", "sue", "suo", "zu", "zua", "zui", "zue", "zuo", "se", "ze", "so", "zo", "ta", "da", "chi", "che", "cha", "chu", "cho", "tsu", "tu", "tsa", "tua", "tsi", "tui", "tse", "tue", "tso", "tuo", "te", "ti", "tyu", "de", "di", "dyu", "to", "tu", "twu", "do", "du", "dwu", "na", "ni", "nye", "nya", "nyu", "nyo", "nu", "nua", "nui", "nue", "nuo", "ne", "no", "ha", "ba", "pa", "hi", "hye", "hya", "hyu", "hyo", "bi", "bye", "bya", "byu", "byo", "pi", "pye", "pya", "pyu", "pyo", "fu", "fa", "fi", "fe", "fo", "bu", "bua", "bui", "bue", "buo", "pu", "pua", "pui", "pue", "puo", "he", "be", "pe", "ho", "bo", "po", "ma", "mi", "mye", "mya", "myu", "myo", "mu", "mua", "mui", "mue", "muo", "me", "mo", "ya", "yu", "yo", "ra", "ri", "rye", "rya", "ryu", "ryo", "ru", "rua", "rui", "rue", "ruo", "re", "ro", "wa", "o", "wo", "n", "-" };
    }
}
