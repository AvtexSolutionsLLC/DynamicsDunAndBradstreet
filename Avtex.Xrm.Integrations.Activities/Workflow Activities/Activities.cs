using System;
using System.Linq;
using System.Text;
using System.Activities;
using System.Net;
using System.IO;
using System.Net.Http.Headers;
using System.Globalization;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Avtex.Xrm.Integrations
{
    public sealed class LocateCompanyBasic : CodeActivity
    {
        [Input("searchString")]
        public InArgument<string> searchString { get; set; }

        [Output("apiResponse")]
        public OutArgument<string> apiResponse { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {

            ITracingService tracer = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                tracer.Trace("BEGIN Execute");
                wf_dnbsearch entity = (wf_dnbsearch)service.Retrieve("wf_dnbsearch", context.PrimaryEntityId, new ColumnSet(true));

                tracer.Trace("Process: EntityId - " + entity.Id.ToString());
                tracer.Trace("Process: Begin - ");
                //retreive CRM apimanager
                ApiManager apiManager = RetrieveApiManager(service, "wf_name", "wf_token", "wf_baseurl", "wf_accountid", "wf_accountpwd", "wf_producturl", "wf_dnb_product");
                tracer.Trace("Process: apiManager.AccountId - " + apiManager.AccountId);
                tracer.Trace("Process: apiManager.AccountPwd - " + apiManager.AccountPwd);
                tracer.Trace("Process: apiManager.AuthUrl - " + apiManager.AuthUrl);
                tracer.Trace("Process: apiManager.Name - " + apiManager.Name);
                tracer.Trace("Process: apiManager.ProductName - " + apiManager.ProductName);
                tracer.Trace("Process: apiManager.ProductUrl - " + apiManager.ProductUrl);
                tracer.Trace("Process: apiManager.Token - " + apiManager.Token);
                tracer.Trace("Process: apiManager.Client.BaseAddress - " + apiManager.Client.BaseAddress);

                Avtex.Xrm.Integrations.wf_orderproductresponse opr = new wf_orderproductresponse();

                apiManager.Client.DefaultRequestHeaders.Clear();
                apiManager.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                apiManager.Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiManager.Token);
                tracer.Trace("Process: apiManager.Client.DefaultRequestHeaders created. ");
                string response = FetchBuildaListBasic(apiManager, searchString.Get(executionContext), tracer);

                entity.wf_response = response;
                apiResponse.Set(executionContext, response);

                service.Update(entity);

            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }

        public ApiManager RetrieveApiManager(IOrganizationService service, params string[] columns)
        {
            StringBuilder tracelog = new StringBuilder();
            tracelog.AppendLine("BEGIN RetrieveApiManager");
            //tracingService.Trace("BEGIN RetrieveApiManager.");

            try
            {
                QueryExpression query = new QueryExpression()
                {
                    Distinct = false,
                    EntityName = wf_dnbapiconfiguration.EntityLogicalName,
                    ColumnSet = new ColumnSet(columns),

                };

                //"wf_name", "wf_token", "wf_baseurl", "wf_accountid", "wf_accountpwd", "wf_producturl", "wf_dnb_product");
                Entity apiEntity = service.RetrieveMultiple(query).Entities.FirstOrDefault();

                ApiManager apiManager = new ApiManager();

                apiManager.id = apiEntity.Id;

                if (apiEntity.Attributes.ContainsKey("wf_name"))
                {
                    apiManager.Name = apiEntity["wf_name"].ToString();
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Name Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_token"))
                {
                    apiManager.Token = apiEntity.GetAttributeValue<string>("wf_token");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No API Token Found.");
                }

                if (apiEntity.Attributes.ContainsKey("wf_baseurl"))
                {
                    apiManager.AuthUrl = apiEntity["wf_baseurl"].ToString();
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Server URL Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_accountid"))
                {
                    apiManager.AccountId = apiEntity.GetAttributeValue<string>("wf_accountid");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Account id Found.");
                }

                if (apiEntity.Attributes.ContainsKey("wf_accountpwd"))
                {
                    apiManager.AccountPwd = apiEntity.GetAttributeValue<string>("wf_accountpwd");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_AccountPwd Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_dnb_product"))
                {
                    apiManager.ProductName = apiEntity.GetAttributeValue<string>("wf_dnb_product");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_dnb_product Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_producturl"))
                {
                    apiManager.ProductUrl = apiEntity.GetAttributeValue<string>("wf_producturl");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_dnb_product Found.");
                }
                
                return apiManager;
            }

            catch (Exception err)
            {
                // tracingService.Trace("========================================");
                throw new InvalidWorkflowException(tracelog.ToString() + "========================================/n" + err.ToString());
            }

        }

        public string FetchBuildaListBasic(ApiManager apiManager, string searchString, ITracingService tracer)
        {
            
            tracer.Trace("FetchBuildaListAdvanced: Begin.- " + "https://direct.dnb.com/V6.3/organizations?KeywordText=" + searchString + "&SearchModeDescription=Advanced&findcompany=true");

            try
            {
                WebClient client = new WebClient();

                client.Headers.Add("Authorization: " + apiManager.Token);
                byte[] responseBytes = client.DownloadData("https://direct.dnb.com/V6.3/organizations?KeywordText=" + searchString + "&SearchModeDescription=Advanced&findcompany=true");
                string response = Encoding.UTF8.GetString(responseBytes);
                
                tracer.Trace(response);

                return response;
            }
            catch (WebException exception)
            {
                string str = string.Empty;
                if (exception.Response != null)
                {
                    using (StreamReader reader =
                        new StreamReader(exception.Response.GetResponseStream()))
                    {
                        str = reader.ReadToEnd();
                    }
                    exception.Response.Close();
                }
                if (exception.Status == WebExceptionStatus.Timeout)
                {
                    throw new InvalidPluginExecutionException(
                        "The timeout elapsed while attempting to issue the request.", exception);
                }
                throw new InvalidPluginExecutionException(exception.Message);
            }
            catch (Exception e)
            {
                tracer.Trace("FetchBuildaListBasic: Catch. ");
                throw e;
            }

        }

    }
    public sealed class LocateCompanyAdvanced : CodeActivity
    {
        [Input("searchStringAdv")]
        public InArgument<string> searchStringAdv { get; set; }

        [Output("apiResponseAdv")]
        public OutArgument<string> apiResponseAdv { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            
            ITracingService tracer = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                tracer.Trace("BEGIN Execute");
                wf_dnbsearch entity = (wf_dnbsearch)service.Retrieve("wf_dnbsearch", context.PrimaryEntityId, new ColumnSet(true));

                tracer.Trace("Process: EntityId - " + entity.Id.ToString());
                tracer.Trace("Process: Begin - ");
                //retreive CRM apimanager
                ApiManager apiManager = RetrieveApiManager(service, "wf_name", "wf_token", "wf_baseurl", "wf_accountid", "wf_accountpwd", "wf_producturl", "wf_dnb_product");
                tracer.Trace("Process: apiManager.AccountId - " + apiManager.AccountId);
                tracer.Trace("Process: apiManager.AccountPwd - " + apiManager.AccountPwd);
                tracer.Trace("Process: apiManager.AuthUrl - " + apiManager.AuthUrl);
                tracer.Trace("Process: apiManager.Name - " + apiManager.Name);
                tracer.Trace("Process: apiManager.ProductName - " + apiManager.ProductName);
                tracer.Trace("Process: apiManager.ProductUrl - " + apiManager.ProductUrl);
                tracer.Trace("Process: apiManager.Token - " + apiManager.Token);
                tracer.Trace("Process: apiManager.Client.BaseAddress - " + apiManager.Client.BaseAddress);

                Avtex.Xrm.Integrations.wf_orderproductresponse opr = new wf_orderproductresponse();
                
                apiManager.Client.DefaultRequestHeaders.Clear();
                apiManager.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                apiManager.Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiManager.Token);
                tracer.Trace("Process: apiManager.Client.DefaultRequestHeaders created.");
                tracer.Trace("searchStringAdv.Get(executionContext) - " + searchStringAdv.Get(executionContext));
                entity.wf_searchstring = searchStringAdv.Get(executionContext);

                string response = FetchBuildaListAdvanced(apiManager, searchStringAdv.Get(executionContext), tracer);
                
                entity.wf_response = response;
                apiResponseAdv.Set(executionContext, response);

                service.Update(entity);
                }
                catch (Exception e)
                {
                    throw new InvalidPluginExecutionException(e.Message);
                }
            }
    
        public ApiManager RetrieveApiManager(IOrganizationService service, params string[] columns)
        {
            StringBuilder tracelog = new StringBuilder();
            tracelog.AppendLine("BEGIN RetrieveApiManager");
            
            try
            {
                QueryExpression query = new QueryExpression()
                {
                    Distinct = false,
                    EntityName = wf_dnbapiconfiguration.EntityLogicalName,
                    ColumnSet = new ColumnSet(columns),

                };

                //"wf_name", "wf_token", "wf_baseurl", "wf_accountid", "wf_accountpwd", "wf_producturl", "wf_dnb_product");
                Entity apiEntity = service.RetrieveMultiple(query).Entities.FirstOrDefault();

                ApiManager apiManager = new ApiManager();

                apiManager.id = apiEntity.Id;

                if (apiEntity.Attributes.ContainsKey("wf_name"))
                {
                    apiManager.Name = apiEntity["wf_name"].ToString();
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Name Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_token"))
                {
                    apiManager.Token = apiEntity.GetAttributeValue<string>("wf_token");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No API Token Found.");
                }

                if (apiEntity.Attributes.ContainsKey("wf_baseurl"))
                {
                    apiManager.AuthUrl = apiEntity["wf_baseurl"].ToString();
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Server URL Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_accountid"))
                {
                    apiManager.AccountId = apiEntity.GetAttributeValue<string>("wf_accountid");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Account id Found.");
                }

                if (apiEntity.Attributes.ContainsKey("wf_accountpwd"))
                {
                    apiManager.AccountPwd = apiEntity.GetAttributeValue<string>("wf_accountpwd");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_AccountPwd Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_dnb_product"))
                {
                    apiManager.ProductName = apiEntity.GetAttributeValue<string>("wf_dnb_product");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_dnb_product Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_producturl"))
                {
                    apiManager.ProductUrl = apiEntity.GetAttributeValue<string>("wf_producturl");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_dnb_product Found.");
                }
                
                return apiManager;
            }

            catch (Exception err)
            {
                // tracingService.Trace("========================================");
                throw new InvalidWorkflowException(tracelog.ToString() + "========================================/n" + err.ToString());
            }

        }

        public string FetchBuildaListAdvanced(ApiManager apiManager, string searchString, ITracingService tracer)
        {
            //https://direct.dnb.com/V6.1/organizations?RadiusSearchCountryISOAlpha2Code=US&RadiusMeasurementUnitCode=3353&RadiusMeasurement=10&RadiusSearchPostalCode=07869&OrganizationName=Gorman&SearchModeDescription=Advanced&findcompany=true
           
            tracer.Trace("FetchBuildaListAdvanced: Begin.- " + "https://direct.dnb.com/V6.1/organizations?" + searchString + "SearchModeDescription=Advanced&findcompany=true");

            try
            {
                //<snippetWebClientPlugin2>
                // Download the target URI using a Web client.
                WebClient client = new WebClient();
                byte[] data = new byte[0];
                client.Headers.Add("x-dnb-user", apiManager.AccountId);
                client.Headers.Add("x-dnb-pwd", apiManager.AccountPwd);
                byte[] tokenBytes = client.UploadData(apiManager.AuthUrl,data);
                apiManager.Token = client.ResponseHeaders.Get("Authorization"); 
                //</snippetWebClientPlugin2>
                tracer.Trace("Token - " + apiManager.Token);
                client.Headers.Clear();
                client.Headers.Add("Authorization: " + apiManager.Token);
                byte[] responseBytes = client.DownloadData("https://direct.dnb.com/V6.1/organizations?" + searchString + "SearchModeDescription=Advanced&findcompany=true");
                string response = Encoding.UTF8.GetString(responseBytes);
                //</snippetWebClientPlugin2>
                tracer.Trace(response);
                return response;
            }
            catch (WebException exception)
            {
                string str = string.Empty;
                if (exception.Response != null)
                {
                    using (StreamReader reader =
                        new StreamReader(exception.Response.GetResponseStream()))
                    {
                        str = reader.ReadToEnd();
                    }
                    exception.Response.Close();
                    return str;
                }
                if (exception.Status == WebExceptionStatus.Timeout)
                {
                    throw new InvalidPluginExecutionException(
                        "The timeout elapsed while attempting to issue the request.", exception);
                }
                throw new InvalidPluginExecutionException(exception.Message);
            }
            catch (Exception e)
            {
                tracer.Trace("FetchBuildaListAdvanced: Catch. ");
                throw e;
            }
        }       
    }
    public sealed class LocateCompanyMatchBasic : CodeActivity
    {
        [Input("searchStringMatch")]
        public InArgument<string> searchStringMatch { get; set; }

        [Output("apiResponseMatch")]
        public OutArgument<string> apiResponseMatch { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {

            ITracingService tracer = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                tracer.Trace("BEGIN Execute");
                wf_dnbsearch entity = (wf_dnbsearch)service.Retrieve("wf_dnbsearch", context.PrimaryEntityId, new ColumnSet(true));

                tracer.Trace("Process: EntityId - " + entity.Id.ToString());
                tracer.Trace("Process: Begin - ");
                //retreive CRM apimanager
                ApiManager apiManager = RetrieveApiManager(service, "wf_name", "wf_token", "wf_baseurl", "wf_accountid", "wf_accountpwd", "wf_producturl", "wf_dnb_product");
                tracer.Trace("Process: apiManager.AccountId - " + apiManager.AccountId);
                tracer.Trace("Process: apiManager.AccountPwd - " + apiManager.AccountPwd);
                tracer.Trace("Process: apiManager.AuthUrl - " + apiManager.AuthUrl);
                tracer.Trace("Process: apiManager.Name - " + apiManager.Name);
                tracer.Trace("Process: apiManager.ProductName - " + apiManager.ProductName);
                tracer.Trace("Process: apiManager.ProductUrl - " + apiManager.ProductUrl);
                tracer.Trace("Process: apiManager.Token - " + apiManager.Token);
                tracer.Trace("Process: apiManager.Client.BaseAddress - " + apiManager.Client.BaseAddress);
               
                Avtex.Xrm.Integrations.wf_orderproductresponse opr = new wf_orderproductresponse();

                apiManager.Client.DefaultRequestHeaders.Clear();
                apiManager.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                apiManager.Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiManager.Token);
                tracer.Trace("Process: apiManager.Client.DefaultRequestHeaders created. ");
                string response = FetchBuildaListBasic(apiManager, searchStringMatch.Get(executionContext), tracer);

                entity.wf_response = response;
                apiResponseMatch.Set(executionContext, response);

                service.Update(entity);

            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }

        public ApiManager RetrieveApiManager(IOrganizationService service, params string[] columns)
        {
            StringBuilder tracelog = new StringBuilder();
            tracelog.AppendLine("BEGIN RetrieveApiManager");
            //tracingService.Trace("BEGIN RetrieveApiManager.");

            try
            {
                QueryExpression query = new QueryExpression()
                {
                    Distinct = false,
                    EntityName = wf_dnbapiconfiguration.EntityLogicalName,
                    ColumnSet = new ColumnSet(columns),

                };

                //"wf_name", "wf_token", "wf_baseurl", "wf_accountid", "wf_accountpwd", "wf_producturl", "wf_dnb_product");
                Entity apiEntity = service.RetrieveMultiple(query).Entities.FirstOrDefault();

                ApiManager apiManager = new ApiManager();

                apiManager.id = apiEntity.Id;

                if (apiEntity.Attributes.ContainsKey("wf_name"))
                {
                    apiManager.Name = apiEntity["wf_name"].ToString();
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Name Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_token"))
                {
                    apiManager.Token = apiEntity.GetAttributeValue<string>("wf_token");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No API Token Found.");
                }

                if (apiEntity.Attributes.ContainsKey("wf_baseurl"))
                {
                    apiManager.AuthUrl = apiEntity["wf_baseurl"].ToString();
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Server URL Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_accountid"))
                {
                    apiManager.AccountId = apiEntity.GetAttributeValue<string>("wf_accountid");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Account id Found.");
                }

                if (apiEntity.Attributes.ContainsKey("wf_accountpwd"))
                {
                    apiManager.AccountPwd = apiEntity.GetAttributeValue<string>("wf_accountpwd");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_AccountPwd Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_dnb_product"))
                {
                    apiManager.ProductName = apiEntity.GetAttributeValue<string>("wf_dnb_product");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_dnb_product Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_producturl"))
                {
                    apiManager.ProductUrl = apiEntity.GetAttributeValue<string>("wf_producturl");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_dnb_product Found.");
                }
                
                return apiManager;
            }

            catch (Exception err)
            {
                // tracingService.Trace("========================================");
                throw new InvalidWorkflowException(tracelog.ToString() + "========================================/n" + err.ToString());
            }

        }

        public string FetchBuildaListBasic(ApiManager apiManager, string searchString, ITracingService tracer)
        {
            //Example url
        //https://direct.dnb.com/V5.0/organizations?CountryISOAlpha2Code=US&SubjectName=Gorman%20Manufacturing&TerritoryName=CA&cleansematch=true

            tracer.Trace("Fetch Match Basic: Begin.- " + "https://direct.dnb.com/V5.0/organizations?" + searchString + "cleansematch=true");

            try
            {
                WebClient client = new WebClient();

                byte[] data = new byte[0];
                client.Headers.Add("x-dnb-user", apiManager.AccountId);
                client.Headers.Add("x-dnb-pwd", apiManager.AccountPwd);
                byte[] tokenBytes = client.UploadData(apiManager.AuthUrl, data);
                apiManager.Token = client.ResponseHeaders.Get("Authorization");

                tracer.Trace("Token - " + apiManager.Token);
                client.Headers.Clear();
                client.Headers.Add("Authorization: " + apiManager.Token);
                byte[] responseBytes = client.DownloadData("https://direct.dnb.com/V5.0/organizations?" + searchString + "cleansematch=true");
                string response = Encoding.UTF8.GetString(responseBytes);
                //</snippetWebClientPlugin2>
                tracer.Trace(response);

                return response;
            }
            catch (WebException exception)
            {
                string str = string.Empty;
                if (exception.Response != null)
                {
                    using (StreamReader reader =
                        new StreamReader(exception.Response.GetResponseStream()))
                    {
                        str = reader.ReadToEnd();
                    }
                    exception.Response.Close();
                }
                if (exception.Status == WebExceptionStatus.Timeout)
                {
                    throw new InvalidPluginExecutionException(
                        "The timeout elapsed while attempting to issue the request.", exception);
                }
                throw new InvalidPluginExecutionException(exception.Message);
            }
            catch (Exception e)
            {
                tracer.Trace("FetchBuildaListBasic: Catch. ");
                throw e;
            }
        }
    }

    public sealed class GetCompanyProfile : CodeActivity
    {
        [Output("apiResponseDCP")]
        public OutArgument<string> apiResponseDCP { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracer = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                tracer.Trace("BEGIN Execute");
                Account entity = (Account)service.Retrieve("account", context.PrimaryEntityId, new ColumnSet(true));
                tracer.Trace("Process: EntityId - " + entity.Id.ToString());
                Process(entity, service, tracer, executionContext);//REFACTOR

            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
        public void Process(Account company, IOrganizationService service, ITracingService tracer, CodeActivityContext executionContext)
        {
            tracer.Trace("Process: Begin - " );
            //retreive CRM apimanager
            ApiManager apiManager = RetrieveApiManager(service, "wf_name", "wf_token", "wf_baseurl", "wf_accountid", "wf_accountpwd", "wf_producturl", "wf_dnb_product");
            tracer.Trace("Process: apiManager.AccountId - " + apiManager.AccountId);
            tracer.Trace("Process: apiManager.AccountPwd - " + apiManager.AccountPwd);
            tracer.Trace("Process: apiManager.AuthUrl - " + apiManager.AuthUrl);
            tracer.Trace("Process: apiManager.Name - " + apiManager.Name);
            tracer.Trace("Process: apiManager.ProductName - " + apiManager.ProductName);
            tracer.Trace("Process: apiManager.ProductUrl - " + apiManager.ProductUrl);
            tracer.Trace("Process: apiManager.Token - " + apiManager.Token);
            tracer.Trace("Process: apiManager.Client.BaseAddress - " + apiManager.Client.BaseAddress);

            Avtex.Xrm.Integrations.wf_orderproductresponse opr = new wf_orderproductresponse();
            try
            {
                apiManager.Client.DefaultRequestHeaders.Clear();
                apiManager.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                apiManager.Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiManager.Token);
                tracer.Trace("Process: apiManager.Client.DefaultRequestHeaders created. ");
                string response = FetchDCP(apiManager, company.new_DUNS, tracer);

                dynamic json = JsonConvert.DeserializeObject(response);

                //JObject transactionDetail = json.OrderProductResponse.TransactionDetail;
                opr.wf_ServiceTransactionID = json.OrderProductResponse.TransactionDetail.ServiceTransactionID;
                tracer.Trace("wf_ServiceTransactionID - " + opr.wf_ServiceTransactionID);
                opr.wf_TransactionTimestamp = json.OrderProductResponse.TransactionDetail.TransactionTimestamp;
                tracer.Trace("wf_TransactionTimestamp - " + opr.wf_TransactionTimestamp);


                //JObject transactionResult = json.OrderProductResponse.TransactionResult;
                opr.wf_SeverityText = json.OrderProductResponse.TransactionResult.SeverityText;
                opr.wf_ResultID = json.OrderProductResponse.TransactionResult.ResultID;
                opr.wf_ResultText = json.OrderProductResponse.TransactionResult.ResultText;
                tracer.Trace("wf_ResultText - " + opr.wf_ResultText);

                //JObject inquiryDetail = json.OrderProductResponse.OrderProductResponseDetail.InquiryDetail;
                opr.wf_name = json.OrderProductResponse.OrderProductResponseDetail.InquiryDetail.DUNSNumber;
                opr.wf_DUNSNumber = json.OrderProductResponse.OrderProductResponseDetail.InquiryDetail.DUNSNumber;
                company.new_DUNS = json.OrderProductResponse.OrderProductResponseDetail.InquiryDetail.DUNSNumber;
                opr.wf_CountryISOAlpha2Code = json.OrderProductResponse.OrderProductResponseDetail.InquiryDetail.CountryISOAlpha2Code;
               // company.new_Country = json.OrderProductResponse.OrderProductResponseDetail.InquiryDetail.CountryISOAlpha2Code;
                //JObject product = json.OrderProductResponse.OrderProductResponseDetail.Product;
                opr.wf_DNBProductID = json.OrderProductResponse.OrderProductResponseDetail.Product.DNBProductID;
                tracer.Trace(".wf_DNBProductID - " + opr.wf_DNBProductID);
                //JObject product = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.SubjectHeader;
                opr.wf_OrganizationSummaryText = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.SubjectHeader.OrganizationSummaryText;
                company.Description =  json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.SubjectHeader.OrganizationSummaryText;
                //JObject d = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.SubjectHeader.LastUpdateDate;
                //opr.wf_LastUpdateDate = DateTime.Parse(d.GetValue("$").ToString());


                opr.wf_APIResponse = response;
                opr.wf_name = company.new_DUNS + "- " + DateTime.Today;

                service.Create(opr);
                service.Update(company);

                apiResponseDCP.Set(executionContext, response);
                //throw new InvalidPluginExecutionException("The Detailed Company Profile (DCP_PREM) has imported successfully.");
               
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
        public ApiManager RetrieveApiManager(IOrganizationService service, params string[] columns)
        {
            StringBuilder tracelog = new StringBuilder();
            tracelog.AppendLine("BEGIN RetrieveApiManager");
            

            try
            {
                QueryExpression query = new QueryExpression()
                {
                    Distinct = false,
                    EntityName = wf_dnbapiconfiguration.EntityLogicalName,
                    ColumnSet = new ColumnSet(columns),

                };

                //"wf_name", "wf_token", "wf_baseurl", "wf_accountid", "wf_accountpwd", "wf_producturl", "wf_dnb_product");
                Entity apiEntity = service.RetrieveMultiple(query).Entities.FirstOrDefault();

                ApiManager apiManager = new ApiManager();

                apiManager.id = apiEntity.Id;

                if (apiEntity.Attributes.ContainsKey("wf_name"))
                {
                    apiManager.Name = apiEntity["wf_name"].ToString();
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Name Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_token"))
                {
                    apiManager.Token = apiEntity.GetAttributeValue<string>("wf_token");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No API Token Found.");
                }

                if (apiEntity.Attributes.ContainsKey("wf_baseurl"))
                {
                    apiManager.AuthUrl = apiEntity["wf_baseurl"].ToString();
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Server URL Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_accountid"))
                {
                    apiManager.AccountId = apiEntity.GetAttributeValue<string>("wf_accountid");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Account id Found.");
                }
                
                if (apiEntity.Attributes.ContainsKey("wf_accountpwd"))
                {
                    apiManager.AccountPwd = apiEntity.GetAttributeValue<string>("wf_accountpwd");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_AccountPwd Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_dnb_product"))
                {
                    apiManager.ProductName = apiEntity.GetAttributeValue<string>("wf_dnb_product");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_dnb_product Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_producturl"))
                {
                    apiManager.ProductUrl = apiEntity.GetAttributeValue<string>("wf_producturl");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_dnb_product Found.");
                }
                
                return apiManager;
            }

            catch (Exception err)
            {
                // tracingService.Trace("========================================");
                throw new InvalidWorkflowException(tracelog.ToString() + "========================================/n" + err.ToString());
            }

        }
        public string FetchDCP(ApiManager apiManager, string dunsNumber, ITracingService tracer)
        {
           
            tracer.Trace(" FetchDCP: Begin.- " + "https://direct.dnb.com/V5.3/organizations/" + dunsNumber.Trim() + "/products/DCP_PREM");

            try
            {
                //<snippetWebClientPlugin2>
                // Download the target URI using a Web client. 
                WebClient client = new WebClient();
                   
                        client.Headers.Add("Authorization: " + apiManager.Token);
                        byte[] responseBytes = client.DownloadData("https://direct.dnb.com/V5.3/organizations/"+ dunsNumber.Trim() + "/products/DCP_PREM");
                        string response = Encoding.UTF8.GetString(responseBytes);
                        //</snippetWebClientPlugin2>
                        tracer.Trace(response);                  
                        return response;
            }
            catch (WebException exception)
            {
                string str = string.Empty;
                if (exception.Response != null)
                {
                    using (StreamReader reader =
                        new StreamReader(exception.Response.GetResponseStream()))
                    {
                        str = reader.ReadToEnd();
                    }
                    exception.Response.Close();
                }
                if (exception.Status == WebExceptionStatus.Timeout)
                {
                    throw new InvalidPluginExecutionException(
                        "The timeout elapsed while attempting to issue the request.", exception);
                }
                throw new InvalidPluginExecutionException(exception.Message);
            }
            catch (Exception e)
            {
                tracer.Trace(" FetchDCP: Catch. ");
                throw e;
            }
        }
    }

    public sealed class GetCompanyProfileForImport : CodeActivity
    {

        [Input("dunsNumber")]
        public InArgument<string> dunsNumber { get; set; }
        [Output("apiResponseDCP")]
        public OutArgument<string> apiResponseDCP { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracer = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                tracer.Trace("BEGIN Execute");
                wf_orderproductresponse entity = new wf_orderproductresponse();
                entity.Id = Guid.NewGuid();
                
                Process(entity, service, tracer, executionContext);

            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message );
            }
        }
        public void Process(wf_orderproductresponse opr, IOrganizationService service, ITracingService tracer, CodeActivityContext executionContext)
        {
            tracer.Trace("Process: Begin - ");
            //retreive CRM apimanager
            ApiManager apiManager = RetrieveApiManager(service, "wf_name", "wf_token", "wf_baseurl", "wf_accountid", "wf_accountpwd", "wf_producturl", "wf_dnb_product");
            tracer.Trace("Process: apiManager.AccountId - " + apiManager.AccountId);
            tracer.Trace("Process: apiManager.AccountPwd - " + apiManager.AccountPwd);
            tracer.Trace("Process: apiManager.AuthUrl - " + apiManager.AuthUrl);
            tracer.Trace("Process: apiManager.Name - " + apiManager.Name);
            tracer.Trace("Process: apiManager.ProductName - " + apiManager.ProductName);
            tracer.Trace("Process: apiManager.ProductUrl - " + apiManager.ProductUrl);
            tracer.Trace("Process: apiManager.Token - " + apiManager.Token);
            tracer.Trace("Process: apiManager.Client.BaseAddress - " + apiManager.Client.BaseAddress);

            try
            {
                WebClient client = new WebClient();

                byte[] data = new byte[0];
                client.Headers.Add("x-dnb-user", apiManager.AccountId);
                client.Headers.Add("x-dnb-pwd", apiManager.AccountPwd);
                byte[] tokenBytes = client.UploadData(apiManager.AuthUrl, data);
                apiManager.Token = client.ResponseHeaders.Get("Authorization");

                tracer.Trace("Token - " + apiManager.Token);
                apiManager.Client.DefaultRequestHeaders.Clear();
                apiManager.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                apiManager.Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiManager.Token);
                tracer.Trace("Process: apiManager.Client.DefaultRequestHeaders created. ");

                string duns = dunsNumber.Get(executionContext);
                if (duns != null){
                    
                    duns = duns.Replace("'", "");
                    duns = duns.Trim();
                }
                else{
                    throw new InvalidPluginExecutionException("Invalid DUNS Number . Query aborted.");
                }
                string response = FetchDCP(apiManager, duns, tracer);

                dynamic json = JsonConvert.DeserializeObject(response);

                opr.wf_ServiceTransactionID = json.OrderProductResponse.TransactionDetail.ServiceTransactionID;
                opr.wf_TransactionTimestamp = json.OrderProductResponse.TransactionDetail.TransactionTimestamp;
                //opr.wf_ServiceVersionNumber = orderProductResponseItem.ServiceVersionNumber;

                opr.wf_SeverityText = json.OrderProductResponse.TransactionResult.SeverityText;
                opr.wf_ResultID = json.OrderProductResponse.TransactionResult.ResultID;
                opr.wf_ResultText = json.OrderProductResponse.TransactionResult.ResultText;

                opr.wf_name = json.OrderProductResponse.OrderProductResponseDetail.InquiryDetail.DUNSNumber;
                opr.wf_DUNSNumber = json.OrderProductResponse.OrderProductResponseDetail.InquiryDetail.DUNSNumber;
                opr.wf_CountryISOAlpha2Code = json.OrderProductResponse.OrderProductResponseDetail.InquiryDetail.CountryISOAlpha2Code;

                opr.wf_DNBProductID = json.OrderProductResponse.OrderProductResponseDetail.Product.DNBProductID;

                opr.wf_OrganizationSummaryText = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.SubjectHeader.OrganizationSummaryText;
                JObject d = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.SubjectHeader.LastUpdateDate;
                //opr.wf_LastUpdateDate = DateTime.Parse(d.GetValue("$").ToString());              

                JObject e = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.OrganizationDetail.ControlOwnershipDate;
                if (e != null)
                {
                    opr.wf_ControlOwnershipDate = e.GetValue("$").ToString();
                }
                JObject f = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.OrganizationDetail.ControlOwnershipTypeText;
                if (f != null)
                {
                    opr.wf_ControlOwnershipTypeText = f.GetValue("$").ToString();
                    //opr.wf_LegalFormText = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.OrganizationDetail.RegisteredDetail.LegalFormDetails.LegalFormText;
                }
                JObject g = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.Financial.KeyFinancialFiguresOverview[0].StatementHeaderDetails;
                if (g != null)
                {
                    opr.wf_FinancialPeriodDuration = g.GetValue("FinancialPeriodDuration").ToString();
                }
                JObject h = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.Financial.KeyFinancialFiguresOverview[0].StatementHeaderDetails.FinancialStatementToDate;

                JObject i = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.Financial.KeyFinancialFiguresOverview[0].SalesRevenueAmount[0];
                if (i != null)
                {
                    opr.wf_SalesRevenueAmount = new Money(decimal.Parse(i.GetValue("$").ToString()));
                    opr.wf_CurrencyISOAlpha3Code = i.GetValue("@CurrencyISOAlpha3Code").ToString();
                    opr.wf_ReliabilityText = i.GetValue("@ReliabilityText").ToString();
                }

                JObject j = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.OrganizationName.OrganizationPrimaryName[0].OrganizationName;
                if (j != null)
                {
                    opr.wf_OrganizationName = j.GetValue("$").ToString();
                }
                opr.wf_OrganizationStartYear = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.OrganizationDetail.OrganizationStartYear;

                JObject k = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.Telecommunication.TelephoneNumber[0];
                if (k != null)
                {
                    opr.wf_InternationalDialingCode = k.GetValue("InternationalDialingCode").ToString();
                    opr.wf_TelecommunicationNumber = k.GetValue("TelecommunicationNumber").ToString();
                }
                JObject m = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.Location.PrimaryAddress[0].StreetAddressLine[0];
                if (m != null)
                {
                    JToken mtoken = m["LineText"];
                    if (mtoken != null)
                    {
                        opr.wf_StreetAddressLine = m.GetValue("LineText").ToString();
                    }
                }
                JObject l = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.Location.PrimaryAddress[0];
                if (l != null)
                {
                    JToken ltoken = l["PrimaryTownName"];
                    if (ltoken != null)
                    {
                        opr.wf_PrimaryTownName = l.GetValue("PrimaryTownName").ToString();
                    }
                    ltoken = null;
                    ltoken = l["TerritoryAbbreviatedName"];
                    if (ltoken != null)
                    {
                        opr.wf_TerritoryAbbreviatedName = l.GetValue("TerritoryAbbreviatedName").ToString();
                    }
                    ltoken = null;
                    ltoken = l["TerritoryOfficialName"];
                    if (ltoken != null)
                    {
                        opr.wf_TerritoryOfficialName = l.GetValue("TerritoryOfficialName").ToString();
                    }
                    ltoken = l["PostalCode"];
                    if (ltoken != null)
                    {
                        opr.wf_PostalCode = l.GetValue("PostalCode").ToString();
                    }
                    ltoken = null;
                    ltoken = l["CountryGroupName"];
                    if (ltoken != null)
                    {
                        opr.wf_CountryGroupName = l.GetValue("CountryGroupName").ToString();
                    }
                    ltoken = null;
                    ltoken = l["CountyOfficialName"];
                    if (ltoken != null)
                    {
                        opr.wf_CountyOfficialName = l.GetValue("CountyOfficialName").ToString();
                    }
                }


                opr.wf_APIResponse = response;
                opr.wf_name = dunsNumber.Get(executionContext) + "- " + DateTime.Today;              

                service.Create(opr);
               
                Account c = new Account();
                c.Id = Guid.NewGuid();
                c.Name = opr.wf_OrganizationName;
                c.Address1_City = opr.wf_PrimaryTownName;
                c.Address1_Country = opr.wf_CountryISOAlpha2Code;
                c.Address1_County = opr.wf_CountyOfficialName;
                c.Address1_Line1 = opr.wf_StreetAddressLine;
                c.Address1_PostalCode = opr.wf_PostalCode;
                c.Address1_StateOrProvince = opr.wf_TerritoryOfficialName;
                c.Address1_Telephone1 = opr.wf_TelecommunicationNumber;
                c.Description = opr.wf_OrganizationSummaryText;
                
                RegionInfo ri = new RegionInfo(opr.wf_CountryISOAlpha2Code);
                if (ri != null)//TODO
                {
                   //c.new_Country = Helpers.getOptionSetValue(service, c.LogicalName, "new_Country", ri.EnglishName);
                   //c.wf_Country = Helpers.getOptionSetValue(service, c.LogicalName, "wf_Country", ri.EnglishName);
                    
                }
                c.NumberOfEmployees = json.OrderProductResponse.OrderProductResponseDetail.Product.Organization.EmployeeFigures.ConsolidatedEmployeeDetails.TotalEmployeeQuantity;
                c.Telephone1 = opr.wf_TelecommunicationNumber;
                c.wf_AnnualSales = opr.wf_SalesRevenueAmount;
                c.wf_DUNS = opr.wf_DUNSNumber;
                c.new_DUNS = opr.wf_DUNSNumber;
                EntityReference er = new EntityReference("wf_orderproductresponse", opr.Id);
                c.wf_DunsNumberId = er;
                service.Create(c);

                apiResponseDCP.Set(executionContext, response);
               
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message );
            }
        }
        public ApiManager RetrieveApiManager(IOrganizationService service, params string[] columns)
        {
            StringBuilder tracelog = new StringBuilder();
            tracelog.AppendLine("BEGIN RetrieveApiManager");
          
            try
            {
                QueryExpression query = new QueryExpression()
                {
                    Distinct = false,
                    EntityName = wf_dnbapiconfiguration.EntityLogicalName,
                    ColumnSet = new ColumnSet(columns),
                };

                //"wf_name", "wf_token", "wf_baseurl", "wf_accountid", "wf_accountpwd", "wf_producturl", "wf_dnb_product");
                Entity apiEntity = service.RetrieveMultiple(query).Entities.FirstOrDefault();

                ApiManager apiManager = new ApiManager();

                apiManager.id = apiEntity.Id;

                if (apiEntity.Attributes.ContainsKey("wf_name"))
                {
                    apiManager.Name = apiEntity["wf_name"].ToString();
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Name Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_token"))
                {
                    apiManager.Token = apiEntity.GetAttributeValue<string>("wf_token");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No API Token Found.");
                }

                if (apiEntity.Attributes.ContainsKey("wf_baseurl"))
                {
                    apiManager.AuthUrl = apiEntity["wf_baseurl"].ToString();
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Server URL Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_accountid"))
                {
                    apiManager.AccountId = apiEntity.GetAttributeValue<string>("wf_accountid");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No Account id Found.");
                }

                if (apiEntity.Attributes.ContainsKey("wf_accountpwd"))
                {
                    apiManager.AccountPwd = apiEntity.GetAttributeValue<string>("wf_accountpwd");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_AccountPwd Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_dnb_product"))
                {
                    apiManager.ProductName = apiEntity.GetAttributeValue<string>("wf_dnb_product");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_dnb_product Found.");
                }
                if (apiEntity.Attributes.ContainsKey("wf_producturl"))
                {
                    apiManager.ProductUrl = apiEntity.GetAttributeValue<string>("wf_producturl");
                }
                else
                {
                    throw new InvalidWorkflowException("ApiManager: No wf_dnb_product Found.");
                }                

                return apiManager;
            }

            catch (Exception err)
            {
                // tracingService.Trace("========================================");
                throw new InvalidWorkflowException(tracelog.ToString() + "========================================/n" + err.ToString());
            }

        }
        public string FetchDCP(ApiManager apiManager, string dunsNumber, ITracingService tracer)
        {
           
            tracer.Trace(" FetchDCP: Begin.- " + "https://direct.dnb.com/V5.3/organizations/" + dunsNumber.Trim() + "/products/DCP_PREM");

            try
            {
                //<snippetWebClientPlugin2>
                // Download the target URI using a Web client.
                WebClient client = new WebClient();
                client.Headers.Clear();
                client.Headers.Add("Authorization: " + apiManager.Token);
                byte[] responseBytes = client.DownloadData("https://direct.dnb.com/V5.3/organizations/" + dunsNumber.Trim() + "/products/DCP_PREM");
                string response = Encoding.UTF8.GetString(responseBytes);
                //</snippetWebClientPlugin2>
                tracer.Trace(response);
                return response;
            }
            catch (WebException exception)
            {
                string str = string.Empty;
                if (exception.Response != null)
                {
                    using (StreamReader reader =
                        new StreamReader(exception.Response.GetResponseStream()))
                    {
                        str = reader.ReadToEnd();
                    }
                    exception.Response.Close();
                }
                if (exception.Status == WebExceptionStatus.Timeout)
                {
                    throw new InvalidPluginExecutionException(
                        "The timeout elapsed while attempting to issue the request.", exception);
                }
                throw new InvalidPluginExecutionException(exception.Message + " TRACE -" + tracer.ToString());
            }
            catch (Exception e)
            {
                tracer.Trace(" FetchDCP: Catch. ");
                throw e;
            }
        }
    }
}
