using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Globalization;

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
            for (ulong j = 0; (long)j < file.Length; j++)
            {
                int c = file.ReadByte();
                if (c < 0 || c > 255) break;
                ascii[j] = Convert.ToByte(c);
            }
            data = Encoding.ASCII.GetString(ascii);

            data = data
                .Trim()
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Replace("\t", " ")
                .ToUpperInvariant();

            Regex re = new Regex(@"^SOLID\s+(?<name1>[A-Z0-9_-]+)\s*(\s*FACET\s+NORMAL\s+(\d+\.\d+)\s+(\d+\.\d+)\s+(\d+\.\d+)\s+OUTER\s+LOOP\s*(\s*VERTEX\s+(\d+\.\d+)\s+(\d+\.\d+)\s+(\d+\.\d+)\s*){3}\s+ENDLOOP\s+ENDFACET\s*)+\s*ENDSOLID\s+(?<name2>[A-Z0-9_-]+)$");
            Match match = re.Match(data);
            if (match == null || !match.Success) throw new StlFileException("Niepoprawny format pliku ASCII");

            string name1 = match.Groups["name1"].Captures[0].Value, name2 = match.Groups["name2"].Captures[0].Value;
            if (!name1.Equals(name2)) throw new StlFileException($"SOLID \"{name1}\" nie został poprawnie zakończony");

            MatchCollection facets = new Regex(@"\s*FACET\s+NORMAL\s+(?<i>\d+\.\d+)\s+(?<j>\d+\.\d+)\s+(?<k>\d+\.\d+)\s+OUTER\s+LOOP\s*(\s*VERTEX\s+(\d+\.\d+)\s+(\d+\.\d+)\s+(\d+\.\d+)\s*){3}\s+ENDLOOP\s+ENDFACET\s*").Matches(data);
            if (facets.Count < 1) throw new StlFileException("Oczekiwano minimum jednego trójkąta");
            Triangle[] triangles = new Triangle[facets.Count];
            uint i = 0;
            foreach (Match facet in facets)
            {
                Triangle newtri = new Triangle();
                newtri.Normal = new Point3D
                {
                    X = double.Parse(facet.Groups["i"].Value, CultureInfo.InvariantCulture),
                    Y = double.Parse(facet.Groups["j"].Value, CultureInfo.InvariantCulture),
                    Z = double.Parse(facet.Groups["k"].Value, CultureInfo.InvariantCulture)
                };

                MatchCollection vertexes = new Regex(@"\s*VERTEX\s+(?<x>\d+\.\d+)\s+(?<y>\d+\.\d+)\s+(?<z>\d+\.\d+)\s*").Matches(facet.Value);

                newtri.Vertex1 = new Point3D
                {
                    X = double.Parse(vertexes[0].Groups["x"].Value, CultureInfo.InvariantCulture),
                    Y = double.Parse(vertexes[0].Groups["y"].Value, CultureInfo.InvariantCulture),
                    Z = double.Parse(vertexes[0].Groups["z"].Value, CultureInfo.InvariantCulture)
                };
                newtri.Vertex2 = new Point3D
                {
                    X = double.Parse(vertexes[1].Groups["x"].Value, CultureInfo.InvariantCulture),
                    Y = double.Parse(vertexes[1].Groups["y"].Value, CultureInfo.InvariantCulture),
                    Z = double.Parse(vertexes[1].Groups["z"].Value, CultureInfo.InvariantCulture)
                };
                newtri.Vertex3 = new Point3D
                {
                    X = double.Parse(vertexes[2].Groups["x"].Value, CultureInfo.InvariantCulture),
                    Y = double.Parse(vertexes[2].Groups["y"].Value, CultureInfo.InvariantCulture),
                    Z = double.Parse(vertexes[2].Groups["z"].Value, CultureInfo.InvariantCulture)
                };

                triangles[i] = newtri;
                i++;
            }
            this.Triangles.Clear();
            this.Triangles.AddRange(triangles);
        }
    }
}
