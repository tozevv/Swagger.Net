Original project, copyright and all rightfull credits in https://github.com/miketrionfo/Swagger.Net

Fork of Swagger.Net to include some simple new features:

- Support for Models. Swagger will now output a tabbed structure of the response object. Which level is minimized and can be expanded by clicking +
- Support for response codes. Adding this to the action comments will show the response codes for each action. Ex.
        /// &lt;responseCodes&gt;
        ///     &lt;response&gt;&lt;code&gt;200&lt;/code&gt;&lt;message&gt;Ok&lt;/message&gt;&lt;/response&gt;
        ///     &lt;response&gt;&lt;code&gt;204&lt;/code&gt;&lt;message&gt;No Content&lt;/message&gt;&lt;/response&gt;
        ///     &lt;response&gt;&lt;code&gt;401&lt;/code&gt;&lt;message&gt;Unauthorized&lt;/message&gt;&lt;/response&gt;
        /// &lt;/responseCodes&gt;
- SwaggerIgnore attribute - Excludes action from listing
- Added Required attribute to apply to properties. Applying this attribute will toogle the optional tag in the model property.
- Api Sources. It's now possible to have static documentation for the api in the docs/apiSources folder. The files in the folder contain the same structure as the JSON returned from the API but since they're static it allows to setup alternate routes.
- Enum parameters will render as dropdown list
- Support for jsonp response
- Parameter defaults. Will set a default value in the parameter text field &amp;lt;param name=&quot;id&quot; default=&quot;12312312&quot;&amp;gt;The id.&amp;lt;/param&amp;lt;
- &lt;overrideReturn type=&quot;Fully qualified type&quot;&gt; Will override the response type with the type provided. If the type is part of another dll, there needs to be the XML file for that assembly 