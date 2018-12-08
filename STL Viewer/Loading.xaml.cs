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
        public Loading()
        {
            InitializeComponent();
        }

        public void Set(float progress)
        {
            if (progress > 1 || progress < 0) throw new ArgumentOutOfRangeException("Progress must be value of range [0,1].");
            Dispatcher.Invoke(()=>
            {
                this.Progress.Minimum = 0;
                this.Progress.Maximum = 1;
                this.Progress.Value = progress;
            });
            if (progress == 1)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
