using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Data.SQLite;

namespace Teatteri
{
    public class FilterArguments
    {
        public List<string> Genres { get; set; }
        public string Country { get; set; }
        public int lowerVote { get; set; }
        public int lowerYear { get; set; }
        public int upperYear { get; set; }


        public FilterArguments()
        {
            Genres = new List<string>();
            SQLiteConnection mConnection = new SQLiteConnection(@"Data Source=D:\Tekstitiedostot\Ohjelmointi\Teatteri\Movies.sqlite;Version=3;");
            mConnection.Open();
            string sql = @"SELECT name FROM genrenames";
            SQLiteCommand mCommand = new SQLiteCommand(sql, mConnection);
            SQLiteDataReader reader = mCommand.ExecuteReader();
            while (reader.Read())
            {
                this.Genres.Add((string)reader["name"]);
            }
            this.lowerVote = 0;
            mConnection.Close();
        }
    }

    public static class MovieFilter
    {
        
        private static ObservableCollection<Movie> ByGenre(ObservableCollection<Movie> movielist, List<string> genres)
        {
            LogWriter listLog = new LogWriter();
            ObservableCollection<Movie> filteredList = new ObservableCollection<Movie>();

            foreach (Movie mov in movielist)
            {
                bool genrematch = false;
                foreach (Genre genre in mov.Genres)
                {
                    foreach (string filtergenre in genres)
                    {
                        if (filtergenre == genre.name && filtergenre != null)
                        {
                            genrematch = true;
                            break;
                        }
                    }
                    if (genrematch == true)
                        break;
                }
                if (genrematch == true)
                {
                    filteredList.Add(mov);
                    continue;
                }
            }

            return filteredList;
        }
        private static ObservableCollection<Movie> ByVote(ObservableCollection<Movie> movielist, float lower)
        {
            ObservableCollection<Movie> filteredList = new ObservableCollection<Movie>();
            foreach (Movie mov in movielist)
            {
                if ((mov.Vote_average/2) >= lower)
                    filteredList.Add(mov);
            }
            return filteredList;
        }

        private static ObservableCollection<Movie> ByYear(ObservableCollection<Movie> movielist, int lower, int upper)
        {
            ObservableCollection<Movie> filteredList = new ObservableCollection<Movie>();
            var query =
                from movie in movielist
                where movie.Year >= lower && movie.Year <= upper
                select movie;
            foreach (Movie movie in query)
                filteredList.Add(movie);
            return filteredList;
        }

        private static ObservableCollection<Movie> ByCountry(ObservableCollection<Movie> movielist, string[] countries)
        {
            ObservableCollection<Movie> filteredList = new ObservableCollection<Movie>();
            foreach (Movie mov in movielist)
            {
                bool match = false;
                foreach (Country country in mov.Production_countries)
                {
                    foreach (string filtercountry in countries)
                    {
                        if (filtercountry == country.name)
                        {
                            match = true;
                            break;
                        }
                    }
                    if (match == true)
                        break;
                }
                if (match == true)
                {
                    filteredList.Add(mov);
                    continue;
                }
            }
            return filteredList;
        }

        public static ObservableCollection<Movie> Execute(ObservableCollection<Movie> movielist, List<string> genres, float lowerVote = 0, int lowerYear = 0, int upperYear = 30000, string[] country = null)
        {
            ObservableCollection<Movie> filteredList = ByGenre(movielist, genres);
            if (lowerVote > 0)
                filteredList = ByVote(filteredList, lowerVote);
            filteredList = ByYear(filteredList, lowerYear, upperYear);
            if (country != null)
                filteredList = ByCountry(filteredList, country);
            
            return filteredList;
        }
        
    }
}
