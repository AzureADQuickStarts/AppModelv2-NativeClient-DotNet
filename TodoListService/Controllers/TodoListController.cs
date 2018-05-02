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
using System.Net;
using System.Net.Http;
using System.Web.Http;

// The following using statements were added for this sample.
using System.Collections.Concurrent;
using TodoListService.Models;
using System.Security.Claims;

namespace TodoListService.Controllers
{
    [Authorize]
    public class TodoListController : ApiController
    {
        //
        // To Do items list for all users.  Since the list is stored in memory, it will go away if the service is cycled.
        //
        static ConcurrentBag<TodoItem> todoBag = new ConcurrentBag<TodoItem>();

        private ClaimsIdentity userClaims;

        public TodoListController()
        {
            userClaims = User.Identity as ClaimsIdentity;
        }

        /// <summary>
        /// Assure the presence of a scope claim containing a specific scope (i.e. access_as_user)
        /// </summary>
        /// <param name="scopeName">The name of the scope</param>
        private void CheckAccessTokenScope(string scopeName)
        {
            // Make sure access_as_user scope is present
            string scopeClaimValue = userClaims.FindFirst("http://schemas.microsoft.com/identity/claims/scope")?.Value;
            if (!string.Equals(scopeClaimValue, scopeName, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = @"Please request an access token to scope '{scopeName}'"
                });
            }
        }

        // GET api/todolist
        public IEnumerable<TodoItem> Get()
        {
            CheckAccessTokenScope("access_as_user");
            
            // You can use the ClaimsPrincipal to access information about the
            // user making the call.  In this case, we use the 'sub' or
            // NameIdentifier claim to serve as a key for the tasks in the data store.
            // the NameIdentififier claim contains an immutable, unique identifier for the use
            
            Claim subject = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier);

            return from todo in todoBag
                   where todo.Owner == subject.Value
                   select todo;
        }

        // POST api/todolist
        public void Post(TodoItem todo)
        {
            CheckAccessTokenScope("access_as_user");

            if (null != todo && !string.IsNullOrWhiteSpace(todo.Title))
            {
                todoBag.Add(new TodoItem { Title = todo.Title, Owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value });
            }
        }
    }
}
