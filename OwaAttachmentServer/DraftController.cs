using Microsoft.Exchange.WebServices.Data;
using System.Configuration;
using System.Diagnostics;
using System.Web.Http;

namespace OwaAttachmentServer
{
    public class DraftController : ApiController
    {        
        public class LoginModel
        {
            public string email { get; set; }

            public string password { get; set; }
        }

        [Route("login")]
        [HttpPost]
        public IHttpActionResult Login(LoginModel model)
        {
            return Ok(ExchangeServiceProvider.CreateProvider(model.email, model.password));
        }

        [Route("create")]
        [HttpGet]
        public IHttpActionResult Create()
        {
            ExchangeServiceProvider.CreateMessage();         

            return Ok("OK");
        }

        [HttpGet]
        public IHttpActionResult Logined()
        {
            return Ok(ExchangeServiceProvider.Service != null);
        }

        [HttpGet]
        public IHttpActionResult Message()
        {
            return Ok(ExchangeServiceProvider.Message != null);
        }

        [HttpGet]
        public IHttpActionResult Logout()
        {
            ExchangeServiceProvider.Logout();

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult Home()
        {
            var exchangeUrl = ExchangeServiceProvider.Url;

            var returnUrl = $"{exchangeUrl}/owa";

            return Ok(returnUrl);
        }

        [Route("open")]
        [HttpGet]
        public IHttpActionResult Open()
        {
            var propSet = new PropertySet();

            propSet.Add(ItemSchema.Id);
            propSet.Add(ItemSchema.WebClientReadFormQueryString);
            propSet.Add(ItemSchema.WebClientEditFormQueryString);

            try
            {
                var item = Item.Bind(ExchangeServiceProvider.Service, ExchangeServiceProvider.Message.Id, propSet);

                var exchangeUrl = ExchangeServiceProvider.Url;

                var returnUrl = $"{exchangeUrl}/owa{item.WebClientEditFormQueryString}";

                return Ok(returnUrl);
            }
            catch (System.Exception)
            {
                return Ok();
            }
        }
    }
}
