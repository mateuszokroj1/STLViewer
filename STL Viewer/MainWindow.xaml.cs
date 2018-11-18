using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Timers;
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
using Microsoft.Win32;

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
        private Timer dimming1, dimming2;
        private OpenFileDialog dialog = new OpenFileDialog();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dimming1 = new Timer(4000);
            dimming1.Elapsed += (obj, ev) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.toolBar.Visibility = Visibility.Hidden;
                    dimming1.Stop();
                    if (this.fullscreen.IsChecked ?? false)
                        dimming2.Start();
                });
            };
            dimming1.Start();
            dimming2 = new Timer(4000);
            dimming2.Elapsed += (obj, ev) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.toolBar.Visibility = Visibility.Hidden;
                    this.opengl.ForceCursor = true;
                    this.opengl.Cursor = Cursors.None;
                    dimming2.Stop();
                });
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dialog.CheckFileExists = true;
            dialog.DefaultExt = "stl";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.Multiselect = false;
            dialog.Title = "Otwórz plik STL";
            dialog.Filter = "Stereolithography 3D Model (STL)|*.stl";
            if (!dialog.ShowDialog(this) ?? false || dialog.FileNames.Length < 1 || !File.Exists(dialog.FileName)) return;
            try { file = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read); }
            catch(IOException exc) { MessageBox.Show($"Wystąpił błąd podczas odczytu pliku: {exc.Message}", "Błąd odczytu", MessageBoxButton.OK, MessageBoxImage.Error); file = null; }
            if(!file.CanRead) { MessageBox.Show("Pliku nie można odczytać", "Błąd odczytu", MessageBoxButton.OK, MessageBoxImage.Error); file = null; }
            bool ascii = true;
            for (uint i = 0; i < file.Length; i++)
                if(file.ReadByte() > 128)
                {
                    ascii = false;
                    break;
                }

            if(ascii)
            {
                this.stl = new StlAscii();
                Progress progress = new Progress();
                Loading loading = new Loading(progress);
                (this.stl as StlAscii).LoadAsync(this.file, progress);
                if(loading.ShowDialog() ?? false)
                {
                    this.file?.Close();
                    this.file = null;
                    this.stl = null;
                }
            }
            else
            {
                this.stl = new StlBinary();
                Progress progress = new Progress();
                Loading loading = new Loading(progress);
                (this.stl as StlAscii).LoadAsync(this.file, progress);
                if (loading.ShowDialog() ?? false)
                {
                    this.file?.Close();
                    this.file = null;
                    this.stl = null;
                }
            }
        }

        private void window_MouseMove(object sender, MouseEventArgs e)
        {
            dimming1.Stop();
            dimming2.Stop();
            this.toolBar.Visibility = Visibility.Visible;
            this.opengl.ForceCursor = false;
            this.opengl.Cursor = Cursors.Cross;
            dimming1.Start();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if((sender as CheckBox).IsChecked ?? false) // Full screen
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

        private void opengl_OpenGLDraw(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            if (!this.IsFocused || !this.IsActive) return;
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
                gl.Color((byte)128, (byte)128, (byte)128);
                gl.Begin(OpenGL.GL_TRIANGLES);

                foreach (Triangle tri in stl.Triangles)
                {
                    gl.Vertex4d(tri.Vertex1.X, tri.Vertex1.Y, tri.Vertex1.Z, 1);
                    gl.Vertex4d(tri.Vertex2.X, tri.Vertex2.Y, tri.Vertex2.Z, 1);
                    gl.Vertex4d(tri.Vertex3.X, tri.Vertex3.Y, tri.Vertex3.Z, 1);
                }

                gl.End();
            }
            gl.Flush();
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.stl = null;
            this.file?.Close();
            this.dimming1?.Close();
            this.dimming2?.Close();
            GC.Collect();
        }
    }
}
