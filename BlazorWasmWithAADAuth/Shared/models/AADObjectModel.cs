using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorWasmWithAADAuth.Shared.models
{
    public class AADObjectModel
    {
        public AADObjectModel()
        {
        }

        public AADObjectModel(UserGraphModel userGraph)
        {
            ObjectId = userGraph.Id;
            FriendlyName = userGraph.UserPrincipalName;
            isValid = true;
            ObjectType = "USER";
        }

        public AADObjectModel(GroupGraphModel groupGraph)
        {
            ObjectId = groupGraph.Id;
            FriendlyName = groupGraph.DisplayName;
            isValid = true;
            ObjectType = "GROUP";
        }
        public string ObjectId { get; set; }
        public string FriendlyName { get; set; }
        public string ObjectType { get; set; }
        public bool isValid { get; set; } = false;// group, user, SP

    }
}
