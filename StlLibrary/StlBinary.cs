using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StlLibrary
{
    public class StlBinary : StlFile
    {
        public StlBinary() { }

        public void Load(FileStream file)
        {
            if(file == null) throw new ArgumentNullException();
            if (!file.CanRead || file.Length < 84) throw new ArgumentException("Invalid format");

            using (BinaryReader reader = new BinaryReader(file))
            {
                file.Position = 0;
                byte[] str = reader.ReadBytes(80);
                base.Header = Encoding.ASCII.GetString(str);
                UInt32 length = reader.ReadUInt32();
                if (length < 1) throw new ArgumentException("At least one triangle is required");

                this.Triangles = new double[length,4,3];
                for (uint i = 0; i < length; i++)
                {
                    this.Triangles[i, 0, 0] = reader.ReadSingle();
                    this.Triangles[i, 0, 1] = reader.ReadSingle();
                    this.Triangles[i, 0, 2] = reader.ReadSingle();

                    this.Triangles[i, 1, 0] = reader.ReadSingle();
                    this.Triangles[i, 1, 1] = reader.ReadSingle();
                    this.Triangles[i, 1, 2] = reader.ReadSingle();

                    this.Triangles[i, 2, 0] = reader.ReadSingle();
                    this.Triangles[i, 2, 1] = reader.ReadSingle();
                    this.Triangles[i, 2, 2] = reader.ReadSingle();

                    this.Triangles[i, 3, 0] = reader.ReadSingle();
                    this.Triangles[i, 3, 1] = reader.ReadSingle();
                    this.Triangles[i, 3, 2] = reader.ReadSingle();

                    reader.ReadUInt16(); // Argument
                }
                this.IsLoaded = true;
            }   
        }

        public Task LoadAsync(FileStream file, Progress progressinfo)
        {
            if (progressinfo == null) progressinfo = new Progress();
            return Task.Run(() =>
            {
                if (file == null) throw new ArgumentNullException();
                if (!file.CanRead || file.Length < 84) throw new ArgumentException("Invalid format");

                using (BinaryReader reader = new BinaryReader(file))
                {
                    file.Position = 0;
                    byte[] str = reader.ReadBytes(80);
                    base.Header = Encoding.ASCII.GetString(str);
                    UInt32 length = reader.ReadUInt32();
                    if (length < 1) throw new ArgumentException("At least one triangle is required");

                    progressinfo.SetCount(length);
                    this.Triangles = new double[length, 4, 3];
                    for (uint i = 0; i < length; i++)
                    {
                        if (progressinfo.Cancel) return;
                        this.Triangles[i, 0, 0] = reader.ReadSingle();
                        this.Triangles[i, 0, 1] = reader.ReadSingle();
                        this.Triangles[i, 0, 2] = reader.ReadSingle();

                        this.Triangles[i, 1, 0] = reader.ReadSingle();
                        this.Triangles[i, 1, 1] = reader.ReadSingle();
                        this.Triangles[i, 1, 2] = reader.ReadSingle();

                        this.Triangles[i, 2, 0] = reader.ReadSingle();
                        this.Triangles[i, 2, 1] = reader.ReadSingle();
                        this.Triangles[i, 2, 2] = reader.ReadSingle();

                        this.Triangles[i, 3, 0] = reader.ReadSingle();
                        this.Triangles[i, 3, 1] = reader.ReadSingle();
                        this.Triangles[i, 3, 2] = reader.ReadSingle();

                        reader.ReadUInt16(); // Argument
                        progressinfo.SetCurrent(i+1);
                    }
                    this.IsLoaded = true;
                }
            });
        }
    }
}
