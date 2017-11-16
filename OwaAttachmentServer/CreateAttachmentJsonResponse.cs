using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwaAttachmentServer.Response
{
    public class CreateAttachmentJsonResponse : IEwsRequest
    {
        public ResponseHeader Header { get; set; }
        public ResponseBody Body { get; set; }
        

        public string GetErrorCode()
        {
            var responseItems = Body.ResponseMessages.Items;

            if (responseItems != null && responseItems.Any(p => p.ResponseClass == "Error"))
            {
                var responseItem = responseItems.FirstOrDefault(p => p.ResponseClass == "Error");

                if(responseItem != null)
                {
                    return responseItem.ResponseCode;
                }
            }

            return null;
        }
    }

    public class ResponseHeader
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

    public class ResponseBody
    {
        public ResponseMessages ResponseMessages { get; set; }
    }

    public class ResponseMessages
    {
        public ResponseItem[] Items { get; set; }
    }

    public class ResponseItem
    {
        public string __type { get; set; }
        public string MessageText { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseClass { get; set; }
        public object[] Attachments { get; set; }
    }

}
