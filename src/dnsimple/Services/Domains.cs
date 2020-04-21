using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;
using static dnsimple.JsonTools<dnsimple.Services.Domain>;

namespace dnsimple.Services
{
    /// <summary>
    /// The <c>DomainsService</c> handles communication with the domain related
    /// methods of the DNSimple API.
    /// </summary>
    /// <see>https://developer.dnsimple.com/v2/domains/</see>
    public partial class DomainsService
    {
        private IClient Client { get; }

        /// <summary>
        /// Creates a new instance of a <c>DomainsService</c> by passing an
        /// instance of the DNSimple <c>IClient</c>.
        /// </summary>
        /// <param name="client">An instance of the <c>IClient</c></param>
        /// <see cref="IClient"/>
        public DomainsService(IClient client) => 
            Client = client;

        /// <summary>
        /// Retrieves the details of an existing domain.
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <param name="domainIdentifier">The domain name or ID</param>
        /// <returns>A <c>DomainResponse</c> containing the data of the domain
        /// requested.</returns>
        public DomainResponse GetDomain(long accountId, string domainIdentifier)
        {
            return new DomainResponse(Client.Http.Execute(Client.Http
                .RequestBuilder(DomainPath(accountId, domainIdentifier))
                .Request));
        }

        /// <summary>
        /// Lists the domains in the account.
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <param name="options">Options passed to the list (sorting, filtering, pagination)</param>
        /// <returns>A <c>DomainResponse</c> containing a list of domains.</returns>
        /// <see cref="DomainListOptions"/>
        public DomainsResponse ListDomains(long accountId, ListOptionsWithFiltering options)
        {
            var requestBuilder =
                Client.Http.RequestBuilder(DomainsPath(accountId));
            requestBuilder.AddParameter(options.UnpackSorting());
            requestBuilder.AddParameters(options.UnpackFilters());
            
            if (!options.Pagination.IsDefault())
            {
                requestBuilder.AddParameters(options.UnpackPagination());
            }
            

            return new DomainsResponse(
                Client.Http.Execute(requestBuilder.Request));
        }
        
        /// <summary>
        /// Lists the domains in the account.
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <returns>A <c>DomainResponse</c> containing a list of domains.</returns>
        public DomainsResponse ListDomains(long accountId)
        {
            var requestBuilder =
                Client.Http.RequestBuilder(DomainsPath(accountId));

            return new DomainsResponse(
                Client.Http.Execute(requestBuilder.Request));
        }
        
        /// <summary>
        /// Adds a domain to the account.
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <param name="domainName">The name of the domain to be created</param>
        /// <returns>A <c>DomainResponse</c> containing the data of the newly
        /// created domain.</returns>
        public DomainResponse CreateDomain(long accountId, string domainName)
        {
            var request =
                Client.Http.RequestBuilder(DomainsPath(accountId));
            request.Method(Method.POST);

            var parameters = new Collection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("name", domainName)
            };
            request.AddParameters(parameters);

            return new DomainResponse(Client.Http.Execute(request.Request));
        }

        /// <summary>
        /// Permanently deletes a domain from the account.
        /// </summary>
        /// <remarks>It cannot be undone.</remarks>
        /// <param name="accountId">The account ID</param>
        /// <param name="domainIdentifier">The domain name or id</param>
        public void DeleteDomain(long accountId, string domainIdentifier)
        {
            var request =
                Client.Http.RequestBuilder(DeleteDomainPath(accountId,
                    domainIdentifier));
            request.Method(Method.DELETE);

            Client.Http.Execute(request.Request);
        }

        private static string DeleteDomainPath(long accountId,
            string domainIdentifier)
        {
            return $"{DomainsPath(accountId)}/{domainIdentifier}";
        }

        private static string DomainPath(long accountId,
            string domainIdentifier)
        {
            return $"/{accountId}/domains/{domainIdentifier}";
        }

