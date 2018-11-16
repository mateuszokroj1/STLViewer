using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using SharpGL;
using StlLibrary;

namespace STL_Viewer
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileStream file = null;
        private StlLibrary.StlFile stl = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void opengl_OpenGLDraw(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();
           
            gl.Color(0, 0, 0);
            gl.Begin(OpenGL.GL_QUADS);
                    gl.Vertex(0, 0);
                    gl.Vertex(opengl.Width, 0);
                    gl.Vertex(0, opengl.Height);
                    gl.Vertex(opengl.Width, opengl.Height);
            gl.End();

            if (file != null && stl != null && stl.IsLoaded && stl.Triangles.Count() > 0)
            {
                gl.LoadIdentity();
                //gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_DIFFUSE);
                gl.Color((byte)128,(byte)128,(byte)128);
                gl.Begin(OpenGL.GL_TRIANGLES);

                foreach (Triangle tri in stl.Triangles)
                {
                    gl.Vertex4d(tri.Vertex1.X, tri.Vertex1.Y, tri.Vertex1.Z,0);
                    gl.Vertex4d(tri.Vertex2.X, tri.Vertex2.Y, tri.Vertex2.Z, 0);
                    gl.Vertex4d(tri.Vertex3.X, tri.Vertex3.Y, tri.Vertex3.Z, 0);
                }

                gl.End();
            }
            gl.Flush();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Shell
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if((sender as CheckBox).IsChecked.GetValueOrDefault(false)) // Full screen
            {
                this.ResizeMode = ResizeMode.NoResize;
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.None;
                this.Top = this.Left = 0;
                this.Width = SystemParameters.PrimaryScreenWidth;
                this.Height = SystemParameters.PrimaryScreenHeight;
                this.Topmost = true;  
            }
            else
            {
                this.ResizeMode = ResizeMode.CanResize;
                this.Topmost = false;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.Top = this.Left = 0;
                this.WindowState = WindowState.Maximized;
            }
        }
    }
}
