This is the .Net/C# library for City Hall Enterprise Settings Server

 ABOUT

 This project is written in C# 4.5 and can be installed using:
   Install-Package CityHall 



 USAGE

 The intention is to use the built-in City Hall web site for actual
 settings management, and then use this library for consuming those
 settings, in an application.  As such, there is really only command 
 to be familiar with:

 string value = await CityHall.Settings.Get("/some_app/value1/");

 In order for this to work, you must set up the .config file:

   <configSections>
      <section name="CityHall" type="CityHall.Config.CityHallConfigSection, CityHall" />
   </configSections>
   <CityHall url="http://path.to.city.hall.server/api/" />

 The currently machine running the code should have a user with an 
 empty password and a default environment already set up on that server.



 ADVANCED USAGE

 The library covers all of the initial API in both synchronous (found
 in the CityHall.Synchronous.SyncSettings class) and asynchronous modes.

 Sample usage:
 
 var asyncSettings = await Settings.New(url, user, password); 
     This must be called to initiate a session with City Hall. The 
     password should be in plaintext, it will be hashed by the library.
 
 await asyncSettings.Logout(); 
     To be called when the session is over. Strictly speaking, this is
     not necessary, sessions time out on their own.
 
 string value = await asyncSettings.Values.Get("/some_app/value1", environment: "dev", over: "cityhall");
     The fully qualified way to access a values. Again, for most
     use cases, maintaining your own ISettings instance shouldn't be
     necessary, as you'll mostly want to use the code to retrieve 
     values.

In case you need synchronous operations, the entire API is duplicated:

 using CityHall.Synchronous;
 var syncSettings = SyncSettings.New(url, user, password);
 string value = syncSettings.Values.Get("/some_app/value1");
 
You may also switch between them by using:

  var syncSettings = asyncSettings.SynchronousSettings;
 
 For more in depth information about this library, please check the wiki.


 LICENSE

  City Hall source files are made available under the terms of the GNU Affero General Public License (AGPL).
