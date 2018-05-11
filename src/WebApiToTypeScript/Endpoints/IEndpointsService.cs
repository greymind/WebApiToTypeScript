using WebApiToTypeScript.Block;
using WebApiToTypeScript.WebApi;

namespace WebApiToTypeScript.Endpoints
{
    public interface IEndpointsService
    {
        TypeScriptBlock CreateServiceBlock();
        void WriteServiceObjectToBlock(TypeScriptBlock serviceBlock, WebApiController webApiController);
    }
}