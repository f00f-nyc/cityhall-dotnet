using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncSettings = CityHall.Synchronous.ISettings;

namespace CityHall
{
    public interface ISettings
    {
        SyncSettings SynchronousSettings();
    }
}
