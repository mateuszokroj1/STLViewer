using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Media3D;

namespace StlLibrary
{
    public class StlBinary : StlFile
    {
        public StlBinary(FileStream file)
        {
            if(file == null) throw new ArgumentNullException();
            if (!file.CanRead || file.Length < 84) throw new ArgumentException("Nieprawidłowy format");

            using (BinaryReader reader = new BinaryReader(file))
            {
                file.Position = 0;
                byte[] str = reader.ReadBytes(80);
                base.Header = Encoding.ASCII.GetString(str);
                if (base.Triangles.Count > 0) base.Triangles.Clear();
                UInt32 length = reader.ReadUInt32();
                if (length < 1) throw new ArgumentException("Wymagany przynajmniej jeden trójkąt");

                for (uint i = 1; i <= length; i++)
                    base.Triangles.Add(new Triangle
                    {
                        Normal = new Point3D
                        {
                            X = reader.ReadSingle(),
                            Y = reader.ReadSingle(),
                            Z = reader.ReadSingle()
                        },
                        Vertex1 = new Point3D
                        {
                            X = reader.ReadSingle(),
                            Y = reader.ReadSingle(),
                            Z = reader.ReadSingle()
                        },
                        Vertex2 = new Point3D
                        {
                            X = reader.ReadSingle(),
                            Y = reader.ReadSingle(),
                            Z = reader.ReadSingle()
                        },
                        Vertex3 = new Point3D
                        {
                            X = reader.ReadSingle(),
                            Y = reader.ReadSingle(),
                            Z = reader.ReadSingle()
                        },
                        Argument = reader.ReadUInt16()
                    });
            }
        }
    }
}
