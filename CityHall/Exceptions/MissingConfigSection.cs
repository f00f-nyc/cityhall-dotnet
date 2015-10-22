using System;

namespace CityHall.Exceptions
{
    public class MissingConfigSection : Exception
    {
        public MissingConfigSection()
            : base(@"Attempted to find a CityHall.Config.CityHallConfigSection, but it doesn't exist.  
In order to use CityHall, you must either add one to your app.config, or pass the url to the Get() function.")
        { }
    }
}
