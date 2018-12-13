﻿using System;
using System.IO;
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
        private FileStream file = null;
        private StlFile stl = null;
        private Timer dimming1, dimming2;
        private DimState state;
        private OpenFileDialog dialog = new OpenFileDialog();
        private ManipulationMode manipulating = ManipulationMode.None;
        private double x = 0, y = 0, zoom = -4.0, rx = 20.0, ry = 20.0;
        private Point mousestartpos;
        public ViewMode ViewMode { get; set; } = ViewMode.Material;

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
            {
                this.manipulating = ManipulationMode.Translate;
                this.mousestartpos = e.GetPosition(this.window);
            }
            else if(e.LeftButton != MouseButtonState.Pressed & e.RightButton == MouseButtonState.Pressed)
            {
                this.manipulating = ManipulationMode.Translate;
                this.mousestartpos = e.GetPosition(this.window);
            }
        }

        private void Opengl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            manipulating = ManipulationMode.None;
        }

        private void Opengl_MouseMove(object sender, MouseEventArgs e)
        {
            if(manipulating == ManipulationMode.Translate)
            {
                Point current = e.GetPosition(this.window);
                this.x = -(this.mousestartpos - current).X / 50;
                this.y = (this.mousestartpos - current).Y / 50;
            }
            else if(manipulating == ManipulationMode.Rotate)
            {

            }
        }

        private void Opengl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            this.zoom = Math.Min(Math.Max(-200f, this.zoom + (e.Delta / 100f)), 200f);
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
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (e.Progress < 1)
                            this.taskbar.ProgressValue = e.Progress;
                        else
                        {
                            this.taskbar.ProgressState = TaskbarItemProgressState.None;
                            loading.DialogResult = true;
                            loading.Close();
                        }
                    }));
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
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Progress < 1)
                        this.taskbar.ProgressValue = e.Progress;
                    else
                    {
                        this.taskbar.ProgressState = TaskbarItemProgressState.None;
                        loading.DialogResult = true;
                        loading.Close();
                    }
                }));
                (this.stl as StlBinary).LoadAsync(this.file, progress);
                if (!(loading.ShowDialog() ?? false))
                {
                    progress.Cancel = true;
                    this.file?.Close();
                    this.file = null;
                    this.stl = null;
                }
                this.taskbar.ProgressState = TaskbarItemProgressState.None;
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

        private void Opengl_OpenGLInitialized(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            OpenGL GL = args.OpenGL;
            GL.Enable(OpenGL.GL_DEPTH_TEST);
            float[] global_ambient = new float[] { 0, 0f, 0f, 1.0f };
            float[] light0pos = new float[] { 0.0f, 5.0f, 10.0f, 1.0f };
            float[] light0ambient = new float[] { 0.5f, 0.5f, 0.2f, 1.0f };
            float[] light0diffuse = new float[] { 1f, 1f, 0.3f, 1.0f };
            float[] light0specular = new float[] { 1f, 1f, 1f, 1.0f };

            float[] lmodel_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            GL.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, lmodel_ambient);

            GL.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);
            GL.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            GL.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            GL.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            GL.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);
            

            GL.ShadeModel(OpenGL.GL_SMOOTH);
        }

        private void Opengl_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            this.zoom = (float)e.CumulativeManipulation.Scale.Length;
        }

        private void opengl_OpenGLDraw(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;

            //  Clear the color and depth buffers
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            //  Load identity matrix = reset modelview
            gl.LoadIdentity();

            switch (ViewMode)
            {
                case ViewMode.Material:
                    gl.Disable(OpenGL.GL_CULL_FACE);
                    gl.Enable(OpenGL.GL_LIGHTING);
                    gl.Enable(OpenGL.GL_LIGHT0);
                    break;
                case ViewMode.BasicColor:
                    gl.Disable(OpenGL.GL_CULL_FACE);
                    gl.Disable(OpenGL.GL_LIGHTING);
                    gl.Disable(OpenGL.GL_LIGHT0);
                    gl.Color(1f,1f,0);
                    break;
                case ViewMode.Mesh:
                    gl.Disable(OpenGL.GL_LIGHTING);
                    gl.Disable(OpenGL.GL_LIGHT0);
                    gl.Enable(OpenGL.GL_CULL_FACE);
                    gl.CullFace();
                    break;
            }
            //  Moving the drawing axis
            gl.Translate(this.x, this.y, this.zoom);
            // Rotating
            gl.Rotate ((float)this.rx, (float)this.ry, 20.0f);
            if (stl == null || !stl.IsLoaded) // START SCREEN
            {
                gl.Begin(OpenGL.GL_QUADS);
                
                gl.Vertex4f(-1f, -1f, 1f, 1f);
                gl.Vertex4f(-1f, 1f, 1f, 1f);
                gl.Vertex4f(1f, 1f, 1f, 1f);
                gl.Vertex4f(1f,-1f, 1f, 1f);
                gl.Vertex4f(-1f, 1f, -1f, 1f);
                gl.Vertex4f(1f, 1f, -1f, 1f);
                gl.Vertex4f(1f, -1f, -1f, 1f);
                gl.Vertex4f(-1f, -1f, -1f, 1f);

                gl.End();
            } else
            {
                gl.Begin(BeginMode.Triangles);
                    gl.Color(1f,1f,1f);
                    foreach(Triangle facet in stl.Triangles)
                    {
                        gl.Normal(facet.Normal.X, facet.Normal.Y, facet.Normal.Z);
                        gl.Vertex4d(facet.Vertex1.X, facet.Vertex1.Y, facet.Vertex1.Z, 1.0);
                        gl.Vertex4d(facet.Vertex2.X, facet.Vertex2.Y, facet.Vertex2.Z, 1.0);
                        gl.Vertex4d(facet.Vertex3.X, facet.Vertex3.Y, facet.Vertex3.Z, 1.0);
                    }
                gl.End();
            }
            

            //  Flush OpenGL.
            gl.Flush();
            
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.opengl.OpenGLDraw -= this.opengl_OpenGLDraw;
            Dispose(true);
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
