Original project, copyright and all rightfull credits in https://github.com/miketrionfo/Swagger.Net

Fork of Swagger.Net to include some simple new features:

- Support for Models. Swagger will now output a tabbed structure of the response object. Which level is minimized and can be expanded by clicking +
- Support for response codes. Adding this to the action comments will show the response codes for each action. Ex.
        /// <responseCodes>
        ///     <response><code>200</code><message>Ok</message></response>
        ///     <response><code>204</code><message>No Content</message></response>
        ///     <response><code>401</code><message>Unauthorized</message></response>
        /// </responseCodes>
- SwaggerIgnore attribute - Excludes action from listing
- Added Required attribute to apply to properties. Applying this attribute will toogle the optional tag in the model property.
- Api Sources. It's now possible to have static documentation for the api in the docs/apiSources folder. The files in the folder contain the same structure as the JSON returned from the API but since they're static it allows to setup alternate routes.
- Enum parameters will render as dropdown list
- Support for jsonp response
- Parameter defaults <param name="id" default="12312312">The id.</param>
- <overrideReturn type="Fully qualified type"> Will override the response type with the type provided. If the type is part of another dll, there needs to be the XML file for that assembly 