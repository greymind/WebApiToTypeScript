using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Results;
using Newtonsoft.Json;

namespace WebApiTestApplication.Controllers
{
    public class EncryptedIntBinding : HttpParameterBinding
    {
        public EncryptedIntBinding(HttpParameterDescriptor descriptor)
            : base(descriptor)
        {
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider,
            HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var queryString = actionContext.Request
                .RequestUri
                .Query;

            var query = HttpUtility.ParseQueryString(queryString);

            var value = query.GetValues(Descriptor.ParameterName)
                .Single();

            SetValue(actionContext, int.Parse(value));

            var tsc = new TaskCompletionSource<object>();
            tsc.SetResult(null);
            return tsc.Task;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class EncryptedIntAttribute : ParameterBindingAttribute
    {
        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameterDescriptor)
        {
            return new EncryptedIntBinding(parameterDescriptor);
        }
    }

    public enum DummyEnum
    {
        Hi = 1,
        [Description("Bye a lot")]
        Bye = 2,
        ValueWithMultipleUppercaseWords = 3
    }

    public class DummyClass
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public AnotherClass C { get; set; }
    }

    public class AnotherClass
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string[] List { get; set; }
    }

    public class DerivedClassWithShadowedProperty : AnotherClass
    {
        public new string Number { get; set; }
    }

    public class DerivedClassWithAnotherShadowedProperty : DerivedClassWithShadowedProperty
    {
        public new int Number { get; set; }
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
            return $"value {hole} / {id}";
        }
        
        [HttpGet]
        [Route("getSomething/{id}/ha")]
        public string GetSomething(string hole, int id, DummyEnum y = DummyEnum.Bye)
        {
            return $"value {id} {y.ToString()}";
        }

        [HttpGet]
        [Route("GetSomethingElse")]
        public string GetSomethingElse(int id, [FromUri]DummyClass y, string hole)
        {
            return $"{nameof(GetSomethingElse)}: {id} {y.Name} {y.Date.ToShortDateString()} {hole}";
        }

        [HttpGet]
        [Route("GetEnumerableString")]
        public IEnumerable<string> GetEnumerableString()
        {
            return new List<string>();
        }

        [HttpGet]
        [Route("GetIHttpActionResult")]
        public IHttpActionResult GetIHttpActionResult()
        {
            return new OkResult(new HttpRequestMessage());
        }

        [HttpPost]
        [Route("")]
        public string Post(string hole, DummyClass value)
        {
            var valueJson = JsonConvert.SerializeObject(value);
            return $"thanks for the {valueJson} in the {hole}";
        }

        [HttpPost]
        [Route("derived")]
        public string Post(string hole, DerivedClassWithShadowedProperty value)
        {
            var valueJson = JsonConvert.SerializeObject(value);
            return $"thanks for the {valueJson} in the {hole}";
        }

        [HttpPost]
        [Route("derivedAgain")]
        public string Post(string hole, DerivedClassWithAnotherShadowedProperty value)
        {
            var valueJson = JsonConvert.SerializeObject(value);
            return $"thanks for the {valueJson} in the {hole}";
        }

        [HttpPut]
        [Route("{id}")]
        public string Put(int id, [FromBody]string value, string hole)
        {
            var valueJson = JsonConvert.SerializeObject(value);
            return $"Putting {valueJson} for {id} in {hole}";
        }

        [HttpDelete]
        [Route("")]
        public string Delete(int id, string hole)
        {
            return $"{id} in {hole} deleted!";
        }
    }
}