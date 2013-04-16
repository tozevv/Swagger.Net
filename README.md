Original project, copyright and all rightfull credits in https://github.com/miketrionfo/Swagger.Net

Fork of Swagger.Net to include some simple new features:

- Lowercase route support: supports /tag instead of /Tag
- Attribute to control authorization in swagger API: implement a ISwaggerAuthorization to control if a resource description should be included
- Ignore Route Query Parameters: parameters in the route /tag?sort={sort} get messy in swagger. Apparently there is no problem in treating them as regular FromUri parameters instead of path parameters. 
