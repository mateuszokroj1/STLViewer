using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace StlLibrary
{
    /// <summary>
    /// Functionality for STL file format
    /// </summary>
    public abstract class StlFile
    {
        public string Header { get; set; }
        public List<Triangle> Triangles { get; set; } = new List<Triangle>();
    }

    public class StlFileException : ApplicationException
    {
        public StlFileException() : base("StlFileException") { }
        public StlFileException(string message) : base("StlFileException: " + message) { }
    }
}
