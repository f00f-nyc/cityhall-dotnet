This is the .Net/C# library for City Hall Enterprise Settings Server

 ABOUT

 This project is written in C# 4.5 and can be installed using:
   Install-Package CityHall 



 USAGE

 The intention is to use the built-in City Hall web site for actual
 settings management, and then use this library for consuming those
 settings, in an application.  As such, there are only a few commands to 
 be familiar with:

 using CityHall;
 
 var asyncSettings = Settings(url, user, password); - Must be called to 
     initiate a session with City Hall. The password should be in 
     plaintext, it will be hashed by the library.
 
 asyncSettings.Logout(); - To be called when the session is over.
 
 asyncSettings.get()- This should be the way to retrieve a value.
     To get the value of '/some_app/value1', use:
        await asyncSettings.get("/some_app/value1");

In case you need synchronous operations, the entire API is duplicated:

 using CityHall.Synchronous;
 var syncSettings = Settings(url, user, password);
 syncSettings.get("/some_app/value1");
 
You may also switch between them by using:

  var syncSettings = asyncSettings.GetSynchronousSettings();
 
 For more in depth information about this library, please check the wiki.


 LICENSE

  City Hall source files are made available under the terms of the GNU Affero General Public License (AGPL).
