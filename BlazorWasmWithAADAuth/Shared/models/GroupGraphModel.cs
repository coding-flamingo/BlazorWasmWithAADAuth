using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorWasmWithAADAuth.Shared.models
{
    public class GroupGraphModel
    {
        public string DisplayName { get; set; }
        public string Id { get; set; }
    }

    public class GroupListGraphModel
    {
        public List<GroupGraphModel> value { get; set; }
    }
}
