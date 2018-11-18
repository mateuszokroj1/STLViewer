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
        private StlLibrary.Progress Info;
        public Loading(StlLibrary.Progress progressinfo)
        {
            InitializeComponent();
            Info = progressinfo;
        }
    }
}
