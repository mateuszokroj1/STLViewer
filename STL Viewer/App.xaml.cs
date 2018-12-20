using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace STL_Viewer
{
    /// <summary>
    /// Logika interakcji dla klasy App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            STL_Viewer.MainWindow window = null;
            if (e.Args.Length == 0)
                window = new MainWindow();
            else if(File.Exists(e.Args[0]))
            {
                FileStream file = null;
                try { file = new FileStream(e.Args[0], FileMode.Open, FileAccess.Read, FileShare.Read); }
                catch(IOException exc)
                {
                    Type t = exc.GetType();
                    MessageBox.Show($"Błąd odczytu pliku:\n\rType: {t.FullName}\n\rMessage: {exc.Message}","STL Viewer", MessageBoxButton.OK, MessageBoxImage.Error);
                    window = new MainWindow();
                }
                if (file != null) window = new MainWindow(file);
            }
            else
            {
                MessageBox.Show("Nieprawidłowy plik podany w pierwszym argumencie","STL Viewer", MessageBoxButton.OK, MessageBoxImage.Warning);
                window = new MainWindow();
            }

            window.Show();
        }
    }
}
