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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace Teatteri
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Movie> movies;
        ObservableCollection<Movie> moviesFiltered;
        FilterArguments filterArguments;
        YearList yearList;
        public DataBaseManager dbm;

        public MainWindow()
        {
            App.Current.Shutdown();            //delete
            WindowState = WindowState.Maximized;                   
            WindowStyle = WindowStyle.None;

            dbm = new DataBaseManager();
            this.filterArguments = new FilterArguments();

            movies = dbm.GetMovieListFromDB();
            foreach (Movie mov in movies)
            {
                mov.Original_title = mov.Original_title + " [" + mov.Year + "]";
            }

            yearList = new YearList();
            
            InitializeComponent();

            yearBox1.DataContext = yearList.Years;
            yearBox2.DataContext = yearList.Years;
            yearBox1.SelectedValue = "1900";
            yearBox2.SelectedValue = yearList.Year;

            Random rand = new Random();
            int y = rand.Next(0, movies.Count-1);

            moviesFiltered = MovieFilter.Execute(movies, filterArguments.Genres);

            MoviePanel.DataContext = moviesFiltered;
            MovieGrid.DataContext = moviesFiltered[y];
        }

        private void Button_MouseClick(object sender, MouseButtonEventArgs e)
        {
            if (MoviePanel.SelectedIndex != -1)
            {
                MovieGrid.DataContext = moviesFiltered[(int)MoviePanel.SelectedIndex];        
            }

        }

        private void MoviePanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MoviePanel.SelectedIndex != -1)
            {
                MoviePanel.DataContext = moviesFiltered;

                MovieGrid.DataContext = moviesFiltered[(int)MoviePanel.SelectedIndex];
            }
        }

        private void GenreCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            string genreName = (sender as DependencyObject).GetValue(CheckBox.NameProperty).ToString();
            if (genreName == "ScienceFiction")
                genreName = "Science Fiction";
            else if (genreName == "TVMovie")
                genreName = "TV Movie";

            foreach (string genre in filterArguments.Genres)
            {
                if (genreName == genre)
                    return;
            }

            filterArguments.Genres.Add(genreName);
            moviesFiltered = MovieFilter.Execute(movies, filterArguments.Genres, filterArguments.lowerVote, filterArguments.lowerYear, filterArguments.upperYear);
            MoviePanel.DataContext = moviesFiltered;
        }

        private void GenreCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            string genreName = (sender as DependencyObject).GetValue(CheckBox.NameProperty).ToString();
            if (genreName == "ScienceFiction")
                genreName = "Science Fiction";
            else if (genreName == "TVMovie")
                genreName = "TV Movie";

            int i = filterArguments.Genres.IndexOf(genreName);
            filterArguments.Genres.RemoveAt(i);

            moviesFiltered = MovieFilter.Execute(movies, filterArguments.Genres, filterArguments.lowerVote, filterArguments.lowerYear, filterArguments.upperYear);
            MoviePanel.DataContext = moviesFiltered;

        }

        private void ALL_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox box in GenreBoxGrid.Children)
            {
                box.IsChecked = false;
            }
        }

        private void ALL_Checked(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox box in GenreBoxGrid.Children)
            {
                box.IsChecked = true;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int stars = (int)e.NewValue;
            filterArguments.lowerVote = stars;
            moviesFiltered = MovieFilter.Execute(movies, filterArguments.Genres, filterArguments.lowerVote, filterArguments.lowerYear, filterArguments.upperYear);
            MoviePanel.DataContext = moviesFiltered;
        }

        private void yearBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filterArguments.upperYear = Int32.Parse((string)yearBox2.SelectedValue);
            moviesFiltered = MovieFilter.Execute(movies, filterArguments.Genres, filterArguments.lowerVote, filterArguments.lowerYear, filterArguments.upperYear);
            MoviePanel.DataContext = moviesFiltered;

        }

        private void yearBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filterArguments.lowerYear = Int32.Parse((string)yearBox1.SelectedValue);
            moviesFiltered = MovieFilter.Execute(movies, filterArguments.Genres, filterArguments.lowerVote, filterArguments.lowerYear, filterArguments.upperYear);
            MoviePanel.DataContext = moviesFiltered;
        }

        private void SortByAlphabet_Checked(object sender, RoutedEventArgs e)
        {
            var ordered = moviesFiltered.OrderBy(n => n.Original_title);
            moviesFiltered = new ObservableCollection<Movie>(ordered);
            MoviePanel.DataContext = moviesFiltered;
        }

        private void SortByRating_Checked(object sender, RoutedEventArgs e)
        {
            var ordered = moviesFiltered.OrderByDescending(n => n.Vote_average);
            moviesFiltered = new ObservableCollection<Movie>(ordered);
            MoviePanel.DataContext = moviesFiltered;
        }

        private void SortByYear_Checked(object sender, RoutedEventArgs e)
        {
            var ordered = moviesFiltered.OrderByDescending(n => n.Year);
            moviesFiltered = new ObservableCollection<Movie>(ordered);
            MoviePanel.DataContext = moviesFiltered;

        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            dbm.RefreshDatabase(movies);
            movies = dbm.GetMovieListFromDB();
            moviesFiltered = MovieFilter.Execute(movies, filterArguments.Genres, filterArguments.lowerVote, filterArguments.lowerYear, filterArguments.upperYear);
            MoviePanel.DataContext = moviesFiltered;
            RefreshDialogBox refDialog = new RefreshDialogBox();
            refDialog.Show();
        }

        private void MoviePanel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string file = GetVideoFile.Get(moviesFiltered[(int)MoviePanel.SelectedIndex].FilePath);

            if (file != null)
            {

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Settings.playerPath;
                psi.Arguments = "\"" + file + "\""; // escape the quotes

                Process.Start(psi);
            }
        }

        private void MoviePanel_KeyDown(object sender, KeyEventArgs e)
        {
            string file = GetVideoFile.Get(moviesFiltered[(int)MoviePanel.SelectedIndex].FilePath);

            if (e.Key == Key.Return)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Settings.playerPath;
                psi.Arguments = "\"" + file + "\""; // escape the quotes

                Process.Start(psi);
            }

        }
    }
}
