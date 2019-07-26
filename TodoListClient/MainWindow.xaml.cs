//----------------------------------------------------------------------------------------------
//    Copyright 2014 Microsoft Corporation
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//----------------------------------------------------------------------------------------------

// The following using statements were added for this sample.
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;

namespace TodoListClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        // The Client ID is used by the application to uniquely identify itself to the Microsoft identity platform endpoint
        // The AAD Instance is the instance of the identity platform endpoint
        // The Redirect URI is the URI where the identity platform endpoint will return OAuth responses.
        // The Authority is the sign-in URL.
        
        
        private string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        
        // The todoListServiceBaseAddress is the address of your Web API
        private string todoListServiceBaseAddress = ConfigurationManager.AppSettings["TodoListServiceBaseAddress"];
        private string todoListServiceScope = ConfigurationManager.AppSettings["TodoListServiceScope"];


        private HttpClient httpClient = new HttpClient();
        private IPublicClientApplication app = null;

        private string[] Scopes = null;
        
        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Scopes = new string[] {todoListServiceScope};

            // Initialize the PublicClientApplication
            app = PublicClientApplicationBuilder.Create(clientId)
                .Build();
				
			TokenCacheHelper.EnableSerialization(app.UserTokenCache);


            // TODO: Check if the user is already signed in. 
            // As the app starts, we want to check to see if the user is already signed in.
            // You can do so by trying to get a token from MSAL, using the method
            // AcquireTokenSilent.  This forces MSAL to throw an exception if it cannot
            // get a token for the user without showing a UI.
            try
            {
                var accounts = await app.GetAccountsAsync();
                var result = await app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
			                      .ExecuteAsync();
                // If we got here, a valid token is in the cache - or MSAL was able to get a new oen via refresh token.
                // Proceed to fetch the user's tasks from the TodoListService via the GetTodoList() method.
                
                SignInButton.Content = "Clear Cache";
                GetTodoList();
            }
            catch (MsalUiRequiredException)
            {
                // The app should take no action and simply show the user the sign in button.
            }
            catch (MsalException ex)
            {
                if (ex.ErrorCode == "failed_to_acquire_token_silently")
                {
                    // If user interaction is required, the app should take no action,
                    // and simply show the user the sign in button.
                }
                else
                {
                    // Here, we catch all other MsalExceptions
                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Inner Exception : " + ex.InnerException.Message;
                    }
                    MessageBox.Show(message);
                }
            }
        }
        

        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task GetTodoList()
        {

            // Get a token from MSAL, and attach
            // it to the GET request in the Authorization
            // header.

            AuthenticationResult result = null;
            try
            {
                // Here, we try to get an access token to call the TodoListService
                // without invoking any UI prompt.  AcquireTokenSilentAsync forces
                // MSAL to throw an exception if it cannot get a token silently.
                var accounts = await app.GetAccountsAsync();
                result = await app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
            catch (MsalException ex)
            {
                // MSAL couldn't get a token silently, so show the user a message
                // and let them click the Sign-In button.

                if (ex.ErrorCode == "failed_to_acquire_token_silently")
                {
                    MessageBox.Show("Please sign in first");
                    SignInButton.Content = "Sign In";
                }
                else
                {
                    // In any other case, an unexpected error occurred.

                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Inner Exception : " + ex.InnerException.Message;
                    }
                    MessageBox.Show(message);
                }

                return;
            }

            // Once the token has been returned by MSAL,
            // add it to the http authorization header,
            // before making the call to access the To Do list service.

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            // Call the To Do list service.
            HttpResponseMessage response = await httpClient.GetAsync(todoListServiceBaseAddress + "/api/todolist");

            if (response.IsSuccessStatusCode)
            {
                string s = await response.Content.ReadAsStringAsync();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                List<TodoItem> toDoArray = serializer.Deserialize<List<TodoItem>>(s);
                TodoList.ItemsSource = toDoArray.Select(t => new { t.Title });
            }
            else
            {
                MessageBox.Show("An error occurred : " + response.ReasonPhrase);
            }

            return;
        }

        private async void AddTodoItem(object sender, RoutedEventArgs e)
        {
            // This method follows the same pattern as GetTodoList()

            if (string.IsNullOrEmpty(TodoText.Text))
            {
                MessageBox.Show("Please enter a value for the To Do item name");
                return;
            }

            AuthenticationResult result = null;
            try
            {
                var accounts = await app.GetAccountsAsync();
                result = await app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
            catch (MsalException ex)
            {
                if (ex.ErrorCode == "failed_to_acquire_token_silently")
                {
                    MessageBox.Show("Please sign in first");
                    SignInButton.Content = "Sign In";
                }
                else
                {
                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Inner Exception : " + ex.InnerException.Message;
                    }

                    MessageBox.Show(message);
                }

                return;
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Title", TodoText.Text) });
            HttpResponseMessage response = await httpClient.PostAsync(todoListServiceBaseAddress + "/api/todolist", content);

            if (response.IsSuccessStatusCode)
            {
                TodoText.Text = "";
                GetTodoList();
            }
            else
            {
                MessageBox.Show("An error occurred : " + response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        /// <param name="app"></param>
        private async Task ClearCache(IPublicClientApplication app)
        {
            var accounts = (await app.GetAccountsAsync()).ToList();
            while (accounts.Any())
            {
                await app.RemoveAsync(accounts.First());
                accounts = (await app.GetAccountsAsync()).ToList();
            }
        }
        private async void SignIn(object sender = null, RoutedEventArgs args = null)
        {
            // TODO: Sign the user out if they clicked the "Clear Cache" button

            // If the user clicked the 'clear cache' button,
            // clear the MSAL token cache and show the user as signed out.
            // It's also necessary to clear the cookies from the browser
            // control so the next user has a chance to sign in.

            if (SignInButton.Content.ToString() == "Clear Cache")
            {
                TodoList.ItemsSource = string.Empty;
                await ClearCache(app);
                SignInButton.Content = "Sign In";
                return;
            }

            // If the user clicked the 'Sign-In' button, force
            // MSAL to prompt the user for credentials by using
            // AcquireTokenAsync, a method that is guaranteed to show a prompt to the user.
            // MSAL will get a token for the TodoListService and cache it for you.

            AuthenticationResult result = null;
            var accounts = await app.GetAccountsAsync();
            try
            {
                result = await app.AcquireTokenInteractive(Scopes)
                    .WithAccount(accounts.FirstOrDefault())
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
                Dispatcher.Invoke(() =>
                {
                    SignInButton.Content = "Clear Cache";
                });
                GetTodoList();
            }
            catch (MsalException ex)
            {
                // If MSAL cannot get a token, it will throw an exception.
                // If the user canceled the login, it will result in the
                // error code 'authentication_canceled'.

                if (ex.ErrorCode == "authentication_canceled")
                {
                    MessageBox.Show("Sign in was canceled by the user");
                }
                else
                {
                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Inner Exception : " + ex.InnerException.Message;
                    }

                    MessageBox.Show(message);
                }

                return;
            }


            // TODO: Invoke UI & get a token if the user clicked "Sign In"

        }
    }
}
