using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Myfavmov
{
    public class Movie : IComparable
    {
        
        public String Title { get; set; }
        public String Year { get; set; }
        public String Rated { get; set; }
        public String Released { get; set; }
        public String Runtime { get; set; }
        public String Genre { get; set; }
        public String Director { get; set; }
        public String Writer { get; set; }
        public String Actors { get; set; }
        public String Plot { get; set; }
        public String Poster { get; set; }
        public String imdbRating { get; set; }
        public String imdbVotes { get; set; }
        public String imdbID { get; set; }
        public String Response { get; set; }
        public String Error { get; set; }
        public String shortTitle { get; set; }
        public Boolean watchList { get; set; }

        public int personalRating { get; set; }


        public Movie()
        {
            shortTitle = this.Title;
            watchList = false;
        }
        public override string ToString()
        {
            if (shortTitle == null)
            {
                return Title;
            }
            else
            {
                return shortTitle;
            }
        }

        public int CompareTo(object obj)
        {
            Movie mov2 = (Movie)obj;
            if (this.personalRating > mov2.personalRating)
            {
                return -1;
            }
            if (this.personalRating < mov2.personalRating)
            {
                return 1;
            }
            if (this.personalRating == mov2.personalRating)
            {
                return 0;
            }

            return 0;
        }
    }
}
