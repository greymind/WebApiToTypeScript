using System;
using System.Collections.Generic;
using System.Web.Http;

namespace WebApiTestApplication.Controllers
{
    [RoutePrefix("api/Test")]
    public class TestController : ApiController
    {
        // GET: api/Test
        [HttpGet]
        [Route("GetAll")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Test/5
        [HttpGet]
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet]
        public string GetSomething(int id, int y = 7)
        {
            return "value";
        }

        // POST: api/Test
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Test/5
        [HttpPut]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Test/5
        [HttpDelete]
        public void Delete(int id)
        {
        }
    }
}