using System;

namespace CityHall.Exceptions
{
    public class UrlMissingOrInvalidException : Exception
    {
        public UrlMissingOrInvalidException(string url)
            : base(string.Format("CityHall could not be found at given url: {0}", url))
        { }
    }
}
