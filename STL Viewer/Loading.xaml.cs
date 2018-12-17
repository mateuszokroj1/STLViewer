using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace STL_Viewer
{
    /// <summary>
    /// Logika interakcji dla klasy Loading.xaml
    /// </summary>
    public partial class Loading : Window
    {
        public Loading(string filename)
        {
            InitializeComponent();
            this.text.Text += filename;
        }

        public void Set(double value)
        {
            if (value < 0 || value > 1) throw new ArgumentOutOfRangeException();
            if (value == 1)
            {
                Dispatcher.Invoke(() =>
                {
                    DialogResult = true;
                    Close();
                });
            }
            Dispatcher.Invoke(() => { this.progressbar.Value = value; });
        }
    }
}
