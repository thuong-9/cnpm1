using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using SlugGenerator;

namespace myschool.Utilities
{
    public class Functions
    {
        public static int _UserID = 0;
        public static string _UserName = string.Empty;
        public static string _Email = string.Empty;
        public static string _Message = string.Empty;

        public static string TitleSlugGeneration(string type, string? title, long id)
        {
            return type + "-" + SlugGenerator.SlugGenerator.GenerateSlug(title) + "-" + id.ToString() + ".html";
        }
        public static string getCurrentDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public static string MD5Hash(string text)
        {
            using MD5 md5 = MD5.Create();
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));
            byte[] result = md5.Hash ?? Array.Empty<byte>();
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
                strBuilder.Append(result[i].ToString("x2"));
            return strBuilder.ToString();
        }
        public static string MD5Password(string? text)
        {
            string safeText = text ?? string.Empty;
            string str = MD5Hash(safeText);
            for (int i = 0; i < 5; i++)
                str = MD5Hash(str + str);
            return str;
        }
        public static bool IsLogin()
        {
            if ((Functions._UserID <= 0) || string.IsNullOrEmpty(Functions._UserName) || string.IsNullOrEmpty(Functions._Email))
                return false;
            return true;        
        }
    }
}