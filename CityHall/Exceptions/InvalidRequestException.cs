using System;

namespace CityHall.Exceptions
{
    public class InvalidRequestException : Exception
    {
        public InvalidRequestException(string format, params object[] args)
            : base(string.Format(format, args))
        { }
    }
}
