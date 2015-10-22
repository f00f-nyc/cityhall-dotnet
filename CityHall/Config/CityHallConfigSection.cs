using System;
using System.Configuration;

namespace CityHall.Config
{

    public class CityHallConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("url", IsRequired = true)]
        public String Url 
        {
            get { return (String)this["url"]; }
            set { this["url"] = value; } 
        }

        [ConfigurationProperty("user")]
        public String User
        {
            get { return (String)this["user"]; }
            set { this["user"] = value; }
        }

        [ConfigurationProperty("password")]
        public String Password
        {
            get { return (String)this["password"]; }
            set { this["password"] = value; }
        }
    }
}