        private static string DomainsPath(long accountId)
        {
            return $"/{accountId}/domains";
        }
    }

    /// <summary>
    /// Represents the response from the API call containing one <c>Domain</c>
    /// </summary>
    /// <see cref="Domain"/>
    public class DomainResponse : SimpleDnsimpleResponse<Domain>
    {
        public DomainResponse(JToken json) : base(json)
        {
        }
    }

    /// <summary>
    /// Represents the response from the API call containing (potentially)
    /// multiple <c>Domain</c> objects and a <c>Pagination</c> object.
    /// </summary>
    /// <see cref="DomainsData"/>
    /// <see cref="PaginationData"/>
    public class DomainsResponse : PaginatedDnsimpleResponse<DomainsData>
    {
        public DomainsResponse(JToken response) : base(response) =>
            Data = new DomainsData(response);
    }

    /// <summary>
    /// Represents the <c>struct</c> containing a <c>List</c> of <c>Domain</c>
    /// objects.
    /// </summary>
    /// <see cref="List{T}"/>
    /// <see cref="Domain"/>
    public readonly struct DomainsData
    {
        /// <summary>
        /// The list of <c>Domain</c> objects
        /// </summary>
        public List<Domain> Domains { get; }

        /// <summary>
        /// Constructs a new <c>DomainsData</c> object by passing in the
        /// <c>JToken</c>.
        /// </summary>
        /// <param name="json"></param>
        public DomainsData(JToken json) => 
            Domains = DeserializeList(json);
    }

    /// <summary>
    /// Represents a <c>Domain</c>
    /// </summary>
    /// <see>https://developer.dnsimple.com/v2/domains/</see>
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public struct Domain
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public long? RegistrantId { get; set; }
        public string Name { get; set; }
        public string UnicodeName { get; set; }
        public string State { get; set; }
        public bool? AutoRenew { get; set; }
        public bool? PrivateWhois { get; set; }
        public string ExpiresOn { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
    /// <summary>
    /// Defines the options you may want to send to list domains, such as
    /// pagination, sorting and filtering.
    /// </summary>
    /// <see cref="ListOptionsWithFiltering"/>
    public class DomainListOptions : ListOptionsWithFiltering
    {
        private const string NameLikeFilter = "name_like";
        private const string RegistrantIdFilter = "registrant_id";

        private const string IdSort = "id";
        private const string NameSort = "name";
        private const string ExpiresOnSort = "expires_on";

        /// <summary>
        /// Creates a new instance of <c>DomainListOptions</c>.
        /// </summary>
        public DomainListOptions()
           => Pagination = new Pagination();
        
        /// <summary>
        /// Sets the order by which to sort by id.
        /// </summary>
        /// <param name="order">The order in which we want to sort (asc or desc)</param>
        /// <returns>The instance of the <c>DomainListOptions</c></returns>
        public DomainListOptions SortById(Order order)
        {
            AddSortCriteria(new Sort { Field = IdSort, Order = order });
            return this;
        }

        /// <summary>
        /// Sets the order by which to sort by name.
        /// </summary>
        /// <param name="order">The order in which we want to sort (asc or desc)</param>
        /// <returns>The instance of the <c>DomainListOptions</c></returns>
        public DomainListOptions SortByName(Order order)
        {
            AddSortCriteria(new Sort { Field = NameSort, Order = order });
            return this;
        }

        /// <summary>
        /// Sets the order by which to sort by expires on.
        /// </summary>
        /// <param name="order">The order in which we want to sort (asc or desc)</param>
        /// <returns>The instance of the <c>DomainListOptions</c></returns>
        public DomainListOptions SortByExpiresOn(Order order)
        {
            AddSortCriteria(new Sort { Field = ExpiresOnSort, Order = order });
            return this;
        }

        /// <summary>
        /// Sets the name to be filtered by.
        /// </summary>
        /// <param name="name">The name we want to filter by.</param>
        /// <returns>The instance of the <c>DomainListOptions</c></returns>
        public DomainListOptions FilterByName(string name)
        {
            AddFilter(new Filter { Field = NameLikeFilter, Value = name });
            return this;
        }

        /// <summary>
        /// Sets the registrant id to be filtered by.
        /// </summary>
        /// <param name="registrantId">The registrant id we want to filter by.</param>
        /// <returns>The instance of the <c>DomainListOptions</c></returns>
        public DomainListOptions FilterByRegistrantId(long registrantId)
        {
            AddFilter(new Filter { Field = RegistrantIdFilter, 
                Value = registrantId.ToString() });
            
            return this;
        }
    }
}