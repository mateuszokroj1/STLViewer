using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace StlLibrary
{
    public struct Triangle
    {
        public Point3D Normal { get; set; }
        public Point3D Vertex1 { get; set; }
        public Point3D Vertex2 { get; set; }
        public Point3D Vertex3 { get; set; }
        public UInt16 Argument { get; set; }
    }
}
