using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Teatteri
{
    public class DataBaseManager
    {
        public int NewMovies { get; set; }

        List<Movie> movieList = new List<Movie>();

        SQLiteConnection mConnection;

        public DataBaseManager()    //create db and tables
        {
            if (File.Exists(@"Movies.sqlite") == true)
            {
                mConnection = new SQLiteConnection(@"Data Source=Movies.sqlite;Version=3;");
            }
            else
            {
                SQLiteConnection.CreateFile(@"Movies.sqlite");

                mConnection = new SQLiteConnection(@"Data Source=Movies.sqlite;Version=3;");
                mConnection.Open();

                string sql = "CREATE TABLE movies(movie_id varchar(15) NOT NULL, original_title varchar(45) NOT NULL, overview varchar(1000), vote_average varchar(5), " +
                "release_date varchar(45), director varchar(45), runtime int, filepath varchar(100), PRIMARY KEY (movie_id))";
                SQLiteCommand mCommand = new SQLiteCommand(sql, mConnection);
                mCommand.ExecuteNonQuery();

                sql = "CREATE TABLE moviegenres(movie_id varchar(15), genre_id varchar(20), PRIMARY KEY (movie_id,genre_id), FOREIGN KEY (movie_id) REFERENCES movies(movie_id))";
                mCommand = new SQLiteCommand(sql, mConnection);
                mCommand.ExecuteNonQuery();

                sql = "CREATE TABLE genrenames(genre_id varchar(20), name varchar(45) , PRIMARY KEY (genre_id), FOREIGN KEY (genre_id) REFERENCES genres(genre_id))";
                mCommand = new SQLiteCommand(sql, mConnection);
                mCommand.ExecuteNonQuery();

                sql = "CREATE TABLE actors(actor_name varchar(45), movie_id varchar(15), PRIMARY KEY (actor_name,movie_id), " +
                    "FOREIGN KEY (movie_id) REFERENCES movies(movie_id))";
                mCommand = new SQLiteCommand(sql, mConnection);
                mCommand.ExecuteNonQuery();

                sql = "CREATE TABLE moviecountries(country varchar(20), movie_id varchar(15), PRIMARY KEY (country,movie_id), " +
                    "FOREIGN KEY (movie_id) REFERENCES movies(movie_id))";
                mCommand = new SQLiteCommand(sql, mConnection);
                mCommand.ExecuteNonQuery();

                InitializeGenres();

                string path = Settings.movieFilePath;

                ReadDirectoriesAndCreateMovieObjects(path);

                foreach (Movie mov in movieList)
                {
                    this.AddMovie(mov);
                }
                foreach (Movie mov in movieList)
                {
                    MovieCreator.GetPoster(mov);
                }

            }
        }

        private void ReadDirectoriesAndCreateMovieObjects(string path)
        {
            //LogWriter dbLog = new LogWriter();

            string[] directories = Directory.GetDirectories(path);

            foreach (string dir in directories)
            {
                if (dir[dir.Length-2].ToString().Equals("C"))
                {
                    //dbLog.Write("Found C!");
                    ReadDirectoriesAndCreateMovieObjects(dir);
                }
                int attempts = 0;

                string regexDir = Regex.Replace(dir, @"\\", @"");
                string regexPath = Regex.Replace(path, @"\\", @"");
                if (path[path.Length - 2].ToString().Equals("C"))
                {
                    regexDir = Regex.Replace(regexDir, @"(C)", "");
                    regexPath = Regex.Replace(regexPath, @"(C)", "");
                }
                string dirName = Regex.Replace(regexDir, regexPath, "");

                //dbLog.Write("dirName of " + dir + " was " + dirName);

                if (dir[dir.Length - 2].ToString() != "C")
                {
                    Movie mov = MovieCreator.Create(dirName);
                    try
                    {
                        {
                            if (mov != null)
                            {
                                mov.FilePath = dir;
                                movieList.Add(mov);
                                //dbLog.Write(mov.Original_title + " succesfully added to movieList.");
                            }
                            else
                            {
                                while (attempts < 5)
                                {
                                    //dbLog.Write(dir + " was null! Attempting again...");
                                    System.Threading.Thread.Sleep(2500);
                                    mov = MovieCreator.Create(dir);
                                    attempts++;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        e.GetType();
                        //dbLog.Write("Error adding " + mov.Original_title + " to movieList. Exception message: " + e.Message);
                    }

                }


            }
            //dbLog.CreateLogFile(@"D:\Tekstitiedostot\Ohjelmointi\Teatteri\dblog.txt");
        }

        private void InitializeGenres()
        {
            if (File.Exists(@"\JSON\genres.json") == false)
            {
                WebClient client = new WebClient();
                System.Uri uri = new System.Uri("https://api.themoviedb.org/3/genre/movie/list?api_key=6c0882a144f6f064e6dd0a67876c9095&language=en-US");
                client.DownloadFile(uri, @"\JSON\genres.json");
            }
            GenreJSON genreJSON = JsonConvert.DeserializeObject<GenreJSON>(File.ReadAllText(@"\JSON\genres.json"));

            foreach (Genre genre in genreJSON.genres)
            {
                SQLiteConnection mConnection = new SQLiteConnection(@"Movies.sqlite;Version=3;");
                mConnection.Open();

                string sql = "INSERT INTO genrenames (genre_id, name) VALUES ('" + genre.id + "', '" + genre.name + "')";
                SQLiteCommand mCommand = new SQLiteCommand(sql, mConnection);
                mCommand.ExecuteNonQuery();
            }

        }

        public ObservableCollection<Movie> GetMovieListFromDB()
        {
            ObservableCollection<Movie> movieList = new ObservableCollection<Movie>();

            mConnection = new SQLiteConnection(@"Data Source=Movies.sqlite;Version=3;");
            mConnection.Open();

            string sql = "SELECT * FROM movies";
            SQLiteCommand mCommand = new SQLiteCommand(sql, mConnection);
            SQLiteDataReader reader = mCommand.ExecuteReader();

            while (reader.Read())
            {
                try
                {
                    movieList.Add(new Movie((string)reader["movie_id"], (string)reader["original_title"], GetMovieGenres((string)reader["movie_id"]), (string)reader["overview"], Convert.ToSingle(reader["vote_average"]),
                     (int)reader["runtime"], GetMovieCountries((string)reader["movie_id"]), (string)reader["release_date"], (string)reader["director"], GetMovieActors((string)reader["movie_id"]), (string)reader["filepath"]));
                }
                catch
                {
                    //joo
                }
            }
            return movieList;
        }

        private List<Genre> GetMovieGenres(string id)
        {
            List<Genre> genres = new List<Genre>();

            string sql = @"SELECT * FROM moviegenres INNER JOIN genrenames ON moviegenres.genre_id = genrenames.genre_id WHERE movie_id = '" + id + "'";
            SQLiteCommand mCommand = new SQLiteCommand(sql, mConnection);
            SQLiteDataReader reader = mCommand.ExecuteReader();
            while (reader.Read())
            {
                genres.Add(new Genre((string)reader["genre_id"], (string)reader["name"]));
            }
            return genres;
        }

        private List<Country> GetMovieCountries(string id)
        {
            List<Country> countries = new List<Country>();

            string sql = @"SELECT * FROM moviecountries WHERE movie_id = '" + id + "'";
            SQLiteCommand mCommand = new SQLiteCommand(sql, mConnection);
            SQLiteDataReader reader = mCommand.ExecuteReader();
            while (reader.Read())
            {
                countries.Add(new Country((string)reader["country"]));
            }
            return countries;
        }

        private List<Person> GetMovieActors(string id)
        {
            List<Person> actors = new List<Person>();

            string sql = @"SELECT * FROM actors WHERE movie_id = '" + id + "'";
            SQLiteCommand mCommand = new SQLiteCommand(sql, mConnection);
            SQLiteDataReader reader = mCommand.ExecuteReader();
            while (reader.Read())
            {
                actors.Add(new Person((string)reader["actor_name"]));
            }
            return actors;
        }

        public void AddMovie(Movie mov)
        {
            FormatMovie(mov);

            try
            {
                SQLiteConnection mConnection = new SQLiteConnection(@"Data Source=Movies.sqlite;Version=3;");
                mConnection.Open();
                string sql = @"INSERT INTO movies (movie_id, original_title, overview, vote_average, release_date, director, runtime, filepath) VALUES ('" + mov.Id + "', '" + mov.Original_title +
                    "', '" + mov.Overview + "', '" + mov.Vote_average.ToString() + "', '" + mov.Release_date + "', '" + mov.Crew[0].name + "', '" + mov.Runtime + "', '" + mov.FilePath + "')";
                SQLiteCommand mCommand = new SQLiteCommand(sql, mConnection);
                mCommand.ExecuteNonQuery();

                foreach (Genre genre in mov.Genres)
                {

                    sql = "INSERT INTO moviegenres (movie_id, genre_id) VALUES ('" + mov.Id + "', '" + genre.id + "')";
                    mCommand = new SQLiteCommand(sql, mConnection);
                    mCommand.ExecuteNonQuery();
                }

                int l;
                if (mov.Cast.Count() >= 4)
                    l = 3;
                else
                    l = mov.Cast.Count();
                for (int i = 0; i < l; i++)
                {
                    string actorname = FormatString(mov.Cast[i].name);
                    sql = "INSERT INTO actors (movie_id, actor_name) VALUES ('" + mov.Id + "', '" + actorname + "')";
                    mCommand = new SQLiteCommand(sql, mConnection);
                    mCommand.ExecuteNonQuery();
                }

                foreach (Country country in mov.Production_countries)
                {

                    sql = "INSERT INTO moviecountries (movie_id, country) VALUES ('" + mov.Id + "', '" + country.name + "')";
                    mCommand = new SQLiteCommand(sql, mConnection);
                    mCommand.ExecuteNonQuery();
                }

            }
            catch
            {
                // jaaha
            }

        }

        private string Replace(string str)
        {

            Regex rgx = new Regex(@"\'");
            str = rgx.Replace(str, "\'\'");
            return str;
        }

        public string FormatString(string str)
        {
            if (str != null)
            {
                str = Replace(str);
            }
            return str;

        }

        public void FormatMovie(Movie mov)
        {
            mov.Id = FormatString(mov.Id);
            mov.Original_title = FormatString(mov.Original_title);
            mov.Overview = FormatString(mov.Overview);
            mov.Release_date = FormatString(mov.Release_date);
            mov.Director = FormatString(mov.Director);
        }
        public void RefreshDatabase(ObservableCollection<Movie> movieCollection)
        {
            string[] searchDir = Directory.GetDirectories(Settings.movieFilePath);
            List<string> newDirs = new List<string>();
            List<Movie> newMovies = new List<Movie>();

            foreach (string dir in searchDir)
            {
                string regexPath = Regex.Replace(Settings.movieFilePath, @"\\", @"");
                string regexDir = Regex.Replace(dir, @"\\", @"");
                string dirName = Regex.Replace(regexDir, regexPath, "");
                bool addMovie = true;
                foreach (Movie mov in movieCollection)
                {
                    if (mov.FilePath.Equals(dir))
                    {
                        addMovie = false;
                    }
                }
                if (addMovie)
                {
                    Movie mov = MovieCreator.Create(dirName);
                    if (mov != null)
                    {
                        mov.FilePath = dir;
                        newMovies.Add(mov);
                    }
                }
            }
            foreach (Movie mov in newMovies)    //Add movie objects to db.
            {
                if (mov!=null)
                {
                    AddMovie(mov);
                    MovieCreator.GetPoster(mov);
                }
            }
            foreach (Movie mov in newMovies)    //Add movie objects to ObservableCollection.
            {
                movieCollection.Add(mov);
            }
            StaticVariables.NewMovies = newMovies.Count();  
        }
    }
}
