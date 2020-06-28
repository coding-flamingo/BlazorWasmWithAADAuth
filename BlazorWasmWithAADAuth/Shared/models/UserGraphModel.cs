using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorWasmWithAADAuth.Shared.models
{
    public class UserGraphModel
    {
        public string UserPrincipalName { get; set; }
        public string Id { get; set; }
    }

    public class UserListGraphModel
    {
        public List<UserGraphModel> value { get; set; }
    }
}
