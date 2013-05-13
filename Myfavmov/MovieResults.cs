using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Myfavmov
{
    [DataContract]
    class MovieResults
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Year { get; set; }
        [DataMember]
        public string imdbID { get; set; }

        
        public override string ToString()
        {
            string retString = this.Title + " " + Year;
            if (retString.Length > 20)
            {
                retString = this.Title + System.Environment.NewLine + Year;
            }
            return retString;
        }

        public string ToStringForSearch()
        {
            return Title;
        }

    }
}
