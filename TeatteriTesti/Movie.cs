using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data.SQLite;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;



namespace Teatteri
{
    public class Movie
    {
        public string Id { get; set; }
        public string Original_title { get; set; }
        public List<Genre> Genres { get; set; }
        public string Overview { get; set; }
        public float Vote_average { get; set; }
        public string Runtime { get; set; }
        public List<Country> Production_countries { get; set; }
        public string Release_date { get; set; }
        public string Director { get; set; }
        public int Year { get; set; }
        public string Poster_path { get; set; }
        public string PosterFilePath { get; set; }
        public string FilePath { get; set; }

        public Credits credits = new Credits();
        public List<Person> Cast
        {
            get { return credits.cast; }
            set { credits.cast = value; }
        }
        public List<Person> Crew
        {
            get { return credits.crew; }
            set { credits.crew = value; }
        }

        public string GenreString { get; set; }
        public string CountryString { get; set; }
        public string ActorString { get; set; }
        public string Stars { get; set; }

       
        public Movie(string id, string original_title, List<Genre> genres, string overview, float vote_average, int runtime, List<Country> production_countries, string release_date,
            string director, List<Person> cast, string filePath)
        {
            Id = id;
            Year = Convert.ToInt16(release_date.Substring(0, 4));
            Original_title = original_title;
            Genres = genres;
            Overview = overview;
            Vote_average = vote_average;
            Runtime = runtime.ToString()+" min";
            Production_countries = production_countries;
            Release_date = release_date;
            Director = "Director: "+director;
            Cast = cast;
            PosterFilePath = @"D:\Tekstitiedostot\Ohjelmointi\Teatteri\Posters\" + Id + ".jpg";
            FilePath = filePath;
            List<string> genrenames = new List<string>();

            foreach (Genre genre in genres)
            {
                genrenames.Add(genre.name);
            }
            GenreString = String.Join(", ", genrenames);
            List<string> countrynames = new List<string>();
            foreach (Country country in production_countries)
            {
                countrynames.Add(country.name);
            }
            CountryString = String.Join(", ", countrynames);
            List<string> actors = new List<string>();
            if (cast != null)
            {
                foreach (Person actor in cast)
                {
                    actors.Add(actor.name);
                }
                ActorString = "Cast: " + String.Join(", ", actors);
            }

            Stars = GetStars(vote_average);
        }
        private string GetStars(float vote)
        {
            string starchar = Char.ConvertFromUtf32(9733);
            string stars = "";
            if (vote < 0.5)
                stars = "0";
            else if (vote < 1)
                stars = "½";
            else if (vote >= 1 && vote < 2.5)
                stars = starchar;
            else if (vote >= 2.5 && vote < 3.5)
                stars =  starchar+"½";
            else if (vote >= 3.5 && vote < 4.5)
                stars = starchar+starchar;
            else if (vote >= 4.5 && vote < 5.5)
                stars = starchar + starchar+"½";
            else if (vote >= 5.5 && vote < 6.5)
                stars = starchar + starchar + starchar;
            else if (vote >= 6.5 && vote < 7.5)
                stars = starchar + starchar + starchar+"½";
            else if (vote >= 7.5 && vote < 8.5)
                stars = starchar+ starchar + starchar + starchar;
            else if (vote >= 8.5 && vote < 9.5)
                stars = starchar + starchar + starchar + starchar+"½";
            else if (vote >= 9.5)
                stars = starchar + starchar + starchar + starchar+starchar;
            return stars;               
        }
    }

    public class Credits
    {
        public List<Person> cast;
        public List<Person> crew;
    }

    public class Identifier
    {
        public object[] results;
        public string id;
    }

    public class Genre
    {
        public string id;
        public string name;
        public Genre(string id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }

    public class Person
    {
        public int id;
        public string name;
        public Person(string name)
        {
            this.name = name;
        }
    }

    public class Country
    {
        public string name;
        public Country(string name)
        {
            this.name = name;
        }
    }

    public class GenreJSON
    {
        public List<Genre> genres = new List<Genre>();
    }
    
    public static class MovieCreator
    {      

        public static Movie Create(string movieName)
        {
            try
            {
                Movie mov = JSONToMov(JSONSearch(movieName).id);
                mov = GetCredits(mov);
                
                return mov;
            }
            catch (Exception e)
            {
                string message = e.Message;
                return null;
            }
            
        }

        private static Identifier JSONSearch(string movieName)
        {
            try
            {
                WebClient client = new WebClient();
                System.Uri uri = new System.Uri("https://api.themoviedb.org/3/search/movie?api_key=6c0882a144f6f064e6dd0a67876c9095&query=" + movieName + "&page=1");
                client.DownloadFile(uri, @"D:\Tekstitiedostot\Ohjelmointi\Teatteri\JSON\temp.json");
                Identifier identifier = JsonConvert.DeserializeObject<Identifier>(File.ReadAllText(@"D:\Tekstitiedostot\Ohjelmointi\Teatteri\JSON\temp.json"));
                Identifier identifier2 = JsonConvert.DeserializeObject<Identifier>(identifier.results[0].ToString());
                return identifier2;
            }
            catch (Exception e)
            {
                string yy = e.Message;
                return null;
            }
        }

        private static Movie JSONToMov(string movieId)
        {
            Movie mov;
            WebClient client = new WebClient();
            System.Uri uri = new System.Uri("https://api.themoviedb.org/3/movie/" + movieId + "?api_key=6c0882a144f6f064e6dd0a67876c9095&language=en-US");
            client.DownloadFile(uri, @"D:\Tekstitiedostot\Ohjelmointi\Teatteri\JSON\temp.json");
            mov = JsonConvert.DeserializeObject<Movie>(File.ReadAllText(@"D:\Tekstitiedostot\Ohjelmointi\Teatteri\JSON\temp.json"));
            if (mov.Overview == null || mov.Overview == "")
            {
                mov.Overview = "NOT AVAILABLE";
            }
            return mov;
        }

        private static Movie GetCredits(Movie mov)
        {
            WebClient client = new WebClient();
            System.Uri uri = new System.Uri("https://api.themoviedb.org/3/movie/" + mov.Id + "/credits?api_key=6c0882a144f6f064e6dd0a67876c9095");
            client.DownloadFile(uri, @"D:\Tekstitiedostot\Ohjelmointi\Teatteri\JSON\temp.json");
            mov.credits = JsonConvert.DeserializeObject<Credits>(File.ReadAllText(@"D:\Tekstitiedostot\Ohjelmointi\Teatteri\JSON\temp.json"));
            return mov;
        }
        public static void GetPoster(Movie mov)
        {
            if (mov.PosterFilePath == null)
                mov.PosterFilePath = @"D:\Tekstitiedostot\Ohjelmointi\Teatteri\Posters\noposter.jpg";
            else
            {
                if (File.Exists(mov.PosterFilePath) == false)
                {
                    WebClient client = new WebClient();
                    System.Uri uri = new System.Uri(@"http://image.tmdb.org/t/p/w342" + mov.Poster_path);
                    string a = uri.ToString();
                    client.DownloadFile(uri, mov.PosterFilePath);
                }
            }
        }
    }
}
