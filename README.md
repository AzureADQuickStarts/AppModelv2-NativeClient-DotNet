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

> Pre-requisites: This sample requires Visual Studio 2015 Update 3 or Visual Studio 2017. Don’t have it? Download [Visual Studio 2017 for free](https://www.visualstudio.com/downloads/).

### Step 1: Download or clone this sample

You can clone this sample from your shell or command line:

  ```console
  git clone https://github.com/AzureADQuickStarts/AppModelv2-NativeClient-DotNet.git
  ```

### Step 2: Register your Web API - *TodoListService* in the *Application registration portal*

1. Sign in to the [Application registration portal](https://apps.dev.microsoft.com/portal/register-app) either using a personal Microsoft account (live.com or hotmail.com) or work or school account.
1. Give a name to your Application, such as `AppModelv2-NativeClient-DotNet-TodoListService`. Make sure that the *Guided Setup* option is **Unchecked** then press **Create**. The portal will assign your app a globally unique *Application ID* that you'll use later in your code.
1. Click **Add Platform**, and select **Web API**
1. Click **Save**

> Note: When you add a *Web API* the Application registration portal, it adds a pre-defined App Id URI and Scope, using the format *api://{Application Id}/{Scope Name}* named **access_as_user** (you can review it by clicking 'Edit' button). This sample code uses this default scope.

### Step 3: Configure your *TodoListService* project to match the Web API you just registered

1. Open the solution in Visual Studio and then open the **Web.config** file under the root of **TodoListService** project.
1. Replace the value of `ida:ClientId` parameter with the **Application Id** from the application you just registered in the Application Registration Portal.

#### Step 3.1: Add the new scope to the *TodoListClient*`s app.config

1. Open the **app.config** file located in **TodoListClient** project's root folder and then paste **Application Id** from the application you just registered for your *TodoListService* under `TodoListServiceScope` parameter, replacing the string `{Enter the Application Id of your TodoListService from the app registration portal}`. 

    > Note: Make sure it uses has the format `api://{TodoListService-Application-Id}/access_as_user` (where {TodoListService-Application-Id} is the Guid representing the Application Id for your TodoListService).

### Step 4: Register the *TodoListClient* application in the *Application registration portal*

In this step, you configure your *TodoListClient* project by registering a new application in the Application registration portal. In the cases where the client and server are considered *the same application* you may also just reuse the same application registered in the 'Step 2.'.

1. Go back to [Application registration portal](https://apps.dev.microsoft.com/portal/register-app) to register a new application
1. Give a name to your Application, such as `NativeClient-DotNet-TodoListClient`, make sure that the *Guided Setup* option is **Unchecked** then press **Create**.
1. Click **Add Platform**, and select **Native**.
1. Click **Save**

### Step 5: Configure your *TodoListClient* project

1. In the *Application registration portal*, copy the value of the **Application Id**
1. Open the **app.config** file located in the **TodoListClient** project's root folder and then paste the value in the `ida:ClientId` parameter value

### Step 6: Run your project

1. Press `<F5>` to run your project. Your *TodoListClient* should open.
1. Select **Sign in** in the top right and sign in with the same user you have used to register your aplication, or a user in the same directory.
1. At this point, if you are signing in for the first time, you may be prompted to consent to *TodoListService* Web Api.
1. The sign-in also request the access token to the *access_as_user* scope to access *TodoListService* Web Api and manipulate the *To-Do* list.

### Step 7: Pre-authorize your client application

One of the ways to allow users from other directories to acces your Web API is by *pre-authorizing* the client applications to access your Web API by adding the Application Ids from client applications in the list of *pre-authorized* applications for your Web API. In order to do this:

1. Go back to the *Application registration portal* and open the properties of your **TodoListService**.
1. In the **Web API platform**, click on **Add application** under the *Pre-authorized applications* section.
1. In the *Application ID* field, paste the application ID of the `TodoListClient` application.
1. In the *Scope* field, click on the **Select** combo box and select the scope for this Web API `api://<Application ID>/access_as_user`.
1. Press the **Save** button at the bottom of the page.

### Step 8: Run your project

1. Press `<F5>` to run your project. Your *TodoListClient* should open.
1. Select **Sign in** in the top right (or Clear Cache/Sign-in) and then sign-in either using a personal Microsoft account (live.com or hotmail.com) or work or school account.

## Optional: Restrict sign-in access to your application

By default, when download this code sample and configure the application to use the Azure Active Directory v2 endpoint following the preceeding steps, both personal accounts - like outlook.com, live.com, and others - as well as Work or school accounts from any organizations that are integrated with Azure AD can sign in to your application. This is typically used on SaaS applications.

To restrict who can sign in to your application, use one of the options:

### Option 1: Restrict access to a single organization (single-tenant)

You can restrict sign-in access for your application to only user accounts that are in a single Azure AD tenant - including *guest accounts* of that tenant. This scenario is a common for *line-of-business applications*:

1. In the **web.config** file of your **TodoListService**, change the value for the `Tenant` parameter from `Common` to the tenant name of the organization, such as `contoso.onmicrosoft.com` or the *Tenant Id*.
2. In your [OWIN Startup class](#configure-the-authentication-pipeline), set the `ValidateIssuer` argument to `true`.

### Option 2: Restrict access to a list of known organizations

You can restrict sign-in access to only user accounts that are in an Azure AD organization that is in the list of allowed organizations:

1. In your [OWIN Startup class](#configure-the-authentication-pipeline), set the `ValidateIssuer` argument to `true`.
2. Set the value of the `ValidIssuers` parameter to the list of allowed organizations.

### Option 3: Restrict the categories of users that can sign-in to your application

This scenario is a common for *SaaS* applications that are focused on either consumers or organizations, therefore want to block accepting either personal accounts or work or school accounts.

1. In the **web.config** file of your **TodoListService**, use on of the values below for `Tenant` parameter:

    Value | Description
    ----- | --------
    `common` | Users can sign in with any Work and School account, or Microsoft Personal account
    `organizations` |  Users can sign in with any Work and School account
    `consumers` |  Users can sign in with a Microsoft Personal account

    > Note: the values above are not considered a *tenant*, but a *convention* to restrict certain categories of users

#### Option 4: Use a custom method to validate issuers

You can implement a custom method to validate issuers by using the **IssuerValidator** parameter. For more information about how to use this parameter, read about the [TokenValidationParameters class](https://msdn.microsoft.com/library/system.identitymodel.tokens.tokenvalidationparameters.aspx) on MSDN.
