﻿using System.Collections.Generic;
using System.Web.Http;

namespace OwaAttachmentServer
{
    public class DraftController : ApiController
    {        
        public class LoginModel
        {
            public List<NameValue> headers { get; set; }
        }

        public class NameValue
        {
            public string name { get; set; }

            public string value { get; set; }
        }

        [Route("progress")]
        [HttpGet]
        public IHttpActionResult Progress(LoginModel model)
        {
            return Ok(ExchangeServiceProvider.IsInProgress());
        }

        [Route("login")]
        [HttpPost]
        public IHttpActionResult Login(LoginModel model)
        {
            return Ok(ExchangeServiceProvider.SetCookie(model.headers));
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
            return Ok(ExchangeServiceProvider.CookieExist());
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
    }
}
