using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class Resources
    {
        public static String GetBase64Encoded(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                byte[] result = new byte[stream.Length];
                stream.Read(result, 0, (int)stream.Length);
                return Convert.ToBase64String(result);
            }
        }

        public static String GetAsString(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                byte[] result = new byte[stream.Length];
                stream.Read(result, 0, (int)stream.Length);
                return System.Text.Encoding.Default.GetString(result);
            }
        }
    }
}
