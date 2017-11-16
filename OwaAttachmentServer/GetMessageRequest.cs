using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwaAttachmentServer.GetMessage
{
    public class GetMessageRequest
    {
        public string __type { get; set; } = "GetItemJsonRequest:#Exchange";
        public Header Header { get; set; }
        public Body Body { get; set; }

        public GetMessageRequest(string id, string changeKey, string canary)
        {
            Header = new Header();
            Body = new Body(id, changeKey, canary);
        }
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
        public string __type { get; set; } = "GetItemRequest:#Exchange";
        public Itemshape ItemShape { get; set; }
        public Itemid[] ItemIds { get; set; }
        public string ShapeName { get; set; } = "ItemNormalizedBody";

        public Body(string id, string changeKey, string canary)
        {
            ItemShape = new Itemshape
            {
                InlineImageUrlTemplate = $"service.svc/s/GetFileAttachment?id={{id}}&X-OWA-CANARY={canary}"
            };

            ItemIds = new[]
            {
                new Itemid()
                {
                    ChangeKey = changeKey,
                    Id = id
                }
            };
        }
    }

    public class Itemshape
    {
        public string __type { get; set; } = "ItemResponseShape:#Exchange";
        public string BaseShape { get; set; } = "IdOnly";
        public bool ClientSupportsIrm { get; set; } = true;
        public bool FilterHtmlContent { get; set; } = true;
        public bool ShouldUseNarrowGapForPTagHtmlToTextConversion { get; set; } = true;
        public string BodyType { get; set; } = "HTML";
        public string InlineImageUrlTemplate { get; set; }
    }

    public class Itemid
    {
        public string __type { get; set; } = "ItemId:#Exchange";

        public string Id { get; set; }
        public string ChangeKey { get; set; }
    }

}
