using WebApiToTypeScript.Endpoints;
using WebApiToTypeScript.Enums;
using WebApiToTypeScript.Interfaces;
using WebApiToTypeScript.Types;

namespace WebApiToTypeScript
{
    public abstract class ServiceAware
    {
        protected const string IEndpoint = WebApiToTypeScript.IEndpoint;
        protected const string Endpoints = WebApiToTypeScript.Endpoints;

        protected TypeService TypeService
            => WebApiToTypeScript.TypeService;

        protected EnumsService EnumsService
            => WebApiToTypeScript.EnumsService;

        protected InterfaceService InterfaceService
            => WebApiToTypeScript.InterfaceService;

        protected IEndpointsService LibraryEndpointsService
            => WebApiToTypeScript.LibraryEndpointsService;

        public Config.Config Config
            => WebApiToTypeScript.Config;

        public void LogMessage(string message)
            => WebApiToTypeScript.LogMessages.Add(message);
    }
}