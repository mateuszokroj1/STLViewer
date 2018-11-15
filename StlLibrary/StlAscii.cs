using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

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

            Regex re = new Regex(@"^SOLID\s+(?<name1>[A-Z0-9_-]+)\s+(?<content>.+)ENDSOLID\s+(?<name2>[A-Z0-9_-]+)$");
            Match match;
            try { match = re.Match(data); } catch (ArgumentException) { throw new ApplicationException("Niepoprawy plik"); }

            string name1 = match.Groups["name1"].Captures[0].Value, name2 = match.Groups["name2"].Captures[0].Value;
            if (!name1.Equals(name2)) throw new ApplicationException("Niepoprawny plik");

            data = match.Groups["content"].Captures[0].Value.Trim();

            re = new Regex(@"^(\s*FACET\s+NORMAL\s+(\d+\.\d+(E\d+)?)\s+(\d+\.\d+(E\d+)?)\s+(\d+\.\d+(E\d+)?)\s+OUTER\s+LOOP(\s*VERTEX\s+(\d+\.\d+(E\d+)?)\s+(\d+\.\d+(E\d+)?)\s+(\d+\.\d+(E\d+)?)\s*){3}\s+ENDLOOP\s+ENDFACET\s*)+$");
            if (!re.IsMatch(data)) System.Diagnostics.Debugger.Break();

            MatchCollection facets = new Regex(@"(\s*FACET\s+NORMAL\s+(?<i>\d+\.\d+(E\d+)?)\s+(?<j>\d+\.\d+(E\d+)?)\s+(?<k>\d+\.\d+(E\d+)?)\s+OUTER\s+LOOP(\s*VERTEX\s+(\d+\.\d+(E\d+)?)\s+(\d+\.\d+(E\d+)?)\s+(\d+\.\d+(E\d+)?)\s*){3}\s+ENDLOOP\s+ENDFACET\s*)").Matches(data);
            if (facets.Count < 1) Debugger.Break();
            Triangle[] triangles = new Triangle[facets.Count];
            uint i = 0;
            foreach (Match facet in facets)
            {
                Triangle newtri = new Triangle();
                newtri.Normal = new Point3D
                {
                    X = double.Parse(facet.Groups["i"].Captures[0].Value),
                    Y = double.Parse(facet.Groups["j"].Captures[0].Value),
                    Z = double.Parse(facet.Groups["k"].Captures[0].Value)
                };

                MatchCollection vertexes = new Regex(@"(\s*VERTEX\s+(?<x>\d+\.\d+(E\d+)?)\s+(?<y>\d+\.\d+(E\d+)?)\s+(?<z>\d+\.\d+(E\d+)?)\s*)").Matches(facet.Value);

                newtri.Vertex1 = new Point3D
                {
                    X = double.Parse(vertexes[0].Groups["x"].Value),
                    Y = double.Parse(vertexes[0].Groups["y"].Value),
                    Z = double.Parse(vertexes[0].Groups["z"].Value)
                };
                newtri.Vertex2 = new Point3D
                {
                    X = double.Parse(vertexes[1].Groups["x"].Value),
                    Y = double.Parse(vertexes[1].Groups["y"].Value),
                    Z = double.Parse(vertexes[1].Groups["z"].Value)
                };
                newtri.Vertex3 = new Point3D
                {
                    X = double.Parse(vertexes[2].Groups["x"].Value),
                    Y = double.Parse(vertexes[2].Groups["y"].Value),
                    Z = double.Parse(vertexes[2].Groups["z"].Value)
                };

                triangles[i] = newtri;
                i++;
            }
            this.Triangles.Clear();
            this.Triangles.AddRange(triangles);
        }
    }
}
