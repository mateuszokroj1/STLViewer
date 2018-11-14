using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;

namespace StlLibrary
{
    public class StlAscii : StlFile
    {
        public StlAscii(FileStream file)
        {
            if (file == null) throw new ArgumentNullException();
            if (!file.CanRead || file.Length < 4) throw new ArgumentException("Nie można odczytać lub niepoprawny plik");
            string data;
            byte[] ascii = new byte[file.Length];
            file.Position = 0;
            for (ulong i = 0; (long)i < file.Length; i++)
            {
                int c = file.ReadByte();
                if (c < 0 || c > 255) break;
                ascii[i] = Convert.ToByte(c);
            }
            data = Encoding.ASCII.GetString(ascii);

            data = data.Trim().Replace("\n", "").Replace("\r", "").ToUpperInvariant();

            Regex re = new Regex(@"^SOLID\s+([A-Z0-9_-]+)\s+(.+)ENDSOLID\s+([A-Z0-9_-]+)$");
            Match match;
            try { match = re.Match(data); } catch (ArgumentException) { throw new ApplicationException("Niepoprawy plik"); }

            string name1 = match.Groups[0].Captures[0].Value, name2 = match.Groups[2].Captures[0].Value;
            if (!name1.Equals(name2)) throw new ApplicationException("Niepoprawny plik");

            data = match.Groups[1].Captures[0].Value.Trim();
            re = new Regex(@"^$");
        }
    }
}
