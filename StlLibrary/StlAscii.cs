using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StlLibrary
{
    public class StlAscii : StlFile
    {
        public StlAscii() { }

        public void Load(FileStream file)
        {
            if (file == null) throw new ArgumentNullException();
            if (!file.CanRead || file.Length < 4) throw new ArgumentException("Can not read or incorrect file");
            string data;
            byte[] ascii = new byte[file.Length];
            file.Position = 0;
            for (ulong j = 0; (long)j < file.Length; j++)
            {
                int c = file.ReadByte();
                if (c < 0 || c > 127) break;
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
            if (match == null || !match.Success) throw new StlFileException("Incorrect ASCII file format");

            string name1 = match.Groups["name1"].Captures[0].Value, name2 = match.Groups["name2"].Captures[0].Value;
            if (!name1.Equals(name2)) throw new StlFileException($"SOLID \"{name1}\" has not been properly completed");

            // Read facets
            MatchCollection facets = new Regex(@"\s*FACET\s+NORMAL\s+(?<i>\d+\.\d+)\s+(?<j>\d+\.\d+)\s+(?<k>\d+\.\d+)\s+OUTER\s+LOOP\s*(\s*VERTEX\s+(\d+\.\d+)\s+(\d+\.\d+)\s+(\d+\.\d+)\s*){3}\s+ENDLOOP\s+ENDFACET\s*").Matches(data);
            if (facets.Count < 1) throw new StlFileException("A minimum one triangle was expected");
            this.Triangles = new double[facets.Count,4,3];
            var res = Parallel.For(0, facets.Count, new Action<int>(i =>
            {
                this.Triangles[i, 0, 0] = double.Parse(facets[i].Groups["i"].Value, CultureInfo.InvariantCulture);
                this.Triangles[i, 0, 1] = double.Parse(facets[i].Groups["j"].Value, CultureInfo.InvariantCulture);
                this.Triangles[i, 0, 2] = double.Parse(facets[i].Groups["k"].Value, CultureInfo.InvariantCulture);

                // Read vertexes
                MatchCollection vertexes = new Regex(@"\s*VERTEX\s+(?<x>\d+\.\d+)\s+(?<y>\d+\.\d+)\s+(?<z>\d+\.\d+)\s*").Matches(facets[i].Value);

                this.Triangles[i, 1, 0] = double.Parse(vertexes[0].Groups["x"].Value, CultureInfo.InvariantCulture);
                this.Triangles[i, 1, 1] = double.Parse(vertexes[0].Groups["y"].Value, CultureInfo.InvariantCulture);
                this.Triangles[i, 1, 2] = double.Parse(vertexes[0].Groups["z"].Value, CultureInfo.InvariantCulture);

                this.Triangles[i, 2, 0] = double.Parse(vertexes[1].Groups["x"].Value, CultureInfo.InvariantCulture);
                this.Triangles[i, 2, 1] = double.Parse(vertexes[1].Groups["y"].Value, CultureInfo.InvariantCulture);
                this.Triangles[i, 2, 2] = double.Parse(vertexes[1].Groups["z"].Value, CultureInfo.InvariantCulture);

                this.Triangles[i, 3, 0] = double.Parse(vertexes[2].Groups["x"].Value, CultureInfo.InvariantCulture);
                this.Triangles[i, 3, 1] = double.Parse(vertexes[2].Groups["y"].Value, CultureInfo.InvariantCulture);
                this.Triangles[i, 3, 2] = double.Parse(vertexes[2].Groups["z"].Value, CultureInfo.InvariantCulture);
            }));
            this.IsLoaded = true;
        }

        public Task LoadAsync(FileStream file, Progress progressinfo)
        {
            if (progressinfo == null) progressinfo = new Progress();
            return Task.Run(()=>
            {
                if (file == null) throw new ArgumentNullException();
                if (!file.CanRead || file.Length < 4) throw new ArgumentException("Can not read or incorrect file");
                string data;
                byte[] ascii = new byte[file.Length];
                file.Position = 0;
                for (ulong j = 0; (long)j < file.Length; j++)
                {
                    if (progressinfo.Cancel) return;
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
                if (match == null || !match.Success) throw new StlFileException("Incorrect ASCII file format");

                string name1 = match.Groups["name1"].Captures[0].Value, name2 = match.Groups["name2"].Captures[0].Value;
                if (!name1.Equals(name2)) throw new StlFileException($"SOLID \"{name1}\" nie został poprawnie zakończony");

                MatchCollection facets = new Regex(@"\s*FACET\s+NORMAL\s+(?<i>\d+\.\d+)\s+(?<j>\d+\.\d+)\s+(?<k>\d+\.\d+)\s+OUTER\s+LOOP\s*(\s*VERTEX\s+(\d+\.\d+)\s+(\d+\.\d+)\s+(\d+\.\d+)\s*){3}\s+ENDLOOP\s+ENDFACET\s*").Matches(data);
                if (facets.Count < 1) throw new StlFileException("A minimum one triangle was expected");
                progressinfo.SetCount((ulong)facets.Count);
                this.Triangles = new double[facets.Count, 4, 3];
                for (uint i = 0; i < facets.Count; i++)
                {
                    if (progressinfo.Cancel) return;
                    this.Triangles[i, 0, 0] = double.Parse(facets[(int)i].Groups["i"].Value, CultureInfo.InvariantCulture);
                    this.Triangles[i, 0, 1] = double.Parse(facets[(int)i].Groups["j"].Value, CultureInfo.InvariantCulture);
                    this.Triangles[i, 0, 2] = double.Parse(facets[(int)i].Groups["k"].Value, CultureInfo.InvariantCulture);

                    MatchCollection vertexes = new Regex(@"\s*VERTEX\s+(?<x>\d+\.\d+)\s+(?<y>\d+\.\d+)\s+(?<z>\d+\.\d+)\s*").Matches(facets[(int)i].Value);

                    this.Triangles[i, 1, 0] = double.Parse(vertexes[0].Groups["x"].Value, CultureInfo.InvariantCulture);
                    this.Triangles[i, 1, 1] = double.Parse(vertexes[0].Groups["y"].Value, CultureInfo.InvariantCulture);
                    this.Triangles[i, 1, 2] = double.Parse(vertexes[0].Groups["z"].Value, CultureInfo.InvariantCulture);

                    this.Triangles[i, 2, 0] = double.Parse(vertexes[1].Groups["x"].Value, CultureInfo.InvariantCulture);
                    this.Triangles[i, 2, 1] = double.Parse(vertexes[1].Groups["y"].Value, CultureInfo.InvariantCulture);
                    this.Triangles[i, 2, 2] = double.Parse(vertexes[1].Groups["z"].Value, CultureInfo.InvariantCulture);

                    this.Triangles[i, 3, 0] = double.Parse(vertexes[2].Groups["x"].Value, CultureInfo.InvariantCulture);
                    this.Triangles[i, 3, 1] = double.Parse(vertexes[2].Groups["y"].Value, CultureInfo.InvariantCulture);
                    this.Triangles[i, 3, 2] = double.Parse(vertexes[2].Groups["z"].Value, CultureInfo.InvariantCulture);
                    progressinfo.SetCurrent(i + 1);
                }
                this.IsLoaded = true;
            });
        }
    }
}
