using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Newtonsoft.Json;
using Taxjar;

namespace CRM.Workflows.Helpers
{
    class HelperClass
    {
        public static bool ValidPostalCode(IOrganizationService service, string postalcode)
        {
            bool isValid = false;

            // run address validation call to verify zip/postal code - either regex or service call
            isValid = true;     // setting to true in lieu of making call. replace with actual code
            
            return isValid;
        }

        public static decimal getTaxRate(IOrganizationService service, ITracingService trace, string postalcode, string taxProvider)
        {
            decimal taxRate = new decimal(0.0000);
            string url = string.Empty;
            string key = string.Empty;

            // get Service API URL and API Key from CRM Settings
            // new_setting entity in CRM needs to be in place with the following fields
                // name - string - contains the name of the tax service (e.g. "TaxJar", "Avalara", "CCH")
                // new_value1 - string - contains the base url of the tax service
                // new_value2 - string - contains the api key for the tax service
            // these are named "new_value1" instead of descriptive, so that the settings entity can be used for different settings types

            StringBuilder settingsReq = new StringBuilder();
            settingsReq.Append("<fetch top='1'>");
            settingsReq.Append("<entity name='new_setting' >");
            settingsReq.Append("<attribute name = 'name' /> ");
            settingsReq.Append("<attribute name = 'new_value1' /> ");
            settingsReq.Append("<attribute name = 'new_value2' /> ");
            settingsReq.Append("<filter type='and' >");
            settingsReq.Append("<condition attribute='name' operator='eq' value='" + taxProvider + "' />");
            settingsReq.Append("</filter>");
            settingsReq.Append("</entity>");
            settingsReq.Append("</fetch>");

            trace.Trace("Get Settings from CRM");
            EntityCollection settingsReqColl = service.RetrieveMultiple(new FetchExpression(settingsReq.ToString()));
            if (settingsReqColl == null || settingsReqColl.Entities == null || settingsReqColl.Entities.Count == 0)
            {
                // Default  to TaxJar if no Setting is found
                // full Url = "https://api.taxjar.com/v2/rates/{zipcode}?api_key='5da2f821eee4035db4771edab942a4cc'"
                url = "https://api.taxjar.com/v2/rates";
                key = "?api_key='5da2f821eee4035db4771edab942a4cc'";
                trace.Trace("No Tax Settings found in CRM");
            }
            else
            {
                foreach (Entity settingsRef in settingsReqColl.Entities)
                {
                    url = settingsRef["new_value1"].ToString();
                    key = settingsRef["new_value2"].ToString();
                }
                trace.Trace("Successfully retrieved Settings");
            }

            // Create service call
            if (!(string.IsNullOrWhiteSpace(url) && (string.IsNullOrWhiteSpace(key))))
            {
                HttpClient client = new HttpClient { BaseAddress = new Uri(url) };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync(key).Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonRates = response.Content.ReadAsStringAsync().ToString();
                    Rate rates = JsonConvert.DeserializeObject<Rate>(jsonRates);
                    taxRate = new decimal(rates.combined_rate);
                    trace.Trace(string.Format("Succesfully found rate of {0}", taxRate.ToString()));
                }
                else
                {
                    trace.Trace("No Rate Returned");
                }
            }
            return taxRate;
        }

        public class Rate
        {
            public string zip { get; set; }
            public string country { get; set; }
            public float country_rate { get; set; }
            public string state { get; set; }
            public float state_rate { get; set; }
            public string county { get; set; }
            public float county_rate { get; set; }
            public string city { get; set; }
            public float city_rate { get; set; }
            public float combined_district_rate { get; set; }
            public float combined_rate { get; set; }
            public float freight_taxable { get; set; }
        }
    }
}
