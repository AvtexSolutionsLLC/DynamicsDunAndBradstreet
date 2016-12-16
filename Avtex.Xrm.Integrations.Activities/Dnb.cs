using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

using Newtonsoft.Json;
using System.Web.Script.Serialization;

using System.Collections;
using System.Collections.ObjectModel;

using System.Dynamic;

namespace Avtex.Xrm.Integrations
{
    public class ApiManager
    {

        public Guid id { get; set; }
        public string Name { get; set; }
        public string ProductName { get; set; }
        public string ProductUrl { get; set; }
        public string AuthUrl { get; set; }
        public HttpClient Client = new HttpClient();
        public string Token = "";
        public string AccountId { get; set; }
        public string AccountPwd { get; set; }
         private const string ACCOUNT_ID = "";
        private const string ACCOOUNT_PWD = "";
        private const string ROOT_AUTHENTICATION_URL = "https://direct.dnb.com/rest/Authentication";
        //private static HttpClient client = new HttpClient();

        #region Constructors

        //public DnbIntegrationService(x.IValidationDictionary validationDictionary)
        //    : this(validationDictionary, new xRepository())
        //{ }

        public ApiManager(string authToken)
        {

            Token = authToken;
        }
        public ApiManager()
        {

            Client.BaseAddress = new Uri(ROOT_AUTHENTICATION_URL);
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Add("x-dnb-user", ACCOUNT_ID);
            Client.DefaultRequestHeaders.Add("x-dnb-pwd", ACCOOUNT_PWD);
        }
        #endregion

        public async Task<String> GetAuthToken()
        {
            string token = null;
            HttpResponseMessage response = await Client.GetAsync(this.AuthUrl);
            if (response.IsSuccessStatusCode)
            {
                HttpHeaders headers = response.Headers;
                IEnumerable<string> values;
                if (headers.TryGetValues("Authorization", out values))
                {
                    token = values.First();
                }
            }
            return token;
        }
    }
    public class DataTransferObjects{
    public class OrderProductResponseItem{

                public string ServiceVersionNumber { get; set; }

                //TransactionDetail 
                public string ServiceTransactionID { get; set; }
                public string TransactionTimestamp { get; set; }

                //TransactionResult 
                public string SeverityText { get; set; }
                public string ResultID { get; set; }
                public string ResultText { get; set; }

                //OrderProductResponseDetail

                //OrderProductResponseDetail.InquiryDetail
                public string DUNSNumber { get; set; }
                public string CountryISOAlpha2Code { get; set; }

                //OrderProductResponseDetail.Product
                public string DNBProductID { get; set; }

                //OrderProductResponseDetail.Product.Organization

                //OrderProductResponseDetail.Product.Organization.SubjectHeader
                public string OrganizationSummaryText { get; set; }
                public string LastUpdateDate { get; set; }

                //OrderProductResponseDetail.Product.Organization.Telecommunication.TelephoneNumber   
                public string TelecommunicationNumber { get; set; }
                public string InternationalDialingCode { get; set; }

                //OrderProductResponseDetail.Product.Organization.Location

                //OrderProductResponseDetail.Product.Organization.Location.PrimaryAddress
                public string StreetAddressLine { get; set; }
                public string PrimaryTownName { get; set; }
                public string PrimaryCountryISOAlpha2Code { get; set; }
                public string TerritoryAbbreviatedName { get; set; }
                public string PostalCode { get; set; }

                //OrderProductResponseDetail.Product.Organization.Location.AddressUsageTenureDetail	
                public string TenureTypeText { get; set; }

                //OrderProductResponseDetail.Product.Organization.Location.PremisesUsageDetail.PremisesUsageFunctionDetail
                public string PremisesFunctionText { get; set; }

                //OrderProductResponseDetail.Product.Organization.Location   
                public string CountyOfficialName { get; set; }
                public string TerritoryOfficialName { get; set; }
                public string CountryGroupName { get; set; }
                public string MetropolitanStatisticalAreaUSCensusCode { get; set; }

                //OrderProductResponseDetail.Product.Organization.Financial	

                public string FinancialStatementToDate { get; set; }
                public string FinancialPeriodDuration { get; set; }
                public string SalesRevenueAmount { get; set; }
                public string UnitOfSize { get; set; }
                public string ReliabilityText { get; set; }
                public string CurrencyISOAlpha3Code { get; set; }

                //OrderProductResponseDetail.Product.Organization.OrganizationPrimaryName		
                public string OrganizationName { get; set; }

                //OrderProductResponseDetail.Product.Organization.OrganizationDetail	
                public string ControlOwnershipDate { get; set; }
                public string ControlOwnershipTypeText { get; set; }
                public string OperatingStatusText { get; set; }
                public string OrganizationStartYear { get; set; }
                public string LegalFormText { get; set; }
                public string OrganizationIdentificationNumber { get; set; } //TIN
            
        }   
}

public sealed class DynamicJsonConverter : JavaScriptConverter
    {
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            return type == typeof(object) ? new DynamicJsonObject(dictionary) : null;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get { return new ReadOnlyCollection<Type>(new List<Type>(new[] { typeof(object) })); }
        }

        #region Nested type: DynamicJsonObject

        private sealed class DynamicJsonObject : DynamicObject
        {
            private readonly IDictionary<string, object> _dictionary;

            public DynamicJsonObject(IDictionary<string, object> dictionary)
            {
                if (dictionary == null)
                    throw new ArgumentNullException("dictionary");
                _dictionary = dictionary;
            }

