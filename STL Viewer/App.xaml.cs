using System;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace StlViewer
{
    /// <summary>
    /// Logika interakcji dla klasy App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            CultureInfo culture;
            string appdata = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "STL Viewer");
            dynamic settings = new { lang = "" };
            bool valid = false;
            if (File.Exists(Path.Combine(appdata, "settings.json")))
            {
                using(var file = File.OpenText(Path.Combine(appdata, "settings.json")))
                {
                    string json = file.ReadToEnd();
                    JObject job;
                    try { job = JObject.Parse(json); }
                    catch(JsonReaderException) { job = null; }
                    
                    if (job != null)
                    {
                        JSchema schema = JSchema.Parse(@"{  '$schema': 'http://json-schema.org/draft-04/schema', 'title': 'STL Viewer Settings JSON Schema',  'type': 'object',  'required': ['lang'],  'properties': {    'lang': { 'type': 'string',      'description': 'Language of the UI',      'pattern': '^([A-z]{2})(-[A-z]{2})?$'    }  }}");
                        valid = job.IsValid(schema);
                    }
                    settings = JsonConvert.DeserializeObject(json);
                }
                if (!valid)
                {
                    settings = new { lang = CultureInfo.InstalledUICulture.Name };
                    using (FileStream file = new FileStream(Path.Combine(appdata, "settings.json"), FileMode.Open, FileAccess.Write, FileShare.None))
                    {
                        file.SetLength(0);
                        byte[] utf8 = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(settings));
                        file.Write(utf8, 0, utf8.Length);
                        file.Flush();
                    }
                }
                culture = new CultureInfo(settings.lang.ToString());
            }
            else
            {
                settings = new { lang = CultureInfo.InstalledUICulture.Name };
                string json = JsonConvert.SerializeObject(settings);
                if (!Directory.Exists(appdata))
                    Directory.CreateDirectory(appdata);
                using (var file = File.CreateText(Path.Combine(appdata, "settings.json")))
                {
                    file.Write(json);
                    file.Flush();
                }
                culture = CultureInfo.InstalledUICulture;
            }
            Thread.CurrentThread.CurrentUICulture = culture;
            switch(culture.TwoLetterISOLanguageName)
            {
                case "pl":
                    Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"Properties\Resources.pl.xaml", UriKind.Relative) });
                    break;
                case "en":
                default:
                    Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"Properties\Resources.xaml", UriKind.Relative) });
                    break;
            }
            MainWindow window = null;
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
