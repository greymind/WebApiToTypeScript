using WebApiToTypeScript.Enums;
using WebApiToTypeScript.Types;

namespace WebApiToTypeScript.Interfaces
{
    public abstract class ServiceAware
    {
        protected string IHaveQueryParams = WebApiToTypeScript.IHaveQueryParams;
        protected string IEndpoint = WebApiToTypeScript.IEndpoint;

        protected TypeService TypeService
            => WebApiToTypeScript.TypeService;

        protected EnumsService EnumsService
            => WebApiToTypeScript.EnumsService;

        protected InterfaceService InterfaceService
            => WebApiToTypeScript.InterfaceService;

        public Config.Config Config
            => WebApiToTypeScript.Config;
    }
}