            public override string ToString()
            {
                var sb = new StringBuilder("{");
                ToString(sb);
                return sb.ToString();
            }

            private void ToString(StringBuilder sb)
            {
                var firstInDictionary = true;
                foreach (var pair in _dictionary)
                {
                    if (!firstInDictionary)
                        sb.Append(",");
                    firstInDictionary = false;
                    var value = pair.Value;
                    var name = pair.Key;
                    if (value is string)
                    {
                        sb.AppendFormat("{0}:\"{1}\"", name, value);
                    }
                    else if (value is IDictionary<string, object>)
                    {
                        new DynamicJsonObject((IDictionary<string, object>)value).ToString(sb);
                    }
                    else if (value is ArrayList)
                    {
                        sb.Append(name + ":[");
                        var firstInArray = true;
                        foreach (var arrayValue in (ArrayList)value)
                        {
                            if (!firstInArray)
                                sb.Append(",");
                            firstInArray = false;
                            if (arrayValue is IDictionary<string, object>)
                                new DynamicJsonObject((IDictionary<string, object>)arrayValue).ToString(sb);
                            else if (arrayValue is string)
                                sb.AppendFormat("\"{0}\"", arrayValue);
                            else
                                sb.AppendFormat("{0}", arrayValue);

                        }
                        sb.Append("]");
                    }
                    else
                    {
                        sb.AppendFormat("{0}:{1}", name, value);
                    }
                }
                sb.Append("}");
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (!_dictionary.TryGetValue(binder.Name, out result))
                {
                    // return null to avoid exception.  caller can check for null this way...
                    result = null;
                    return true;
                }

                var dictionary = result as IDictionary<string, object>;
                if (dictionary != null)
                {
                    result = new DynamicJsonObject(dictionary);
                    return true;
                }

                var arrayList = result as ArrayList;
                if (arrayList != null && arrayList.Count > 0)
                {
                    if (arrayList[0] is IDictionary<string, object>)
                        result = new List<object>(arrayList.Cast<IDictionary<string, object>>().Select(x => new DynamicJsonObject(x)));
                    else
                        result = new List<object>(arrayList.Cast<object>());
                }

                return true;
            }
        }

        #endregion
    }

#region Repository
    public interface IDnbRepository
    {
        Task<DataTransferObjects.OrderProductResponseItem> FetchDCP(ApiManager apiManager, string dunsNumber);
    }

    public class DnbIntegrationRepository  :IDnbRepository
    {

         public async Task<DataTransferObjects.OrderProductResponseItem> FetchDCP(ApiManager apiManager, string dunsNumber)
        {
           
            apiManager.Client.DefaultRequestHeaders.Clear();
            apiManager.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            apiManager.Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiManager.Token);
           
            try
            {
                // Task<HttpResponseMessage> response = client.GetAsync(new Uri(apiManager.ProductUrl));
                HttpResponseMessage response = new HttpResponseMessage();
                response = await apiManager.Client.GetAsync(new Uri("https://direct.dnb.com/V5.3/organizations/804735132/products/DCP_PREM"));
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var serializer = new JavaScriptSerializer();
                    var result = serializer.DeserializeObject(data);
                    string json = JsonConvert.SerializeObject(result);
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    jss.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });

                    dynamic dcp = jss.Deserialize(json, typeof(object)) as dynamic;
                    DataTransferObjects.OrderProductResponseItem orderProductResponseItem = new DataTransferObjects.OrderProductResponseItem();

                    orderProductResponseItem.ServiceTransactionID = dcp.OrderProductResponse.TransactionDetail.ServiceTransactionID;
                    orderProductResponseItem.TransactionTimestamp = dcp.OrderProductResponse.TransactionDetail.TransactionTimestamp;

                    orderProductResponseItem.SeverityText = dcp.OrderProductResponse.TransactionResult.SeverityText;
                    orderProductResponseItem.ResultID = dcp.OrderProductResponse.TransactionResult.ResultID;
                    orderProductResponseItem.ResultText = dcp.OrderProductResponse.TransactionResult.ResultText;

                    orderProductResponseItem.DUNSNumber = dcp.OrderProductResponse.OrderProductResponseDetail.InquiryDetail.DUNSNumber;
                    orderProductResponseItem.CountryISOAlpha2Code = dcp.OrderProductResponse.OrderProductResponseDetail.InquiryDetail.CountryISOAlpha2Code;

                    orderProductResponseItem.DNBProductID = dcp.OrderProductResponse.OrderProductResponseDetail.Product.DNBProductID;
                    orderProductResponseItem.OrganizationSummaryText = dcp.OrderProductResponse.OrderProductResponseDetail.Product.Organization.SubjectHeader.OrganizationSummaryText;
                    orderProductResponseItem.LastUpdateDate = dcp.OrderProductResponse.OrderProductResponseDetail.Product.Organization.SubjectHeader.LastUpdateDate;

                    return orderProductResponseItem;
                }
                else
                { return null; }
                
            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }
   

    #endregion
#region Service Layer

public class DnbIntegrationService
{
        #region Private
            private IDnbRepository _repository;
            private ApiManager _apiManager;       
        #endregion

        #region Constructors


        public DnbIntegrationService(IDnbRepository repository, ApiManager apiManager)
            {
                
                _repository = repository;
                _apiManager = apiManager;
            }
        
        #endregion

        public DataTransferObjects.OrderProductResponseItem FetchDCP(string dunsNumber)
        {
           return  _repository.FetchDCP(_apiManager, dunsNumber).Result;

        }
 }

    #endregion

}
