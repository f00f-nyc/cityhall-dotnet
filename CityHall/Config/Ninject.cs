using Ninject;
using RestSharp;

namespace CityHall.Config
{
    public class Ninject
    {
        private class Initialized
        {
            public bool IsIncomplete = true;
        }
        private static Initialized Initialization = new Initialized();
        public static IKernel Kernel { get; set; }

        public static void RegisterIoc()
        {
            lock (Ninject.Initialization)
            {    
                if (Ninject.Initialization.IsIncomplete)
                {
                    Ninject.Kernel = new StandardKernel();
                    Ninject.Kernel.Bind<IRestClient>().To<RestClient>();
                    Ninject.Initialization.IsIncomplete = false;
                }
            }
        }
    }
}
