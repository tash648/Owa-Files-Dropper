using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwaAttachmentServer.FindItemRequest
{
    public class FindItemRequest
    {
        public string __type { get; set; } = "FindItemJsonRequest:#Exchange";
        public Header Header { get; set; } = new Header();
        public Body Body { get; set; } = new Body();
    }

    public class Header
    {
        public string __type { get; set; } = "JsonRequestHeaders:#Exchange";
        public string RequestServerVersion { get; set; } = "Exchange2013";
        public Timezonecontext TimeZoneContext { get; set; }

        public Header()
        {
            TimeZoneContext = new Timezonecontext
            {
                TimeZoneDefinition = new Timezonedefinition
                {
                    Id = "UTC"
                }
            };
        }
    }

    public class Timezonecontext
    {
        public string __type { get; set; } = "TimeZoneContext:#Exchange";
        public Timezonedefinition TimeZoneDefinition { get; set; }
    }

    public class Timezonedefinition
    {
        public string __type { get; set; } = "TimeZoneDefinitionType:#Exchange";
        public string Id { get; set; }
    }

    public class Body
    {
        public string __type { get; set; } = "FindItemRequest:#Exchange";
        public Itemshape ItemShape { get; set; } = new Itemshape();
        public Parentfolderid[] ParentFolderIds { get; set; } = new[]
        {
            new Parentfolderid()
        };

        public string Traversal { get; set; } = "Shallow";
    }

    public class Itemshape
    {
        public string __type { get; set; } = "ItemResponseShape:#Exchange";
        public string BaseShape { get; set; } = "IdOnly";
    }

    public class Parentfolderid
    {
        public string __type { get; set; } = "DistinguishedFolderId:#Exchange";
        public string Id { get; set; } = "drafts";
    }

}
