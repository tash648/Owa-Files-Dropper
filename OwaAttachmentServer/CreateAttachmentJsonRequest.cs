using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwaAttachmentServer
{
    public class CreateAttachmentJsonRequest
    {
        public string __type { get; set; }
        public Header Header { get; set; }
        public Body Body { get; set; }
    }

    public class Header
    {
        public string __type { get; set; }
        public string RequestServerVersion { get; set; }
        public Timezonecontext TimeZoneContext { get; set; }
    }

    public class Timezonecontext
    {
        public string __type { get; set; }
        public Timezonedefinition TimeZoneDefinition { get; set; }
    }

    public class Timezonedefinition
    {
        public string __type { get; set; }
        public string Id { get; set; }
    }

    public class Body
    {
        public string __type { get; set; }
        public Parentitemid ParentItemId { get; set; }
        public Attachment[] Attachments { get; set; }
        public bool RequireImageType { get; set; }
        public bool IncludeContentIdInResponse { get; set; }
        public bool ClientSupportsIrm { get; set; }
        public object CancellationId { get; set; }
    }

    public class Parentitemid
    {
        public string __type { get; set; }
        public string Id { get; set; }
        public string ChangeKey { get; set; }
    }

    public class Attachment
    {
        public string __type { get; set; }
        public string Content { get; set; }
        public bool IsContactPhoto { get; set; }
        public string ContentType { get; set; }
        public bool IsInline { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
    }

}
