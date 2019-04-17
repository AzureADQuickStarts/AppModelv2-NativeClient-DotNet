---
services: active-directory
platforms: dotnet
author: jmprieur
level: 200
client: Windows Desktop WPF
service: ASP.NET Web API
endpoint: AAD V2
---

# Calling an ASP.NET Web API protected by the Azure AD V2 endpoint from an Windows Desktop (WPF) application

## About this sample

### Scenario

You expose a Web API and you want to protect it so that only authenticated user can access it. This sample shows how to expose a ASP.NET Web API so it can accept tokens issued by personal accounts (including outlook.com, live.com, and others) as well as work and school accounts from any company or organization that has integrated with Azure Active Directory.

The sample also include a Windows Desktop application (WPF) that demonstrate how you can request an access token to access a Web APIs.

## How to run this sample

> Pre-requisites: This sample requires Visual Studio 2017. If you don't have it, download [Visual Studio 2017 for free](https://www.visualstudio.com/downloads/).

### Step 1: Download or clone this sample

You can clone this sample from your shell or command line:

  ```console
  git clone https://github.com/AzureADQuickStarts/AppModelv2-NativeClient-DotNet.git
  ```

### Step 2: Register your Web API - *TodoListService* in the *Application registration portal*

#### Choose the Azure AD tenant where you want to create your applications

If you want to register your apps manually, as a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory**.
   Change your portal session to the desired Azure AD tenant.

#### Register the service app (TodoListService)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `AppModelv2-NativeClient-DotNet-TodoListService`.
   - Change **Supported account types** to **Accounts in any organizational directory**.
   - Select **Register** to create the application.

1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project (`ClientId` in `TodoListService\Web.config`).
1. Select the **Expose an API** section, and:
   - Select **Add a scope**
   - accept the proposed Application ID URI (api://{clientId}) by selecting **Save and Continue**
   - Enter the following parameters:
     - for **Scope name** use `access_as_user`
     - Ensure the **Admins and users** option is selected for **Who can consent**
     - in **Admin consent display name** type `Access TodoListService as a user`
     - in **Admin consent description** type `Accesses the TodoListService Web API as a user`
     - in **User consent display name** type `Access TodoListService as a user`
     - in **User consent description** type `Accesses the TodoListService Web API as a user`
     - Keep **State** as **Enabled**
     - Select **Add scope**

#### Configure your *TodoListService* and *TodoListClient* projects to match the Web API you just registered

1. Open the solution in Visual Studio and then open the **Web.config** file under the root of **TodoListService** project.
1. Replace the value of `ida:ClientId` parameter with the **Client ID (Application Id)** from the application you just registered in the Application Registration Portal.

#### Add the new scope to the *TodoListClient*`s app.config

1. Open the **app.config** file located in **TodoListClient** project's root folder and then paste **Application Id** from the application you just registered for your *TodoListService* under `TodoListServiceScope` parameter, replacing the string `{Enter the Application Id of your TodoListService from the app registration portal}`.

   > Note: Make sure it uses the following format:
   >
   > `api://{TodoListService-Application-Id}/access_as_user` 
   >
   >(where {TodoListService-Application-Id} is the Guid representing the Application Id for your TodoListService).

### Step 3:  Register the client app (TodoListClient)

In this step, you configure your *TodoListClient* project by registering a new application in the Application registration portal. In the cases where the client and server are considered *the same application* you may also just reuse the same application registered in the 'Step 2.'. Using the same application is actually needed if you want users to sign-in with Microsoft personal accounts

#### Register the *TodoListClient* application in the *Application registration portal*

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `NativeClient-DotNet-TodoListClient`.
   - Change **Supported account types** to **Accounts in any organizational directory**.
   - Select **Register** to create the application.
1. From the app's Overview page, select the **Authentication** section.
   - In the **Redirect URLs** | **Suggested Redirect URLs for public clients (mobile, desktop)** section, check **urn:ietf:wg:oauth:2.0:oob**
   - Select **Save**.
1. Select the **API permissions** section
   - Click the **Add a permission** button and then,
   - Select the **My APIs** tab.
   - In the list of APIs, select the `AppModelv2-NativeClient-DotNet-TodoListService API`, or the name you entered for the Web API.
   - Check the **access_as_user** permission if it's not already checked. Use the search box if necessary.
   - Select the **Add permissions** button

#### Configure your *TodoListClient* project

1. In the *Application registration portal*, in the **Overview** page copy the value of the **Application (client) Id**
1. Open the **app.config** file located in the **TodoListClient** project's root folder and then paste the value in the `ida:ClientId` parameter value

### Step 4: Run your project

1. Press `<F5>` to run your project. Your *TodoListClient* should open.
1. Select **Sign in** in the top right and sign in with the same user you have used to register your application, or a user in the same directory.
1. At this point, if you are signing in for the first time, you may be prompted to consent to *TodoListService* Web Api.
1. The sign-in also request the access token to the *access_as_user* scope to access *TodoListService* Web Api and manipulate the *To-Do* list.

### Step 5: Pre-authorize your client application

One of the ways to allow users from other directories to access your Web API is by *pre-authorizing* the client applications to access your Web API by adding the Application Ids from client applications in the list of *pre-authorized* applications for your Web API. By adding a pre-authorized client, you will not require user to consent to use your Web API. Follow the steps below to pre-authorize your Web Application::

1. Go back to the *Application registration portal* and open the properties of your **TodoListService**.
1. In the **Expose an API** section, click on **Add application** under the *Pre-authorized applications* section.
1. In the *Application ID* field, paste the application ID of the `TodoListClient` application.
1. In the *Scope* field, click on the **Select** combo box and select the scope for this Web API `api://<Application ID>/access_as_user`.
1. Press the **Save** button at the bottom of the page.

### Step 6:  Run your project

1. Press `<F5>` to run your project. Your *TodoListClient* should open.
1. Select **Sign in** in the top right (or Clear Cache/Sign-in) and then sign-in either using a personal Microsoft account (live.com or hotmail.com) or work or school account.

## Optional: Restrict sign-in access to your application

By default, when you download this code sample and configure the application to use the Azure Active Directory v2 endpoint following the preceeding steps, both personal accounts - like outlook.com, live.com, and others - as well as Work or school accounts from any organizations that are integrated with Azure AD can request tokens and access your Web API. 

To restrict who can sign in to your application, use one of the options:

### Option 1: Restrict access to a single organization (single-tenant)

You can restrict sign-in access for your application to only user accounts that are in a single Azure AD tenant - including *guest accounts* of that tenant. This scenario is a common for *line-of-business applications*:

1. In the **web.config** file of your **TodoListService**, change the value for the `Tenant` parameter from `Common` to the tenant name of the organization, such as `contoso.onmicrosoft.com` or the *Tenant Id*.
2. Open **App_Start\Startup.Auth** file and set the `ValidateIssuer` argument to `true`.

#### Option 2: Use a custom method to validate issuers

You can implement a custom method to validate issuers by using the **IssuerValidator** parameter. For more information about how to use this parameter, read about the [TokenValidationParameters class](https://msdn.microsoft.com/library/system.identitymodel.tokens.tokenvalidationparameters.aspx) on MSDN.
