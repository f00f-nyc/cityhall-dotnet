using System;

namespace CityHall.Exceptions
{
    public class ErrorFromCityHallException : Exception
    {
        public ErrorFromCityHallException(string message)
            : base(message)
        { }
    }
}
