using System;
using System.Collections.Generic;
using System.Text;

namespace PT.Shared
{
    public static class Unicode2TCVN3
    {
        static char[] TCVN3 = new char[213]
        {
            'A','a','¸','¸','µ','µ','¶','¶','·','·','¹','¹',
            '¢','©','Ê','Ê','Ç','Ç','È','È','É','É','Ë','Ë',
            '¡','¨','¾','¾','»','»','¼','¼','½','½','Æ','Æ',
            'B','b','C','c','D','d',
            '§','®',
            'E','e','Ð','Ð','Ì','Ì','Î','Î','Ï','Ï','Ñ','Ñ',
            '£','ª','Õ','Õ','Ò','Ò','Ó','Ó','Ô','Ô','Ö','Ö',
            'F','f','G','g','H','h',
            'I','i','Ý','Ý','×','×','Ø','Ø','Ü','Ü','Þ','Þ',
            'J','j','K','k','L','l','M','m','N','n',
            'O','o','ã','ã','ß','ß','á','á','â','â','ä','ä',
            '¤','«','è','è','å','å','æ','æ','ç','ç','é','é',
            '¥','¬','í','í','ê','ê','ë','ë','ì','ì','î','î',
            'P','p','Q','q','R','r','S','s','T','t',
            'U','u','ó','ó','ï','ï','ñ','ñ','ò','ò','ô','ô',
            '¦','­','ø','ø','õ','õ','ö','ö','÷','÷','ù','ù',
            'V','v','W','w','X','x',
            'Y','y','ý','ý','ú','ú','û','û','ü','ü','þ','þ',
            'Z','z',
            (char)0x80, (char)0x82, (char)0x83, (char)0x84, (char)0x85, (char)0x86, (char)0x87, (char)0x88,
            (char)0x89, (char)0x8A, (char)0x8B, (char)0x8C, (char)0x8E, (char)0x91, (char)0x92, (char)0x93,
            (char)0x94, (char)0x95, (char)0x96, (char)0x97, (char)0x98, (char)0x99, (char)0x9A, (char)0x9B,
            (char)0x9C, (char)0x9E, (char)0x9F
        };


        static char[] Unicode = new char[213]
        {
            'A','a','á','á','à','à','ả','ả','ã','ã','ạ','ạ',
            'Â','â','ấ','ấ','ầ','ầ','ẩ','ẩ','ẫ','ẫ','ậ','ậ',
            'Ă','ă','ắ','ắ','ằ','ằ','ẳ','ẳ','ẵ','ẵ','ặ','ặ',
            'B','b','C','c','D','d',
            'Đ','đ',
            'E','e','é','é','è','è','ẻ','ẻ','ẽ','ẽ','ẹ','ẹ',
            'Ê','ê','ế','ế','ề','ề','ể','ể','ễ','ễ','ệ','ệ',
            'F','f','G','g','H','h',
            'I','i','í','í','ì','ì','ỉ','ỉ','ĩ','ĩ','ị','ị',
            'J','j','K','k','L','l','M','m','N','n',
            'O','o','ó','ó','ò','ò','ỏ','ỏ','õ','õ','ọ','ọ',
            'Ô','ô','ố','ố','ồ','ồ','ổ','ổ','ỗ','ỗ','ộ','ộ',
            'Ơ','ơ','ớ','ớ','ờ','ờ','ở','ở','ỡ','ỡ','ợ','ợ',
            'P','p','Q','q','R','r','S','s','T','t',
            'U','u','ú','ú','ù','ù','ủ','ủ','ũ','ũ','ụ','ụ',
            'Ư','ư','ứ','ứ','ừ','ừ','ử','ử','ữ','ữ','ự','ự',
            'V','v','W','w','X','x',
            'Y','y','ý','ý','ỳ','ỳ','ỷ','ỷ','ỹ','ỹ','ỵ','ỵ',
            'Z','z',
            (char)0x20AC, (char)0x20A1, (char)0x0192, (char)0x201E, (char)0x2026, (char)0x2020, (char)0x2021, (char)0x02C6,
            (char)0x2030, (char)0x0160, (char)0x2039, (char)0x0152, (char)0x017D, (char)0x2018, (char)0x2019, (char)0x201C,
            (char)0x201D, (char)0x2022, (char)0x2013, (char)0x2014, (char)0x02DC, (char)0x2122, (char)0x0161, (char)0x203A,
            (char)0x0153, (char)0x017E, (char)0x0178
        };

        static char ToUnicode(char ch)
        {
            for (int i = 0; i < 213; i++)
                if (ch == TCVN3[i]) return Unicode[i];
            return ch;
        }

        static char ToTCVN3(char ch)
        {
            for (int i = 0; i < 213; i++)
                if (ch == Unicode[i]) return TCVN3[i];
            return ch;
        }

        public static char[] ToTCVN3(string value)
        {
            char[] chars = value.ToCharArray();
            for (int i = 0, max = chars.Length; i < max; i++)
            {
                chars[i] = ToTCVN3(chars[i]);
            }
            return chars;
        }

        public static string ToUnicode(string value)
        {
            char[] chars = value.ToCharArray();
            for (int i = 0, max = chars.Length; i < max; i++)
            {
                chars[i] = ToUnicode(chars[i]);
            }
            return new string(chars);
        }

        public static string ToUnicode(byte[] bytes)
        {
            char[] chars = new char[bytes.Length];
            for (int i = 0, max = bytes.Length; i < max; i++)
            {
                char c = (char)bytes[i];
                chars[i] = ToUnicode(c);
            }
            return new string(chars);
        }
    }
}
