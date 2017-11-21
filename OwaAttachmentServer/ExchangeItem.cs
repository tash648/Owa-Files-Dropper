using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwaAttachmentServer
{
    public class ExchangeItem
    {
        public string __type { get; set; }
        public Itemid ItemId { get; set; }

        public long Size { get; set; }

        public string Id
        {
            get { return ItemId.Id; }
        }

        public string ChangeKey
        {
            get { return ItemId.ChangeKey; }
            set { ItemId.ChangeKey = value; }
        }
    }

    public class Itemid
    {
        public string ChangeKey { get; set; }
        public string Id { get; set; }
    }
}
