using System;
using System.IO;

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
    public class StlFileException : FileFormatException
    {
        public StlFileException() : base("StlFileException") { }
        public StlFileException(string message) : base("StlFileException: " + message) { }
    }
}
