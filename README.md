# HttpDo

**HttpDo** is a robust yet easy to install and use nuget package.

It provides you with means neccessary to create a remotely accessible app.



Despite being lightweight, it also comes with sessions, templating engine, error handling, routing, easy authorization and more!



Below you'll find a list of topics that'll put you up to speed with the plugin.

**Table of Contents:**



## Features

Here's a list of most (if not all) features contained within the package:

- **Sessions**

  Provides means of transfering data between pages and requests.

- **Templating Engine**

  Allows you to display output of code on webpages returned by the server.

- **Error Handling**

  HttpDo comes with its' own error page. A little bit more precise and detailed.

- **Routing**

  Thanks to routing, you don't have to write 'MyVeryLengthyMethodHere' in the browser. 

- **Easy Authorization**

  By default, every request runs through `HasAccess()` and `HasFileAccess()` methods - which can be overriden to fit needs.

- **More..**

  Other features include: formdata bundling, proper navigation via `Response` class and so on..

  

## Getting Started

**Step 1 - Download & Install**

To install the package, simply find it in your Nuget Package Browser then click 'Install'.



.. or click the link below.

[link](link)



.. or paste the lines below to your Package Manager Console.

`code`



**Step 2 - Start Service**

To start the service, simply put the code below somewhere in the program where you find it fitting.	

```csharp
var handler = new HttpHandler("http://localhost:3333", "HttpDo");
```

The first parameter is `rootUrl` and it defines under which URL the connection will be available.

The second parameter is `rootPath` which defines path to folder containing `index.html` and other files that you might be navigating to.