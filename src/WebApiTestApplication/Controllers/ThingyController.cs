using System.Web.Http;
using Newtonsoft.Json;

namespace WebApiTestApplication.Controllers
{
    public class MegaClass : AnotherClass
    {
        public int Something { get; set; }
    }

    public class Chain1<T>
    {
        public T Value { get; set; }
    }

    public class Chain1<T1, T2>
    {
        public T1 Value11 { get; set; }
        public T2 Value12 { get; set; }
    }

    public class Chain2<TValue> : Chain1<TValue, int>
    {
        public TValue Value2 { get; set; }
    }

    public class Chain3 : Chain2<MegaClass>
    {
        public object Value3 { get; set; }
    }

    [RoutePrefix("api/thingy")]
    public class ThingyController : ApiController
    {
        [HttpGet]
        [Route("")]
        public string GetAll()
        {
            return $"values";
        }

        [HttpGet]
        [Route("{id}")]
        public string Get(int? id, string x, [FromUri]MegaClass c)
        {
            var valueJson = JsonConvert.SerializeObject(c);
            return $"value {id} {x} {valueJson}";
        }

        [HttpGet]
        [Route("")]
        public string Getty(string x, int y)
        {
            return $"value {x} {y}";
        }

        [HttpPost]
        [Route("")]
        public string Post(MegaClass value)
        {
            var valueJson = JsonConvert.SerializeObject(value);
            return $"thanks for the {valueJson} in the ace!";
        }
    }
}