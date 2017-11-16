using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwaAttachmentServer.CreateMessage.Request
{
    public class CreateMessageRequest
    {
        public string __type { get; set; } = "CreateItemJsonRequest:#Exchange";
        public Header Header { get; set; }
        public Body Body { get; set; }

        public CreateMessageRequest(string email)
        {
            Header = new Header();
            Body = new Body(email);
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
        public string __type { get; set; } = "CreateItemRequest:#Exchange";
        public Item[] Items { get; set; }
        public bool ClientSupportsIrm { get; set; } = true;
        public string OutboundCharset { get; set; } = "AutoDetect";
        public string MessageDisposition { get; set; } = "SaveOnly";
        public string ComposeOperation { get; set; } = "newMail";

        public Body(string email)
        {
            Items = new Item[]
            {
                new Item(email)
            };
        }
    }

    public class Item
    {
        public string __type { get; set; } = "Message:#Exchange";
        public From From { get; set; }

        public Item(string email)
        {
            From = new From(email);
        }
    }

    public class From
    {
        public string __type { get; set; } = "SingleRecipientType:#Exchange";
        public Mailbox Mailbox { get; set; }

        public From(string email)
        {
            Mailbox = new Mailbox
            {
                EmailAddress = email
            };
        }
    }

    public class Mailbox
    {
        public string __type { get; set; } = "EmailAddress:#Exchange";
        public string MailboxType { get; set; } = "Mailbox";
        public string RoutingType { get; set; } = "SMTP";
        public string EmailAddress { get; set; }
    }

}
