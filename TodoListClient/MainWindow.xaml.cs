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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// The following using statements were added for this sample.
using System.Globalization;
using Microsoft.Experimental.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using System.Runtime.InteropServices;
using System.Configuration;
using Microsoft.Experimental.IdentityModel.Clients.ActiveDirectory;

namespace TodoListClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        // The Client ID is used by the application to uniquely identify itself to the v2.0 endpoint
        // The AAD Instance is the instance of the v2.0 endpoint
        // The Redirect URI is the URI where the v2.0 endpoint will return OAuth responses.
        // The Authority is the sign-in URL.
        
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        Uri redirectUri = new Uri(ConfigurationManager.AppSettings["ida:RedirectUri"]);
        private static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, "common");
        private static string todoListBaseAddress = ConfigurationManager.AppSettings["todo:TodoListBaseAddress"];

        private HttpClient httpClient = new HttpClient();
        private AuthenticationContext authContext = null;

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            authContext = new AuthenticationContext(authority, new FileCache());
            AuthenticationResult result = null;

            // As the app starts, we want to check to see if the user is already signed in.
            // You can do so by trying to get a token from ADAL, passing in the parameter
            // PromptBehavior.Never.  This forces ADAL to throw an exception if it cannot
            // get a token for the user without showing a UI.
   
            try
            {
                result = await authContext.AcquireTokenAsync(new string[] { clientId }, null, clientId, redirectUri, new PlatformParameters(PromptBehavior.Never, null));

                // If we got here, a valid token is in the cache.  Proceed to 
                // fetch the user's tasks from the TodoListService via the 
                // GetTodoList() method.

                SignInButton.Content = "Clear Cache";
                GetTodoList();
            }
            catch (AdalException ex)
            {
                if (ex.ErrorCode == "user_interaction_required")
                {
                    // If user interaction is required, the app should take no action,
                    // and simply show the user the sign in button.
                }
                else
                {
                    // Here, we catch all other AdalExceptions

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

        private async void GetTodoList()
        {
            AuthenticationResult result = null;
            try
            {
                // Here, we try to get an access token to call the TodoListService 
                // without invoking any UI prompt.  PromptBehavior.Never forces
                // ADAL to throw an exception if it cannot get a token silently.

                result = await authContext.AcquireTokenAsync(new string[] { clientId }, null, clientId, redirectUri, new PlatformParameters(PromptBehavior.Never, null));
            }
            catch (AdalException ex)
            {
                // ADAL couldn't get a token silently, so show the user a message
                // and let them click the Sign-In button.

                if (ex.ErrorCode == "user_interaction_required")
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

            // Once the token has been returned by ADAL, 
            // add it to the http authorization header, 
            // before making the call to access the To Do list service.

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);


            // Call the To Do list service.
            HttpResponseMessage response = await httpClient.GetAsync(todoListBaseAddress + "/api/todolist");

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
                result = await authContext.AcquireTokenAsync(new string[] { clientId }, null, clientId, redirectUri, new PlatformParameters(PromptBehavior.Never, null));
            }
            catch (AdalException ex)
            {
                if (ex.ErrorCode == "user_interaction_required")
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

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);

            HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Title", TodoText.Text) });
            HttpResponseMessage response = await httpClient.PostAsync(todoListBaseAddress + "/api/todolist", content);

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

        private async void SignIn(object sender = null, RoutedEventArgs args = null)
        {
            // If the user clicked the 'clear cache' button,
            // clear the ADAL token cache and show the user as signed out.
            // It's also necessary to clear the cookies from the browser
            // control so the next user has a chance to sign in.

            if (SignInButton.Content.ToString() == "Clear Cache")
            {
                TodoList.ItemsSource = string.Empty;
                authContext.TokenCache.Clear();
                ClearCookies();
                SignInButton.Content = "Sign In";
                return;
            }

            // If the user clicked the 'Sign-In' button, force
            // ADAL to prompt the user for credentials by specifying
            // PromptBehavior.Always.  ADAL will get a token for the 
            // TodoListService and cache it for you.

            AuthenticationResult result = null;
            try
            {
                result = await authContext.AcquireTokenAsync(new string[] { clientId }, null, clientId, redirectUri, new PlatformParameters(PromptBehavior.Always, null));
                SignInButton.Content = "Clear Cache";
                GetTodoList();
            }
            catch (AdalException ex)
            {
                // If ADAL cannot get a token, it will throw an exception.
                // If the user canceled the login, it will result in the 
                // error code 'authentication_canceled'.

                if (ex.ErrorCode == "authentication_canceled")
                {
                    MessageBox.Show("Sign in was canceled by the user");
                }
                else
                {
                    // An unexpected error occurred.
                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Inner Exception : " + ex.InnerException.Message;
                    }

                    MessageBox.Show(message);
                }

                return;
            }

        }

        // This function clears cookies from the browser control used by ADAL.
        private void ClearCookies()
        {
            const int INTERNET_OPTION_END_BROWSER_SESSION = 42;
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_END_BROWSER_SESSION, IntPtr.Zero, 0);
        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);

    }
}
