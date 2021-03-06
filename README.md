# HttpDo

**HttpDo** is a lightweight package that allows you to host a web interface (website with buttons and inputs) that can execute methods and display data of a connected app.

Despite being lightweight - it comes with sessions, templating engine, error handling, routing, easy authorization and more!

All you need to do to have a fully functional, remotely accessible control panel for your app is:

- create a small website with buttons and inputs for all methods you want to have control of.
- add one line to start service.
- add one attribute per method you want to share with the control panel.
- .. and you are good to go!

## Table of Contents

- [Features](#Features)

- [Getting Started](#Getting-Started)

- [Routing](#Routing)

- [Return Values](#Return-Values)

- [Call with Params](#Call-with-Params)

- [FormData Container](#FormData-Container)

- [Sessions](#Sessions)

- [Redirecting](#Redirecting)

- [Authorization](#Authorization)

- [Custom Authorization](#Custom-Authorization)

- [Templating Engine](#Templating-Engine)

- [To-Do's](#To-Dos)

  

## Features

Here's a list of most (if not all) features contained within the package:

- **Sessions**

  Provides means of transfering data between pages and requests.

- **Templating Engine**

  Allows you to display output of code on pages returned by the server.

- **Error Handling**

  Because HTTP error codes can't explain C# exceptions.

- **Routing**

  Thanks to routing, you don't have to write 'MyVeryLengthyMethodHere' in the browser. 

- **Easy Authorization**

  By default, every request runs through `HasAccess()` and `HasFileAccess()` methods - which can be overriden to fit needs.

- **More..**

  Other features include: `FormData` container, proper navigation via `Response` class and so on..

  

## Getting Started

**Step 1 - Download & Install**

To install the package, simply find it in your Nuget Package Browser then click 'Install'.



.. or click the link below.

[Click](https://www.nuget.org/packages/HttpDo/)


.. or enter this into your package manager console:

`Install-Package HttpDo`

<hr>

**Step 2 - Start Service**

To start the service, simply put the code below somewhere in the program.	

```csharp
var handler = new HttpHandler("http://*:3333", "HttpDo"); // * means catch-all, will bind to all interfaces
```

The first parameter is `rootUrl` and it defines under which URL the connection will be available.

The second parameter is `rootPath` which defines path to folder containing `index.html` and other files that you might be navigating to.



**Hint:** It's highly advised to declare handler as a public property accessible from outside of the scope.

<hr>

**Step 3 - Hook Methods**

To hook method to a route, simply add one of attributes - `[HttpGet]` or `[HttpPost]`.

The first parameter of the attribute is essential - it defines under which URL the method will be available.



Here's an example of hooking method to a route:

```csharp
[HttpGet("print")]
public static void Print() => Console.WriteLine("This is an example message.");
```



If you've hooked it properly, navigating to `http://localhost:3333/print` should pop a message into the console.



.. and there it is:

![1567708441334](README-Images/1567708441334.png)



## Routing

The built-in routing system follows few strict rules:

- POST requests can't lead to files.
- If the URL ends with an extension, routing will look for a file.
- If the URL has no extension, routing will look for a method to call.
- If the URL has no extension and there's no method to call, it'll append `.html` and look for a file.

Follow them and you won't have any issues.



## Return Values

You can return values from both **GET** and **POST** requests, as long as the return value is convertible to string.

String is a prefered type of the HTTP protocol due to xml, json and other string-based formats.

Usually conversion between non-standard types is done via `ToString()`.



Here's an example on how to return data after a request:

```csharp
[HttpGet("getdate")]
public static string GetDate() => DateTime.Now.ToString();
```



Surely enough, the result we get is:

![1567708969851](README-Images/1567708969851.png)



**Note:** HttpDo is api-ready. All you need is asynchronous javascript and you can make the clock refresh in real time.



## Call with Params

The plugin comes with a system for processing and passing request data to methods as parameters.



In order for method to be executed, the default C# rules still apply:

- If param is nullable and has no value set - we good.

- If param has default value and isn't being set - we good.

- If param isn't optional and isn't being set - you'll get one of these red headaches:

  ![1567709354533](README-Images/1567709354533.png)



Now, here's an example on how to call a method hooked to a GET route:

```csharp
[HttpGet("add")]
public static int Add(int a, int b) => a + b;
```



.. and here's the result:

![1567709980713](README-Images/1567709980713.png)

<hr>

Here's an example on how to accomplish the same thing with a POST request:

```csharp
[HttpPost("add")]
public static int Add(int a, int b) => a + b;
```

.. but what we also need an HTML form:

```html
<form action="/add" method="post">
	<input type="number" name="a" value="54"/>
	<input type="number" name="b" value="6"/>
	<input type="submit"/>
</form>
```



.. yet again, here's the result:

![1567710200033](README-Images/1567710200033.png)



## FormData Container

Performing POST requests is easy, but sometimes you want to handle multiple forms that aren't neccessary identical in terms of inputs.
.. and if they aren't identical, the inputs won't match method parameters either.
If that's the case - what you want to use is `FormData` container, which basically is a `Dictionary<string, dynamic>`, where `key` is the input's name and `value` is .. well, input's value.


.. and here's an example:

```csharp
[HttpPost("contact")]
public static void Contact(FormData form) => Console.Write(form["message"]);

// message refers to input of such name - 'message'
```



**Hint:** Using `FormData` doesn't mean that you can't have more parameters, it's just obsolete to have: `FormData form, int someInput` because `someInput` is already accessible via `form["someInput"]`.



## Sessions

The plugin comes with a way to transfer data between pages.

Sessions are used across the entire system, especially the **Templating Engine**.



Storing and retrieving data from sessions is as simple as this:

```csharp
Handler.GetSession()["key"] = value; // assign
Handler.GetSession()["key"]; // retrieve
```



## Redirecting

If your method is meant to redirect back after it's done working it's magic, simply declare it's return type as `Response` and within the constructor define the route you want it to navigate to.



Here's an example:

```csharp
[HttpGet("redirect")]
public static Response Redirector() => new Response("");
```



## Authorization

Some pages and routes shouldn't be publicly available to unauthorized users.

If you haven't noticed yet, the `HttpGet` and `HttpPost` have an extra parameter called `secure`.

The said parameter indicates whether or not requests will execute `HasAccess()` and `HasFileAccess()` before letting the request go through.



To explain this in a pragmatic way, let's say you have a method as such:

```csharp
[HttpGet("securemethod", true)] // true means secure = true
public static string SecureMethod() => "You are here.";
```



By default `HasAccess()` checks if session variable called `is_authorized` was set.

By default `HasFileAccess()` checks if the same session variable is set, but also checks if the file has `.html` extension.



If you try accessing the page without the session variable being set, you'll get this message:

![1567711004607](README-Images/1567711004607.png)



.. now, let's say we go through the method below and back:

```csharp
[HttpGet("authorize")]
public static void Authorize() => Handler.GetSession()["is_authorized"] = true;
```



.. the result would be far different:

![1567711110455](README-Images/1567711110455.png)


**Hint:** You can create a page for authorization with 2 fields: login and password, then make it go to a POST route that executes method which verifies whether login and password were correct - if so, set the session variable "is_authorized" to true - and that's one way to secure your app.


## Custom Authorization

You can define your own access conditions easily.

To do so, simply inherit from the `HttpHandler` class and override the following methods:

- HasAccess
- HasFileAccess



Here's an example:

```csharp
public class HttpService : HttpHandler
{
	public HttpService(string rootUrl, string rootPath) : base(rootUrl, rootPath) { }

	protected override bool HasAccess() => GetSession()["login"];

	protected override bool HasFileAccess(string path) 
    	=> Path.GetExtension(path) == ".html" 
        || Path.GetDirectoryName(path) == "downloadables";
}
```

Easy enough? I should hope so.



## Templating Engine

Templating Engine parses your html files before they go through, replacing `@{{ code }}` with the result of `code`.



It's also important to mention that **it has full access to session** - which should be your primary source of computed data.



Here's an example how to use it:

```html
<html>
	<head>
		<title>Sample Page</title>
	</head>
	<body>
		So, the date now is: @{{ Session["date"] ?? "Dunno.." }}
	</body>
</html>

```



.. and here's the result:

![1567711608511](README-Images/1567711608511.png)



**Remember:** Session in this case is available under `Session` not `Handler.GetSession()`.

## To-Do's

- Implement more unique way of identifying sessions rather than IP address.
