using System;

namespace CityHall.Exceptions
{
    public class NotLoggedInException : Exception
    {
        public NotLoggedInException()
            : base(@"Have been logged out of City Hall.  Must instantiate a new Settings in order to log back in.")
        { }
    }
}
