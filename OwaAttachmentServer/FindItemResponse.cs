using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwaAttachmentServer
{
    public class FindItemResponse : IEwsResponse
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
        public object HighlightTerms { get; set; }
        public Rootfolder RootFolder { get; set; }
    }

    public class Rootfolder
    {
        public bool IncludesLastItemInRange { get; set; }
        public int TotalItemsInView { get; set; }
        public object Groups { get; set; }
        public Item1[] Items { get; set; }
    }

    public class Item1
    {
        public string __type { get; set; }
        public Itemid ItemId { get; set; }
    }
}
