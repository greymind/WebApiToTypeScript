using System;
using System.Collections.Generic;
using System.Web.Http;

namespace WebApiTestApplication.Controllers
{
    public class EncryptedIntAttribute : Attribute
    {
    }

    [RoutePrefix("api/Test/{hole}/actions")]
    public class TestController : ApiController
    {
        [HttpGet]
        [Route("GetAll")]
        public IEnumerable<string> Get(string hole)
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        [Route("")]
        public string Get([EncryptedInt] int id, string hole)
        {
            return $"value {id}";
        }

        [HttpGet]
        [Route("getSomething/{id}/ha")]
        public string GetSomething(string hole, int id, int y = 7)
        {
            return $"value {id} {y}";
        }

        [HttpGet]
        [Route("GetSomethingElse")]
        public string GetSomethingElse(int id, string y, string hole)
        {
            return $"value {id} {y} {hole}";
        }

        [HttpPost]
        [Route("")]
        public string Post(string hole, [FromBody]string value)
        {
            return $"thanks for the {value} in the {hole}";
        }

        [HttpPut]
        [Route("{id}")]
        public void Put(int id, [FromBody]string value, string hole)
        {
        }

        [HttpDelete]
        [Route("")]
        public void Delete(int id, string hole)
        {
        }
    }
}