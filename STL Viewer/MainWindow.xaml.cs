using System;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using Microsoft.Win32;

using SharpGL;
using StlLibrary;

namespace STL_Viewer
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private FileStream file = null;
        private StlFile stl = null;
        private Timer dimming1, dimming2;
        private DimState state;
        private OpenFileDialog dialog = new OpenFileDialog();

        public MainWindow()
        {
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            state = DimState.Default;
            dimming1 = new Timer(4000);
            dimming1.Elapsed += (obj, ev) =>
            {
                Dispatcher.Invoke(() =>
                {
                    this.toolBar.Visibility = Visibility.Hidden;
                    dimming1.Stop();
                    this.state = DimState.Dimmed1;
                    if (this.fullscreen.IsChecked ?? false)
                        dimming2.Start();
                });
            };
            dimming1.Start();
            dimming2 = new Timer(4000);
            dimming2.Elapsed += (obj, ev) =>
            {
                Dispatcher.Invoke(() =>
                {
                    this.toolBar.Visibility = Visibility.Hidden;
                    this.opengl.Cursor = Cursors.None;
                    this.state = DimState.Dimmed2;
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
            this.file?.Close();
            try { file = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read); }
            catch(IOException exc) { MessageBox.Show($"Wystąpił błąd podczas odczytu pliku: {exc.Message}", "Błąd odczytu", MessageBoxButton.OK, MessageBoxImage.Error); file = null; }
            if(!file.CanRead) { MessageBox.Show("Pliku nie można odczytać", "Błąd odczytu", MessageBoxButton.OK, MessageBoxImage.Error); file = null; }
            Loading();
        }

        private void Loading()
        {
            if (this.file == null || (this.file?.CanRead ?? false)) return;
            bool ascii = true;
            for (uint i = 0; i < file.Length; i++)
                if (file.ReadByte() > 128)
                {
                    ascii = false;
                    break;
                }

            if (ascii)
            {
                this.stl = new StlAscii();
                Progress progress = new Progress();
                Loading loading = new Loading();
                this.taskbar.ProgressState = TaskbarItemProgressState.Normal;
                progress.ProgressChanged += (sender,e) =>
                {
                    if (e.Progress < 1)
                        this.taskbar.ProgressValue = e.Progress;
                    else
                        this.taskbar.ProgressState = TaskbarItemProgressState.None;
                    loading.Set((float)Math.Round(e.Progress,2));
                };
                (this.stl as StlAscii).LoadAsync(this.file, progress);
                if (loading.ShowDialog() ?? false)
                {
                    progress.Cancel = true;
                    this.file?.Close();
                    this.file = null;
                    this.stl = null;
                }
            }
            else
            {
                this.stl = new StlBinary();
                Progress progress = new Progress();
                Loading loading = new Loading();
                this.taskbar.ProgressState = TaskbarItemProgressState.Normal;
                progress.ProgressChanged += (sender, e) =>
                {
                    if (e.Progress < 1)
                        this.taskbar.ProgressValue = e.Progress;
                    else
                        this.taskbar.ProgressState = TaskbarItemProgressState.None;
                    loading.Set((float)Math.Round(e.Progress,2));
                };
                (this.stl as StlBinary).LoadAsync(this.file, progress);
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
            dimming1.Start();
            if (this.state == DimState.Default) return;
            this.toolBar.Visibility = Visibility.Visible;
            this.opengl.Cursor = Cursors.Cross;
            this.state = DimState.Default;
        }

        private bool isfullscreen;
        public bool IsFullscreen
        {
            get => this.isfullscreen;
            set
            {
                this.isfullscreen = value;
                if(value)
                {
                    ResizeMode = ResizeMode.NoResize;
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.None;
                    Top = this.Left = 0;
                    Width = SystemParameters.PrimaryScreenWidth;
                    Height = SystemParameters.PrimaryScreenHeight;
                    Topmost = true;
                }
                else
                {
                    ResizeMode = ResizeMode.CanResize;
                    Topmost = false;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    Top = this.Left = 0;
                    WindowState = WindowState.Maximized;
                }
            }
        }

        private void opengl_OpenGLDraw(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            if (!this.IsActive) return;
            OpenGL gl = args.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();
            
            gl.Color((byte)255,(byte)255,(byte)255);
            gl.Begin(OpenGL.GL_TRIANGLES);
                gl.Vertex4f(-1,0,0,1);
                gl.Vertex4f(1,0,0,1);
                gl.Vertex4f(0,1,0,1);
            gl.End();

            if (file != null && stl != null && stl.IsLoaded && stl.Triangles.Count() > 0)
            {
                
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
            this.opengl.OpenGLDraw -= this.opengl_OpenGLDraw;
            this.Dispose(true);
        }

        #region IDisposable Support
        private bool disposedValue = false; // Aby wykryć nadmiarowe wywołania

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (file != null && file.CanRead) file.Close();
                    this.stl = null;
                    this.dialog = null;
                    this.dimming1.Close();
                    this.dimming1 = null;
                    this.dimming2.Close();
                    this.dimming2 = null;
                }

                GC.Collect();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
    public enum DimState { Default, Dimmed1, Dimmed2 }
}
