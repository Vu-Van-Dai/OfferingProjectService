using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Utils
{
    public static class StringUtils
    {
        public static string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.ToLower();
            // Bỏ dấu
            text = Regex.Replace(text, "[àáạảãâầấậẩẫăằắặẳẵ]", "a");
            text = Regex.Replace(text, "[èéẹẻẽêềếệểễ]", "e");
            text = Regex.Replace(text, "[ìíịỉĩ]", "i");
            text = Regex.Replace(text, "[òóọỏõôồốộổỗơờớợởỡ]", "o");
            text = Regex.Replace(text, "[ùúụủũưừứựửữ]", "u");
            text = Regex.Replace(text, "[ỳýỵỷỹ]", "y");
            text = Regex.Replace(text, "[đ]", "d");
            // Bỏ các ký tự đặc biệt
            text = Regex.Replace(text, "[^a-z0-9\\s-]", "");
            // Thay thế khoảng trắng bằng dấu gạch ngang (tùy chọn)
            text = Regex.Replace(text, "\\s+", " ").Trim();

            return text;
        }
    }
}
