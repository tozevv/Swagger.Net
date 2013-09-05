using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;

namespace Swagger.Net.WebApi.Controllers
{
    public class TagsController : ApiController
    {
        /// <summary>
        /// Get all of the Tags
        /// </summary>
        /// <returns>Nothing</returns>
        /// <responseCodes> 
        ///     <response>
        ///         <code>500</code>
        ///         <message>Internal server errors</message>
        ///     </response>
        /// </responseCodes>
        /// <remarks>Notes</remarks>
        public Tag[] Get(TagType tagType = TagType.NotDefined)
        {
            return new List<Tag>() { new Tag() }.ToArray();
        }

        /// <summary>
        /// What it does
        /// </summary>
        /// <param name="id" default="12312312">The id.</param>
        /// <responseCodes>
        ///     <response>
        ///         <code>401</code>
        ///         <message>Not authorized</message>
        ///     </response>
        ///     <response>
        ///         <code>400</code>
        ///         <message>Bad request</message>
        ///     </response>
        /// </responseCodes>
        /// <remarks>
        ///     Implementation Notes
        /// </remarks>
        public Tag GetById(int id, TagType tagType = TagType.NotDefined)
        {
            return new Tag();
        }

        
        /// <summary>
        /// Create a new Tag
        /// </summary>
        /// <param name="value" default="xxx"></param>
        public void Post([FromBody]string value)
        {
        }

        /// <summary>
        /// Update a Tag by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [SwaggerIgnore]
        public void Put(int id, [FromBody]string value)
        {
        }

        /// <summary>
        /// Remove a Tag by it's id
        /// </summary>
        /// <param name="id"></param>
        public void Delete(int id)
        {
        }
    }


    /// <summary>
    /// Base Tag implementation
    /// </summary>
    public interface ITag
    {
        /// <summary>
        /// INTERFACE Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        int? Value { get; set; }
        /// <summary>
        /// Gets or sets the rating.
        /// </summary>
        /// <value>
        /// The rating.
        /// </value>
        Rating Rating { get; set; }
    }

    /// <summary>
    /// Tag Summary
    /// </summary>
    public class Tag : ITag
    {
        /// <summary>
        /// IMPLEMENTATION Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty("CustomName")]
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Value { get; set; }
        public TagType Type { get; set; }
        /// <summary>
        /// Gets or sets the rating.
        /// </summary>
        /// <value>
        /// The rating.
        /// </value>
        /// <required>false</required>
        public Rating Rating { get; set; }
        /// <summary>
        /// Gets or sets the old ratings.
        /// </summary>
        /// <value>
        /// The old ratings.
        /// </value>
        public IEnumerable<Rating> OldRatings { get; set; }
    }

    public class Rating
    {
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public Tag Tag { get; set; }
    }

    public enum TagType
    {
        Normal, Complex, NotDefined
    }
}
