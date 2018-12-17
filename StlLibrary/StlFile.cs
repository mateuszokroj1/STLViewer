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
        public string Header { get; set; } = string.Empty;
        public double[,,] Triangles { get; set; }
        public bool IsLoaded { get; protected set; } = false;
    }

    [Serializable]
    public class StlFileException : ApplicationException
    {
        public StlFileException() : base("StlFileException") { }
        public StlFileException(string message) : base("StlFileException: " + message) { }
    }
}
