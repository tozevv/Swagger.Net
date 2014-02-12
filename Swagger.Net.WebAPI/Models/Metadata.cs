using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;

namespace Swagger.Net.WebApi.Models
{
    /// <summary>
    /// Main metadata response object. All responses are output as this, except void.
    /// </summary>
    public class Metadata
    {
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public object Content { get; set; }

        /// <summary>
        /// Gets or sets the meta.
        /// </summary>
        /// <value>
        /// The meta.
        /// </value>
        public MetaBase Meta { get; set; }
    }

    #region Base metadata

    /// <summary>
    /// Base metadata for responses
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MetaBase
    {
        /// <summary>
        /// Serves as placeholder for the response pipeline. Content in here will be placed in Metadata.Content
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public int HttpCode { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public object Message { get; set; }
    }

    /// <summary>
    /// Generice metadata response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Metadata<T> : Metadata
    {
        public Metadata()
        {
            Meta = new MetaBase();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class MetadataExtensions
    {
        /// <summary>
        /// Ases the metadata.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content">The content.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static Metadata<T> AsMetadata<T>(this object content, HttpStatusCode statusCode = HttpStatusCode.OK,
                                                string message = null)
        {
            var metadata = new Metadata<T>();
            metadata.Content = content;
            metadata.Meta.HttpCode = (int) statusCode;

            return metadata;
        }
    }

    #endregion

    #region Paged metadata

    /// <summary>
    /// Metadata for Paged response
    /// </summary>
    /// <typeparam name="T">For documentation only</typeparam>
    public class PagedMetadata<T> : Metadata<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedMetadata{T}"/> class.
        /// </summary>
        public PagedMetadata()
        {
            Meta = new PagedMeta();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class PagedMeta : MetaBase
    {
        /// <summary>
        /// Gets or sets the paging.
        /// </summary>
        /// <value>
        /// The paging.
        /// </value>
        public PagingMetadata Paging { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public object Index { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class PagedExtentions
    {
        /// <summary>
        /// Ases the metadata.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paged">The paged.</param>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static PagedMetadata<T> AsMetadata<T>(this Paged<T> paged,
                                                     HttpStatusCode httpStatusCode = HttpStatusCode.OK,
                                                     string message = null) 
        {

            var metadata = new PagedMetadata<T>();
            var pagedMeta = metadata.Meta as PagedMeta;

            metadata.Content = paged.Content;
            pagedMeta.Paging = paged.Paging;
            ;
            pagedMeta.Message = message;
            pagedMeta.HttpCode = (int) httpStatusCode;
            pagedMeta.Index = paged.Index;
            metadata.Meta = pagedMeta;

            return metadata;
        }
    }

    public class Paged<T>
    {
        public object Content { get; set; }

        public PagingMetadata Paging { get; set; }

        public object Index { get; set; }
    }

    #endregion

    #region Pending actions metadata

    /// <summary>
    /// Metadata for pending actions response
    /// </summary>
    /// <typeparam name="T">For documentation only</typeparam>
    public class ActionsMetadata<T> : Metadata<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionsMetadata{T}"/> class.
        /// </summary>
        public ActionsMetadata()
        {
            Meta = new ActionsMeta();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class ActionsMeta : MetaBase
    {
        public IEnumerable<PendingAction> Actions { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class PendingActionsExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pendingActionsMessage"></param>
        /// <param name="httpStatusCode"></param>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ActionsMetadata<T> AsMetadata<T>(this PendingActionsMessage pendingActionsMessage,
                                                       HttpStatusCode httpStatusCode = HttpStatusCode.OK,
                                                       string message = null)
        {
            var metadata = new ActionsMetadata<T>();
            var actionsMeta = metadata.Meta as ActionsMeta;
            metadata.Content = pendingActionsMessage.Content;

            actionsMeta.HttpCode = (int) httpStatusCode;
            actionsMeta.Message = message;
            actionsMeta.Actions = pendingActionsMessage.PendingActions.Select(p => p.Value);
            metadata.Meta = actionsMeta;

            return metadata;
        }
    }

    /// <summary>
    /// Pending actions
    /// </summary>
    public class PendingActionsMessage
    {
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public object Content { get; set; }
        /// <summary>
        /// Gets or sets the pending actions.
        /// </summary>
        /// <value>
        /// The pending actions.
        /// </value>
        public OrderedDictionary<ApiLinkRelType, PendingAction> PendingActions { get; set; }
    }
    #endregion

    /// <summary>
    /// Action
    /// </summary>
    public class PendingAction
    {
        /// <summary>
        /// Action Type
        /// </summary>
        public ApiLinkRelType Action { get; set; }
        /// <summary>
        /// Action related Link
        /// </summary>
        //public IApiLink Link { get; set; }
        /// <summary>
        /// Action Status
        /// </summary>
        public ApiActionLinkStatus Status { get; set; }
    }
    public enum ApiActionLinkStatus
    {
        Pending, Done, Unknown, Optional
    }
    public enum ApiLinkRelType
    {
        self,
        next,
        previous,
        image,
        trailer,
        payWithVoucher,
        payWithAccount,
        payWithAccount2step,
        category,
        genre,
        top,
        mymovies,
        catalog,
        banner,
        banners,
        htmllabel,
        external,
        navigation,
        faq,
        faqGroup,
        signIn,
        signUp,
        tAndC,
        accountManagement,
        search,
        help,
        settings,
        asset,
        adultPinCheck,
        adultPinSet,
        favorites,
        setupMppAccount,
        mppInformation,
        transactionPinCheck,
        transactionPinSet,
        redirect,
        nameValueContent,
        structuredContent,
        person,
        video,
        favorite,
        entitlement,
        updateAccountDetails,
        paymentOptions,
        rating,
        videoOptions,
        adultPinRecover,
        transactionPinRecover,
        mppDetails,
        playbackPosition
    };

    /// <summary>
    /// Metadata for pagination
    /// </summary>
    public class PagingMetadata 
    {
        /// <summary>
        /// Number of records skipped 
        /// </summary>
        public uint? Skip { get; set; }

        /// <summary>
        /// Number of records requested
        /// </summary>
        public uint? Top { get; set; }

        /// <summary>
        /// Used sort order.
        /// Prepend with '-' to denote reverse sort
        /// </summary>
        public string Sort { get; set; }

        /// <summary>
        /// Total number of records in the list
        /// </summary>
        public uint Total { get; set; }

        /// <summary>
        /// Number of returned records 
        /// </summary>
        public uint Count { get; set; }
    }


    /// <summary>
    /// A dictionary that remembers the order that keys were first inserted. If a new entry overwrites an existing entry, the original insertion position is left unchanged. Deleting an entry and reinserting it will move it to the end.
    /// </summary>
    /// <typeparam name="TKey">The type of keys</typeparam>
    /// <typeparam name="TValue">The type of values</typeparam>
    /// <remarks> source: https://gist.github.com/matt-hickford/5137384</remarks>
    public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// The value of the element at the given index.
        /// </summary>
        TValue this[int index] { get; set; }

        /// <summary>
        /// Find the position of an element by key. Returns -1 if the dictionary does not contain an element with the given key.
        /// </summary>
        int IndexOf(TKey key);

        /// <summary>
        /// Insert an element at the given index.
        /// </summary>
        void Insert(int index, TKey key, TValue value);

        /// <summary>
        /// Remove the element at the given index.
        /// </summary>
        void RemoveAt(int index);
    }

    /// <summary>
    /// A dictionary that remembers the order that keys were first inserted. If a new entry overwrites an existing entry, the original insertion position is left unchanged. Deleting an entry and reinserting it will move it to the end.
    /// </summary>
    /// <typeparam name="TKey">The type of keys. Musn't be <see cref="int"/></typeparam>
    /// <typeparam name="TValue">The type of values.</typeparam>
    public sealed class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>
    {
        /// <summary>
        /// An unordered dictionary of key pairs.
        /// </summary>
        private readonly Dictionary<TKey, TValue> fDictionary;

        /// <summary>
        /// The keys of the dictionary in the exposed order.
        /// </summary>
        private readonly List<TKey> fKeys;

        /// <summary>
        /// A dictionary that remembers the order that keys were first inserted. If a new entry overwrites an existing entry, the original insertion position is left unchanged. Deleting an entry and reinserting it will move it to the end.
        /// </summary>
        public OrderedDictionary()
        {
            if (typeof(TKey) == typeof(int))
                throw new NotSupportedException("Integer-like type is not appropriate for keys in an ordered dictionary - accessing values by key or index would be confusing.");

            fDictionary = new Dictionary<TKey, TValue>();
            fKeys = new List<TKey>();
        }

        /// <summary>
        /// The number of elements in the dictionary.
        /// </summary>
        public int Count
        {
            get
            {
                return fDictionary.Count;
            }
        }

        /// <summary>
        /// This dictionary is not read only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// The keys of the dictionary, in order.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                return fKeys.AsReadOnly();
            }
        }

        /// <summary>
        /// The values in the dictionary, in order.
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                return fKeys.Select(key => fDictionary[key]).ToArray();
            }
        }

        /// <summary>
        /// The value at the given index.
        /// </summary>
        public TValue this[int index]
        {
            get
            {
                var key = fKeys[index];
                return fDictionary[key];
            }
            set
            {
                var key = fKeys[index];
                fDictionary[key] = value;
            }
        }

        /// <summary>
        /// The value under the given key. New entries are added at the end of the order. Updating an existing entry does not change its position.     
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                return fDictionary[key];
            }
            set
            {
                if (!fDictionary.ContainsKey(key))
                {
                    // New entries are added at the end of the order.
                    fKeys.Add(key);
                }

                fDictionary[key] = value;
            }
        }

