# Packages

In this section we will learn the most important parts of arranging and asserting our web application components.

## The building blocks of the testing framework

Of course, as a main building block of the ASP.NET Core MVC framework, we will start with controllers. Before we begin, let's make a step backwards. Remember the **"project.json"** and the referenced **"MyTested.AspNetCore.Mvc"** dependency? Good! The My Tested ASP.NET Core MVC framework consists of many packages. Here are the most important ones:

 - **"MyTested.AspNetCore.Mvc.Core"** - Contains setup and assertion methods for MVC core features - controllers, models and routes
 - **"MyTested.AspNetCore.Mvc.DataAnnotations"** - Contains setup and assertion methods for data annotation validations and model state
 - **"MyTested.AspNetCore.Mvc.ViewFeatures"** - Contains setup and assertion methods for view action results, view data, view components and more
 - **"MyTested.AspNetCore.Mvc"** - Contains assertion methods for the full MVC framework 
 
From now on the "MyTested.AspNetCore.Mvc." package name prefix will be skipped for brevity.
 
The packages above reflect the features in the ASP.NET Core packages - "Microsoft.AspNetCore.Mvc.Core", "Microsoft.AspNetCore.Mvc.DataAnnotations", "Microsoft.AspNetCore.Mvc.ViewFeatures" and "Microsoft.AspNetCore.Mvc".

These so called "main" packages can be separated further to **"Controllers"**, **"Routing"**, **"ViewActionResults"**, **"ViewComponents"** and more. Helper packages for testing non-MVC related features like **"EntityFrameworkCore"** and **"Authentication"** are also available.

Additionally, these two packages are also available:

 - **"MyTested.AspNetCore.Mvc.Universe"** - Combines all available binaries and the whole fluent API into one single package. Use it, if you do not want to include specific smaller packages and want to have every feature available.
 - **"MyTested.AspNetCore.Mvc.Lite"** - Completely **FREE** and **UNLIMITED** version of the library. It should not be used in combination with any other package. Includes ""*Controllers"**, `ViewActionResults"** and `ViewComponents"**.

The full list and descriptions of all available packages can be found [HERE](/guide/packages.html). All of them except the **"Lite"** one require a license code in order to be used without limitations. If a license code is not provided, a maximum of 100 assertions per test project is allowed. More information about the licensing can be found [HERE](/guide/licensing.html).

## Breaking down the MVC package

Now, let's get back to the testing. Go to the **"project.json"** file and replace the **"MyTested.AspNetCore.Mvc"** dependency with **"MyTested.AspNetCore.Mvc.Controllers"**. We will start using the small and specific packages for now, and then we will switch to the **"Universe"** one later in the tutorial.

Your **"project.json"** dependencies should look like this:

```json
"dependencies": {
  "dotnet-test-xunit": "2.2.0-*",
  "xunit": "2.2.0-*",
  "MyTested.AspNetCore.Mvc.Controllers": "1.0.0",
  "MusicStore": "*"
},
``` 

If you try to build the solution, you will receive an error stating that **"MyMvc"** does not exist.

<img src="/images/tutorial/mymvcdoesnotexist.jpg" alt="MyMvc no longer exists" />

The **"MyMvc"** class is only available in the **"MyTested.AspNetCore.Mvc"** package and combines all the different test types into a single starting point. To fix our test, we have to use the **"MyController"**:

```c#
[Fact]
public void AddressAndPaymentShouldReturnDefaultView()
    => MyController<CheckoutController>
        .Instance()
        .Calling(c => c.AddressAndPayment())
        .ShouldReturn()
        .View();
```

Unfortunately, it still does not compile because the **"Controllers"** package contains assertions methods only for the [ControllerBase](https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNetCore.Mvc.Core/ControllerBase.cs) class which does not include view features and action results.

You can see this by examining the IntelliSense of the test:

<img src="/images/tutorial/coreintellisense.jpg" alt="Controllers package IntelliSense" />

## Asserting core controllers

We will try the core action results before returning to the view features. Comment the first test for now (so that the project will compile with the currently added dependencies) and add a new class named **"ManageControllerTest"** in the **"Controllers"** folder. We will test the asynchronous **"RemoveLogin"** action in the **"ManageController"**. If you examine it, you will notice that it returns **"RedirectToAction"**, if no user is authenticated:

```c#
public async Task<IActionResult> RemoveLogin(string loginProvider, string providerKey)
{
	ManageMessageId? message = ManageMessageId.Error;
	var user = await GetCurrentUserAsync();
	if (user != null)
	{
		// action code skipped for brevity
	}
	return RedirectToAction("ManageLogins", new { Message = message });
}
```

Add the necessary usings and write the following test into the **"ManageControllerTest"** class:

```c#
[Fact]
public void RemoveLoginShouldReturnRedirectToActionWithNoUser()
    => MyController<ManageController>
        .Instance()
        .Calling(c => c.RemoveLogin(null, null))
        .ShouldReturn()
        .Redirect();
```

Run the test, and it should pass correctly. As you can see, My Tested ASP.NET Core MVC tests asynchronous actions flawlessly. If you do not like the null value literals, you may use the built-in helper class **"With"** to specify that there are no action arguments in this test:

```c#
.Calling(c => c.RemoveLogin(With.No<string>(), With.No<string>()))
```

As a bonus, let's assert some details of the redirect action result. We can see it redirects to the **"ManageLogins"** action with some **"ManageMessageId"** route value so we better test them:

```c#
[Fact]
public void RemoveLoginShouldReturnRedirectToActionWithNoUser()
    => MyController<ManageController>
        .Instance()
        .Calling(c => c.RemoveLogin(
            With.No<string>(),
            With.No<string>()))
        .ShouldReturn()
        .Redirect()
        .ToAction(nameof(ManageController.ManageLogins))
        .ContainingRouteValues(new { Message = ManageController.ManageMessageId.Error });
```

Now we can sleep peacefully! :)

## Adding view action results

OK, back to that commented test. We cannot test views with our current dependencies. Go to the **"project.json"** and add **"MyTested.AspNetCore.Mvc.ViewActionResults"** as a dependency:

```json
"dependencies": {
  "dotnet-test-xunit": "2.2.0-*",
  "xunit": "2.2.0-*",
  "MyTested.AspNetCore.Mvc.Controllers": "1.0.0",
  "MyTested.AspNetCore.Mvc.ViewActionResults": "1.0.0", // <---
  "MusicStore": "*"
},
``` 

This package adds all action results from the [Controller](https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNetCore.Mvc.ViewFeatures/Controller.cs) class from the [ViewFeatures](https://github.com/aspnet/Mvc/tree/dev/src/Microsoft.AspNetCore.Mvc.ViewFeatures) MVC package. Go back to the **"CheckoutControllerTest"** class and uncomment the view test. It should compile and pass successfully now.

## Section summary

In this section we learned how we can use only these parts from My Tested ASP.NET Core MVC that we actually need in our testing project. Each small package dependency adds additional extension methods to the fluent API. We will add more and more packages in the next sections so that you can get familiar with them. Next - [Debugging Failed Tests](/tutorial/debugging.html)!