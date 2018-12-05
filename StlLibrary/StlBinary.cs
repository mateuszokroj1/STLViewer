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
        public StlBinary() { }

        public void Load(FileStream file)
        {
            if(file == null) throw new ArgumentNullException();
            if (!file.CanRead || file.Length < 84) throw new ArgumentException("Nieprawidłowy format");

            using (BinaryReader reader = new BinaryReader(file))
            {
                file.Position = 0;
                byte[] str = reader.ReadBytes(80);
                base.Header = Encoding.ASCII.GetString(str);
                UInt32 length = reader.ReadUInt32();
                if (length < 1) throw new ArgumentException("Wymagany przynajmniej jeden trójkąt");

                Triangle[] triangles = new Triangle[length];
                for (uint i = 1; i <= length; i++)
                    triangles[i] = new Triangle
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
                    };
                base.Triangles = triangles;
                this.IsLoaded = true;
            }   
        }

        public Task LoadAsync(FileStream file, Progress progressinfo)
        {
            if (progressinfo == null) progressinfo = new StlLibrary.Progress();
            return Task.Run(()=>
            {
                if (file == null) throw new ArgumentNullException();
                if (!file.CanRead || file.Length < 84) throw new ArgumentException("Nieprawidłowy format");

                using (BinaryReader reader = new BinaryReader(file))
                {
                    file.Position = 0;
                    byte[] str = reader.ReadBytes(80);
                    base.Header = Encoding.ASCII.GetString(str);
                    UInt32 length = reader.ReadUInt32();
                    if (length < 1) throw new ArgumentException("Wymagany przynajmniej jeden trójkąt");

                    progressinfo.SetCount(length);
                    Triangle[] triangles = new Triangle[length];
                    for (uint i = 0; i < length; i++)
                    {
                        triangles[i] = new Triangle
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
                        };
                        progressinfo.SetCurrent(i+1);
                    }
                    base.Triangles = triangles;
                    this.IsLoaded = true;
                }
            });
        }
    }
}
