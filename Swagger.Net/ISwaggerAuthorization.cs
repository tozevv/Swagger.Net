using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Filters;

namespace Swagger.Net
{
    public interface ISwaggerAuthorization 
    {
        bool IsDescriptionAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext);
    }
}
