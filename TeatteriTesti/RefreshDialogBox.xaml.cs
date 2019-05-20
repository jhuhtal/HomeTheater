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

namespace Teatteri
{
    /// <summary>
    /// Interaction logic for RefreshDialogBox.xaml
    /// </summary>
    public partial class RefreshDialogBox : Window
    {
        public RefreshDialogBox()
        {

            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            NewMoviesText.DataContext = StaticVariables.NewMovies; 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
