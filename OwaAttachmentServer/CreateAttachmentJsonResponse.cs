using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwaAttachmentServer.CreateAttachment
{
    public class CreateAttachmentJsonResponse : IEwsResponse
    {
        public Header Header { get; set; }
        public Body Body { get; set; }


        public string GetErrorCode()
        {
            var responseItems = Body.ResponseMessages.Items;

            if (responseItems != null && responseItems.Any(p => p.ResponseClass == "Error"))
            {
                var responseItem = responseItems.FirstOrDefault(p => p.ResponseClass == "Error");

                if (responseItem != null)
                {
                    return responseItem.ResponseCode;
                }
            }

            return null;
        }
    }


    public class Rootobject
    {
        public Header Header { get; set; }
        public Body Body { get; set; }
    }

    public class Header
    {
        public Serverversioninfo ServerVersionInfo { get; set; }
    }

    public class Serverversioninfo
    {
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public int MajorBuildNumber { get; set; }
        public int MinorBuildNumber { get; set; }
        public string Version { get; set; }
    }

    public class Body
    {
        public Responsemessages ResponseMessages { get; set; }
    }

    public class Responsemessages
    {
        public Item[] Items { get; set; }
    }

    public class Item
    {
        public string __type { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseClass { get; set; }
        public Attachment[] Attachments { get; set; }
    }

    public class Attachment
    {
        public string __type { get; set; }
        public Attachmentid AttachmentId { get; set; }
    }

    public class Attachmentid
    {
        public string RootItemChangeKey { get; set; }
        public string RootItemId { get; set; }
        public string Id { get; set; }
    }    
}
