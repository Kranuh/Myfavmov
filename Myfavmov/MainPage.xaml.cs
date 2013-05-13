using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Myfavmov.Resources;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.ComponentModel;
using Microsoft.Phone.Tasks;

namespace Myfavmov
{
    public partial class MainPage : PhoneApplicationPage
    {

        private string resp = "";
        private Movie currentMovie;
        private List<Movie> movList;
        private SuggestionLister suggestionMovies;
        private Movie toShareMovie;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        private void searchForMovie(string movieSearch)
        {

            //compile request URL
            string reqUrl = "http://www.omdbapi.com/?t=" + movieSearch;
            HttpWebRequest wRequest = (HttpWebRequest)HttpWebRequest.Create(reqUrl);
                progressBarSearch.Visibility = System.Windows.Visibility.Visible;
                progressBarSearch.IsEnabled = true;
                progressBarSearch.IsIndeterminate = true;
            wRequest.BeginGetResponse(GetMovieCallBackSingle, wRequest);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            this.searchForMovie(searchTb.Text);

        }

        private void GetMovieCallBackSingle(IAsyncResult res)
        {
            

            HttpWebRequest request = res.AsyncState as HttpWebRequest;
            if (request != null)
            {
                
                try
                {
                    WebResponse response = request.EndGetResponse(res);
                    StreamReader rdr = new StreamReader(response.GetResponseStream());
                    resp = rdr.ReadToEnd();
                    rdr.Close();
                    byte[] data = Encoding.UTF8.GetBytes(resp);
                    MemoryStream memStream = new MemoryStream(data);
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Movie));
                    currentMovie = (Movie)serializer.ReadObject(memStream);
                    initWithMovie();
                    Dispatcher.BeginInvoke(() =>
                    {
                        progressBarSearch.Visibility = System.Windows.Visibility.Collapsed;
                        progressBarSearch.IsEnabled = false;
                    });
                    
                }
                catch (WebException e)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("No internet connection found.");
                        progressBarSearch.Visibility = System.Windows.Visibility.Collapsed;
                        progressBarSearch.IsEnabled = false;
                    });

                }

                
            }
        }

        private void initWithMovie()
        {

                //Check if the movie is found
                if (currentMovie.Response == "False")
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Sorry, we couldn't find a movie matching this title. Try being more specific!");
                    });

                    return;

                }

                Dispatcher.BeginInvoke(() =>
                        {
                            titleLb.Text = currentMovie.Title;
                            titleLb.Visibility = System.Windows.Visibility.Visible;
                            plotLb.Text = currentMovie.Plot;
                            plotLb.Visibility = System.Windows.Visibility.Visible;


                            if (currentMovie.Poster != "N/A")
                            {
                                Uri ur = new Uri(currentMovie.Poster);
                                imageHolder.Source = new BitmapImage(ur);
                                imageHolder.Visibility = System.Windows.Visibility.Visible;
                            }
                            

                            ratingLb.Text = currentMovie.imdbRating;
                            ratingLb.Visibility = System.Windows.Visibility.Visible;

                            yearLb.Text = currentMovie.Year;
                            yearLb.Visibility = System.Windows.Visibility.Visible;

                            suggestionLink.Visibility = System.Windows.Visibility.Visible;
                        });
            
            
            
        }

        private void suggestionLink_Click_1(object sender, RoutedEventArgs e)
        {
            if (currentMovie == null)
            {
                MessageBox.Show("You haven't searched for a movie yet.");
                return;
            }

            string reqUrl = "http://www.omdbapi.com/?s=" + searchTb.Text;
            HttpWebRequest wRequest = (HttpWebRequest)HttpWebRequest.Create(reqUrl);
            wRequest.BeginGetResponse(callBackMovieList, wRequest);

        }

        private void popupSuggestions()
        {
            Dispatcher.BeginInvoke(() =>
                    {
                        List<MovieResults> suggestionMoviesMorfed = new List<MovieResults>();
                        foreach (MovieResults m in suggestionMovies.Search)
                        {
                            if (m.Title.Length > 34)
                            {
                                m.Title.Insert(33, System.Environment.NewLine);
                            }

                            suggestionMoviesMorfed.Add(m);
                        }
                        //show the list picker
                        suggestionGrid.Visibility = System.Windows.Visibility.Visible;
                        movielist.ItemsSource = suggestionMoviesMorfed;
                        
                    });
        }


        private void callBackMovieList(IAsyncResult res)
        {
            HttpWebRequest request = res.AsyncState as HttpWebRequest;
            if (request != null)
            {

                try
                {
                    WebResponse response = request.EndGetResponse(res);
                    StreamReader rdr = new StreamReader(response.GetResponseStream());
                    resp = rdr.ReadToEnd();
                    rdr.Close();

                    byte[] data = Encoding.UTF8.GetBytes(resp);
                    MemoryStream memStream = new MemoryStream(data);
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SuggestionLister));
                    suggestionMovies = (SuggestionLister)serializer.ReadObject(memStream);
                    popupSuggestions();
                    


                }
                catch (WebException e)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("No internet connection found.");
                    });

                }


            }
        }

        private void createSuggestionList(List<Movie> movieList)
        {
            
        }
        private void searchForMovieByID(string imdbID)
        {
            progressBarSearch.Value = 1;
            //compile request URL
            string reqUrl = "http://www.omdbapi.com/?i=" + imdbID;
            progressBarSearch.Value = 2;
            HttpWebRequest wRequest = (HttpWebRequest)HttpWebRequest.Create(reqUrl);
            progressBarSearch.Value = 20;
            wRequest.BeginGetResponse(GetMovieCallBackSingle, wRequest);
        }

        private void movielist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Object[] selectedMovie = (Object[])e.AddedItems;
            suggestionGrid.Visibility = System.Windows.Visibility.Collapsed;
            movielist.ItemsSource = null;
            suggestionMovies = null;
            MovieResults selected = (MovieResults)selectedMovie[0];
            this.searchForMovieByID(selected.imdbID);


        }

        private void rateButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentMovie == null)
            {
                MessageBox.Show("You haven't searched for a movie yet.");
            }
            else
            {
                favPopup.Visibility = System.Windows.Visibility.Visible;
                favPopup.IsOpen = true;


            }
        }

        private void favButton_Click(object sender, RoutedEventArgs e)
        {
            //save the selected movie to user data
            if (rateSlider.Value == 0)
            {
                MessageBox.Show("You have not rated the movie yet!");
            }
            else
            {
                //add the rating
                currentMovie.personalRating = (int)Math.Round(rateSlider.Value);

                //create the moviesaver
                MovieSaver msv = new MovieSaver("myfavmov");

                //get the favorites
                List<Movie> favMovies = msv.load();
                //if favorites is null make a new list if not add the movie to the list
                if (favMovies == null)
                {
                    favMovies = new List<Movie>();
                    favMovies.Add(currentMovie);
                }
                else
                {
                    //Check wether the list already contains the movie
                    foreach (Movie m in favMovies)
                    {
                        if (m.Title == currentMovie.Title && m.Runtime == currentMovie.Runtime)
                        {
                            MessageBox.Show("You already have this movie in your favorites.");
                            favPopup.IsOpen = false;
                            return;
                        }

                        if (m.Title.Length > 20)
                        {
                            string newTitle = m.Title.Substring(0, 19);
                            m.shortTitle = newTitle + "...";
                        }

                    }

                    favMovies.Add(currentMovie);
                }

                
                msv.save(favMovies);
            }

            favPopup.IsOpen = false;
        }

        private void rateSlider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int sliderVal = (int) Math.Round(rateSlider.Value);
            rateTxt.Text = Convert.ToString(sliderVal) + "/10";
        }


        private void pivotMyMovs_Loaded(object sender, RoutedEventArgs e)
        {
            MovieSaver msv = new MovieSaver("myfavmov");
            try
            {
                //cut long movie titles from file and sort sort
                movList = msv.load();
                List<Movie> cutMovList = new List<Movie>();
                foreach(Movie m in movList)
                {
                    if (m.Title.Length > 20)
                    {
                        string newTitle = m.Title.Substring(0, 19);
                        if (m.personalRating == 0)
                        {
                            m.shortTitle = newTitle + "[W]";
                        }
                        else
                        {
                            m.shortTitle = newTitle + "...";
                        }
                    }
                    else if (m.personalRating == 0)
                    {
                        m.shortTitle = m.Title + "[W]";
                    }
                    
                    cutMovList.Add(m);
                }
                cutMovList.Sort();
                myMoviesList.ItemsSource = cutMovList;
            }
            catch (NullReferenceException)
            {
            }

        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainPivot.SelectedIndex == 0)
            {
                MovieSaver msv = new MovieSaver("myfavmov");
                try
                {
                    movList = msv.load();
                    List<Movie> cutMovList = new List<Movie>();
                    foreach (Movie m in movList)
                    {
                        if (m.Title.Length > 20)
                        {
                            string newTitle = m.Title.Substring(0, 19);
                            if (m.personalRating == 0)
                            {
                                m.shortTitle = newTitle + "[W]";
                            }
                            else
                            {
                                m.shortTitle = newTitle + "...";
                            }
                        }
                        else if (m.personalRating == 0)
                        {
                            m.shortTitle = m.Title + "[W]";
                        }
                        
                        cutMovList.Add(m);
                    }

                    cutMovList.Sort();

                    myMoviesList.ItemsSource = cutMovList;
                }
                catch (NullReferenceException)
                {
                }
            }


            
        }

        private void closeSuggestionsBut_Click(object sender, RoutedEventArgs e)
        {
            suggestionGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void myMoviesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Movie selectedItem = e.AddedItems[0] as Movie;
            mainPivot.SelectedIndex = 2;
            initMovieDisplay(selectedItem);
            
            
        }

        private void initMovieDisplay(Movie m)
        {
            if (m.Poster != "N/A")
            {
                Uri url = new Uri(m.Poster);
                posterHolder.Source = new BitmapImage(url);
            }
            if (m.Title.Length > 15)
            {
                movieTitleHolder.FontSize = 15;
            }
            movieTitleHolder.Text = m.Title;
            jaarHolder.Text = m.Year;
            runtimeHolder.Text = m.Runtime;
            if (m.personalRating == 0)
            {
                pratingHolder.Text = "WATCHLIST";
                rButton.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                pratingHolder.Text = Convert.ToString(m.personalRating) + "/10";
                rButton.Visibility = System.Windows.Visibility.Collapsed;
            }

            imdbRatingHolder.Text = m.imdbRating;
            directorHolder.Text = m.Director;
            plotHolder.Text = m.Plot;
            movieGrid.Visibility = System.Windows.Visibility.Visible;
            toShareMovie = m;

        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            e.Cancel = true;
            bool somethingClosed = false;
            if (favPopup.IsOpen == true)
            {
                favPopup.Visibility = System.Windows.Visibility.Collapsed;
                favPopup.IsOpen = false;
                somethingClosed = true;
            }
            if (suggestionGrid.Visibility == System.Windows.Visibility.Visible)
            {
                suggestionGrid.Visibility = System.Windows.Visibility.Collapsed;
                somethingClosed = true;
            }

            if (favPopup2.IsOpen == true)
            {
                favPopup2.Visibility = System.Windows.Visibility.Collapsed;
                favPopup2.IsOpen = false;
                somethingClosed = true;
            }


            if (somethingClosed == false)
            {
                e.Cancel = false;
            }

        }

        private void shareButton_Click(object sender, RoutedEventArgs e)
        {
            ShareLinkTask share = new ShareLinkTask();
            share.Title = "Share!";
            share.LinkUri = new Uri("http://www.imdb.com/title/" + toShareMovie.imdbID);
            if (toShareMovie.personalRating == 0)
            {
                share.Message = "I've just added: " + toShareMovie.Title + " to my watchlist, using: MyFavMov!";
            }
            else
            {
                share.Message = "I rated: " + toShareMovie.Title + " with a " + toShareMovie.personalRating + " out of 10! With MyFavMov!";
            }
            share.Show();
        }

        private void wButton_Click(object sender, RoutedEventArgs e)
        {
            if (rateSlider.Value != 0)
            {
                MessageBox.Show("You can't rate the movie if you are going to add it to the watchlist!");
                rateSlider.Value = 0;
            }
            else
            {
                currentMovie.watchList = true;

                //create the moviesaver
                MovieSaver msv = new MovieSaver("myfavmov");

                //get the favorites
                List<Movie> favMovies = msv.load();
                //if favorites is null make a new list if not add the movie to the list
                if (favMovies == null)
                {
                    favMovies = new List<Movie>();
                    favMovies.Add(currentMovie);
                }
                else
                {
                    //Check wether the list already contains the movie
                    foreach (Movie m in favMovies)
                    {
                        if (m.Title == currentMovie.Title && m.Runtime == currentMovie.Runtime)
                        {
                            MessageBox.Show("You already have this movie in your favorites.");
                            favPopup.IsOpen = false;
                            return;
                        }

                        if (m.Title.Length > 20)
                        {
                            string newTitle = m.Title.Substring(0, 19);
                            m.shortTitle = newTitle + "...";
                        }

                    }

                    currentMovie.personalRating = 0;
                    favMovies.Add(currentMovie);
                }

                msv.save(favMovies);
                favPopup.IsOpen = false;
            }
        }

        private void rButton_Click(object sender, RoutedEventArgs e)
        {
            favPopup2.IsOpen = true;
        }

        private void rateSlider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int sliderVal = (int)Math.Round(rateSlider2.Value);
            rateTxt1.Text = Convert.ToString(sliderVal) + "/10";

        }

        private void favButton2_Click(object sender, RoutedEventArgs e)
        {
            toShareMovie.watchList = false;
            toShareMovie.personalRating = (int)Math.Round(rateSlider2.Value);
            MovieSaver msv = new MovieSaver("myfavmov");
            List<Movie> movielist = msv.load();
            Movie movietoDelete = new Movie();
            Movie newMovie = toShareMovie;
            foreach(Movie m in movielist)
            {
                if(m.Title == toShareMovie.Title && m.imdbID == toShareMovie.imdbID)
                {
                    movietoDelete = m;
                }
            }

            
            movielist.Remove(movietoDelete);
            movielist.Add(toShareMovie);
            toShareMovie.shortTitle = toShareMovie.Title;
            msv.save(movielist);
            favPopup2.IsOpen = false;
        }

        private void myMoviesList_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Movie selectedMovietoDelete = sender as Movie;
            
            MessageBox.Show(selectedMovietoDelete.imdbID);
        }

    }
}