        /// <summary>
        ///  Find the position of an element by key. Returns -1 if the dictionary does not contain an element with the given key.
        /// </summary>
        public int IndexOf(TKey key)
        {
            return fKeys.IndexOf(key);
        }

        /// <summary>
        /// Remove the element at the given index.
        /// </summary>
        public void RemoveAt(int index)
        {
            var key = fKeys[index];
            fDictionary.Remove(key);
            fKeys.RemoveAt(index);
        }

        /// <summary>
        /// Test whether there is an element with the given key.
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return fDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Try to get a value from the dictionary, by key. Returns false if there is no element with the given key.
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return fDictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Insert an element at the given index.
        /// </summary>
        public void Insert(int index, TKey key, TValue value)
        {
            // Dictionary operation first, so exception thrown if key already exists.
            fDictionary.Add(key, value);
            fKeys.Insert(index, key);
        }

        /// <summary>
        /// Add an element to the dictionary.
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            // Dictionary operation first, so exception thrown if key already exists.
            fDictionary.Add(key, value);
            fKeys.Add(key);
        }

        /// <summary>
        /// Add an element to the dictionary.
        /// </summary>
        public void Add(KeyValuePair<TKey, TValue> pair)
        {
            Add(pair.Key, pair.Value);
        }

        /// <summary>
        /// Test whether the dictionary contains an element equal to that given.
        /// </summary>
        public bool Contains(KeyValuePair<TKey, TValue> pair)
        {
            return fDictionary.Contains(pair);
        }

        /// <summary>
        /// Remove a key-value pair from the dictionary. Return true if pair was successfully removed. Otherwise, if the pair was not found, return false.
        /// </summary>
        public bool Remove(KeyValuePair<TKey, TValue> pair)
        {
            if (Contains(pair))
            {
                Remove(pair.Key);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove the element with the given key key. If there is no element with the key, no exception is thrown.
        /// </summary>
        public bool Remove(TKey key)
        {
            bool wasInDictionary = fDictionary.Remove(key);
            bool wasInKeys = fKeys.Remove(key);
            Contract.Assume(wasInDictionary == wasInKeys);
            return wasInDictionary;
        }

        /// <summary>
        /// Delete all elements from the dictionary.
        /// </summary>
        public void Clear()
        {
            fDictionary.Clear();
            fKeys.Clear();
        }

        /// <summary>
        /// Copy the elements of the dictionary to an array, starting at at the given index.
        /// </summary>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "Must be greater than or equal to zero.");

            if (index + fDictionary.Count > array.Length)
                throw new ArgumentException("array", "Array is too small");

            foreach (var pair in this)
            {
                array[index] = pair;
                index++;
            }
        }

        private IEnumerable<KeyValuePair<TKey, TValue>> Enumerate()
        {
            foreach (var key in fKeys)
            {
                var value = fDictionary[key];
                yield return new KeyValuePair<TKey, TValue>(key, value);
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        /// <summary>
        /// Conditions that should be true at the end of every public method.
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(fDictionary.Count == fKeys.Count, "Unordered dictionary and ordered key list should be the same length.");
        }
    }

}

