using WebApiToTypeScript.Enums;
using WebApiToTypeScript.Interfaces;
using WebApiToTypeScript.Types;

namespace WebApiToTypeScript
{
    public abstract class ServiceAware
    {
        protected string IEndpoint = WebApiToTypeScript.IEndpoint;

        protected TypeService TypeService
            => WebApiToTypeScript.TypeService;

        protected EnumsService EnumsService
            => WebApiToTypeScript.EnumsService;

        protected InterfaceService InterfaceService
            => WebApiToTypeScript.InterfaceService;

        public Config.Config Config
            => WebApiToTypeScript.Config;

        public void LogMessage(string message)
            => WebApiToTypeScript.LogMessages.Add(message);
    }
}