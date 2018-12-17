using System;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;

using Microsoft.Win32;

using SharpGL;
using SharpGL.Enumerations;

using StlLibrary;

namespace STL_Viewer
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        #region Fields
        private FileStream file = null;
        private StlFile stl = null;
        private Timer dimming1, dimming2;
        private DimState state;
        private OpenFileDialog dialog = new OpenFileDialog();
        private ManipulationMode manipulating = ManipulationMode.None;
        private double x = 0, y = 0, zoom = -4.0, scale = 1, rx = 20.0, ry = 20.0;
        private double startx = 0, starty = 0, startrx = 0, startry = 0;
        private Point clickpos;
        public ViewMode ViewMode { get; set; } = ViewMode.Material;
        private bool isfullscreen;
        public bool IsFullscreen
        {
            get => this.isfullscreen;
            set
            {
                this.isfullscreen = value;
                if (value)
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
        #endregion

        public MainWindow() => InitializeComponent();

        #region Window events
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
                    if (IsFullscreen)
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
        #endregion

        #region Viewer manipulation
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            switch (combo.SelectedIndex)
            {
                case 0:
                    ViewMode = ViewMode.Mesh;
                    return;
                case 1:
                    ViewMode = ViewMode.BasicColor;
                    return;
                case 2:
                    ViewMode = ViewMode.Material;
                    return;
                default:
                    combo.SelectedIndex = 0;
                    ViewMode = ViewMode.Mesh;
                    return;
            }
        }
        private void Opengl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.manipulating != ManipulationMode.None) return;
            if(e.LeftButton == MouseButtonState.Pressed & e.RightButton != MouseButtonState.Pressed)
                this.manipulating = ManipulationMode.Translate;
            else if(e.LeftButton != MouseButtonState.Pressed & e.RightButton == MouseButtonState.Pressed)
                this.manipulating = ManipulationMode.Rotate;
            this.startx = this.x;
            this.starty = this.y;
            this.clickpos = e.GetPosition(this.opengl);
            this.startrx = this.rx;
            this.startry = this.ry;
        }

        private void Opengl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            manipulating = ManipulationMode.None;
        }

        private void Opengl_MouseMove(object sender, MouseEventArgs e)
        {
            if(manipulating == ManipulationMode.Translate)
            {
                Point pos = e.GetPosition(this.opengl);
                if (System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.LeftCtrl) == System.Windows.Input.KeyStates.Down | System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.RightCtrl) == System.Windows.Input.KeyStates.Down)
                {
                    this.x = this.startx + ((pos.X - this.clickpos.X) / 500f);
                    this.y = this.starty + -((pos.Y - this.clickpos.Y) / 500f);
                }
                else
                {
                    this.x = this.startx + ((pos.X-this.clickpos.X) / 50f);
                    this.y = this.starty + -((pos.Y-this.clickpos.Y) / 50f);
                }
                this.label1.Content = $"X: {this.x}, Y: {this.y}";
            }
            else if(manipulating == ManipulationMode.Rotate)
            {
                Point pos = e.GetPosition(this.opengl);
                this.ry = this.startry + ((pos.X - this.clickpos.X)/5f);
                this.rx = this.startrx + ((pos.Y - this.clickpos.Y)/5f);
                this.label1.Content = $"RX: {rx}, RY: {ry}";
            }
        }

        private void Opengl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.LeftShift) == System.Windows.Input.KeyStates.Down | System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.RightShift) == System.Windows.Input.KeyStates.Down)
                this.zoom = Math.Min(Math.Max(-1000f, this.zoom + (e.Delta / 2f)), 200f);
            else if(System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.LeftCtrl) == System.Windows.Input.KeyStates.Down | System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.RightCtrl) == System.Windows.Input.KeyStates.Down)
                this.zoom = Math.Min(Math.Max(-1000f, this.zoom + (e.Delta / 1000f)), 200f);
            else
                this.zoom = Math.Min(Math.Max(-1000f, this.zoom + (e.Delta / 200f)), 200f);
            this.label2.Content = $"Zoom: {this.zoom}";
        }
        private void Opengl_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            this.zoom = (float)e.CumulativeManipulation.Scale.Length;
        }
        #endregion

        #region Loading
        private void OpenButton(object sender, RoutedEventArgs e)
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
            if (this.file == null || !this.file.CanRead) return;
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
                Loading loading = new Loading(file.Name);
                loading.Owner = this.window;
                this.taskbar.ProgressState = TaskbarItemProgressState.Normal;
                progress.ProgressChanged += (sender, e) =>
                {
                    loading.Set(e.Progress);
                    Dispatcher.Invoke(() =>
                    {
                        if (e.Progress < 1)
                            this.taskbar.ProgressValue = e.Progress;
                        else
                        {
                            this.taskbar.ProgressState = TaskbarItemProgressState.None;
                        }
                    });
                };
                (this.stl as StlAscii).LoadAsync(this.file, progress);
                if (!(loading.ShowDialog() ?? false))
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
                Loading loading = new Loading(this.file.Name);
                loading.ShowInTaskbar = false;
                loading.Owner = this.window;
                this.taskbar.ProgressState = TaskbarItemProgressState.Normal;
                progress.ProgressChanged += (sender, e) =>
                {
                    loading.Set(e.Progress);
                    Dispatcher.Invoke(() =>
                    {
                        if (e.Progress < 1)
                            this.taskbar.ProgressValue = e.Progress;
                        else
                        {
                            this.taskbar.ProgressState = TaskbarItemProgressState.None;
                        }
                    });
                };
                (this.stl as StlBinary).LoadAsync(this.file, progress);
                if (!(loading.ShowDialog() ?? false))
                {
                    progress.Cancel = true;
                    this.file?.Close();
                    this.file = null;
                    this.stl = null;
                }
            }
            this.x = this.y = this.rx = this.ry = 0f;
            this.zoom = -4;

            // Autoscaling calculation - search maximum Absolute(x) oR Absolute(y)
            if (this.stl.Triangles.Length/3/4 < 1) this.scale = 1;
            else
            {
                this.scale = 1;
                int length = this.stl.Triangles.Length / 4 / 3;
                for (int i = 0; i < length; i++)
                {
                    for(int j = 1; j < 4; j++)
                    {
                        double val = Math.Abs(this.stl.Triangles[i, j, 0]);
                        if (val > this.scale) this.scale = val;
                        val = Math.Abs(this.stl.Triangles[i, j, 1]);
                        if (val > this.scale) this.scale = val;
                    }
                }
                if (this.scale == 0) this.scale = 1;
                else
                    this.scale = 1 / this.scale;
            }
            this.taskbar.ProgressState = TaskbarItemProgressState.None;
        }
        #endregion

        #region OpenGL
        // Setup OpenGL
        private void Opengl_OpenGLInitialized(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            OpenGL GL = args.OpenGL;
            GL.Enable(OpenGL.GL_DEPTH_TEST);
            float[] global_ambient = new float[] { 0, 0f, 0f, 1.0f };
            
            float[] light0ambient = new float[] { 0.0f, 0.0f, 0.0f, 0f };
            float[] light0diffuse = new float[] { 1f, 1f, 0.3f, 1.0f };
            float[] light0specular = new float[] { 1f, 1f, 1f, 1.0f };
            

            float[] light0pos = new float[] { 0.0f, 0.0f, 20.0f, 1.0f };
            GL.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            GL.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            GL.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            GL.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);
        }

        // Rendering frame
        private void opengl_OpenGLDraw(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;

            //  Clear the color and depth buffers
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            //  Load identity matrix = reset modelview
            gl.LoadIdentity();

            // View mode
            switch (ViewMode)
            {
                case ViewMode.Material:
                    // Turn on 3D lighting
                    gl.Enable(OpenGL.GL_LIGHTING);
                    gl.Enable(OpenGL.GL_LIGHT0);
                    // Auto-redo normals after scaling
                    gl.Enable(OpenGL.GL_NORMALIZE);
                    gl.Enable(OpenGL.GL_RESCALE_NORMAL_EXT);
                    // Set material parameters
                    gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SPECULAR, 1f);
                    gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT, 0);
                    // Render mode
                    gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Filled);
                    gl.ShadeModel(ShadeModel.Smooth);
                    break;
                case ViewMode.BasicColor:
                    gl.Disable(OpenGL.GL_LIGHTING);
                    gl.Disable(OpenGL.GL_LIGHT0);
                    gl.Disable(OpenGL.GL_NORMALIZE);
                    gl.Disable(OpenGL.GL_RESCALE_NORMAL_EXT);
                    gl.PolygonMode(FaceMode.Front,PolygonMode.Filled);
                    // Basic color RGB
                    gl.Color(1f,1f,0);
                    break;
                case ViewMode.Mesh:
                    gl.Disable(OpenGL.GL_LIGHTING);
                    gl.Disable(OpenGL.GL_LIGHT0);
                    gl.Disable(OpenGL.GL_NORMALIZE);
                    gl.Disable(OpenGL.GL_RESCALE_NORMAL_EXT);
                    gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Lines);
                    // Lines settings
                    gl.Color(0,0,1f);
                    gl.LineWidth(0.2f);
                    break;
            }

            //  Moving the drawing axis        
            gl.Translate(this.x, this.y, this.zoom);

            // Rotating
            gl.Rotate ((float)this.rx, (float)this.ry, 20.0f);

            // Auto scale of loaded file
            gl.Scale(this.scale, this.scale, this.scale);

            /* Models */
            if (stl == null || !stl.IsLoaded) // START SCREEN
            {
                gl.Begin(OpenGL.GL_QUADS); // View square from 4 vertexes

                // Top
                gl.Normal(0f, 1f, 0f); // Light reflection vector
                gl.Vertex4f(1.0f, 1.0f, -1.0f,1f);
                gl.Vertex4f(-1.0f, 1.0f, -1.0f, 1f);
                gl.Vertex4f(-1.0f, 1.0f, 1.0f, 1f);
                gl.Vertex4f(1.0f, 1.0f, 1.0f, 1f);

                // Bottom
                gl.Normal(0f,-1f,0f);
                gl.Vertex4f(1.0f, -1.0f, 1.0f, 1f);
                gl.Vertex4f(-1.0f, -1.0f, 1.0f, 1f);
                gl.Vertex4f(-1.0f, -1.0f, -1.0f, 1f);
                gl.Vertex4f(1.0f, -1.0f, -1.0f, 1f);

                // Front
                gl.Normal(0f,0f,1f);
                gl.Vertex4f(1.0f, 1.0f, 1.0f, 1f);
                gl.Vertex4f(-1.0f, 1.0f, 1.0f, 1f);
                gl.Vertex4f(-1.0f, -1.0f, 1.0f, 1f);
                gl.Vertex4f(1.0f, -1.0f, 1.0f, 1f);

                // Back
                gl.Normal(0f,0f,-1f);
                gl.Vertex4f(1.0f, -1.0f, -1.0f, 1f);
                gl.Vertex4f(-1.0f, -1.0f, -1.0f, 1f);
                gl.Vertex4f(-1.0f, 1.0f, -1.0f, 1f);
                gl.Vertex4f(1.0f, 1.0f, -1.0f, 1f);

                // Left
                gl.Normal(-1f,0,0);
                gl.Vertex4f(-1.0f, 1.0f, 1.0f, 1f);
                gl.Vertex4f(-1.0f, 1.0f, -1.0f, 1f);
                gl.Vertex4f(-1.0f, -1.0f, -1.0f, 1f);
                gl.Vertex4f(-1.0f, -1.0f, 1.0f, 1f);

                // Right
                gl.Normal(1f,0,0);
                gl.Vertex4f(1.0f, 1.0f, -1.0f, 1f);
                gl.Vertex4f(1.0f, 1.0f, 1.0f, 1f);
                gl.Vertex4f(1.0f, -1.0f, 1.0f, 1f);
                gl.Vertex4f(1.0f, -1.0f, -1.0f, 1f);
                
                gl.End();
            } else
            {
                gl.Begin(BeginMode.Triangles);
                int length = stl.Triangles.Length / 3 / 4;
                for (uint i = 0; i < length; i++)
                {
                    gl.Normal(stl.Triangles[i,0,0], stl.Triangles[i, 0, 1], stl.Triangles[i, 0, 2]);
                    gl.Vertex4d(stl.Triangles[i, 1, 0], stl.Triangles[i, 1, 1], stl.Triangles[i, 1, 2], 1.0);
                    gl.Vertex4d(stl.Triangles[i, 2, 0], stl.Triangles[i, 2, 1], stl.Triangles[i, 2, 2], 1.0);
                    gl.Vertex4d(stl.Triangles[i, 3, 0], stl.Triangles[i, 3, 1], stl.Triangles[i, 3, 2], 1.0);
                }
                gl.End();
            }
            
            // Calculating matrixes and sending to GPU
            gl.Flush();
            
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // Aby wykryć nadmiarowe wywołania

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.opengl.OpenGLDraw -= this.opengl_OpenGLDraw;
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (file != null && file.CanRead) file.Close();
                    this.file = null;
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
    public enum ManipulationMode { None, Translate, Rotate }
    public enum ViewMode { Mesh = 0, BasicColor = 1, Material = 2 }
}
