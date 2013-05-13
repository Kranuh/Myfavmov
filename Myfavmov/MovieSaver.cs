using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Xml;

namespace Myfavmov
{
    class MovieSaver
    {
        private string filename;

        /// <summary>
        /// Initiates a Moviesaver
        /// </summary>
        /// <param name="filename">The filename of the saved file, or the file that has to be loaded</param>
        public MovieSaver(string filename)
        {
            this.filename = filename;
        }

        public void save(List<Movie> movies)
        {
            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream fs = storage.CreateFile("favmov.wp"))
                    {
                        using (StreamWriter writer = new StreamWriter(fs))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(List<Movie>));
                            serializer.Serialize(writer, movies);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                throw ex;
            }
        }

        public List<Movie> load()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storage.FileExists("favmov.wp"))
                {
                    try
                    {
                        using (IsolatedStorageFileStream fs = storage.OpenFile("favmov.wp", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            XDocument doc = XDocument.Load(fs);
                            XmlSerializer serializer = new XmlSerializer(typeof(List<Movie>));
                            return (List<Movie>)serializer.Deserialize(doc.CreateReader());
                        }
                    }
                    catch (IsolatedStorageException)
                    {
                        return null;
                    }
                    catch (XmlException)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            
        }
    }
}
