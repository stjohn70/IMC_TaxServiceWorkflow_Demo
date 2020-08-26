using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Taxjar;

namespace CRM.Workflows.Helpers
{
    class HelperClass
    {
        public static decimal GetServiceTaxRate(IOrganizationService service, string postalcode)
        {
            decimal taxRate = new decimal(0.0000);

            var client = new TaxjarApi("5da2f821eee4035db4771edab942a4cc");

            var rates = client.RatesForLocation(postalcode);
            taxRate = rates.CombinedRate;

            return taxRate;
        }

        public static bool ValidPostalCode(IOrganizationService service, string postalcode)
        {
            bool isValid = false;

            // run address validation call to verify zip/postal code - either regex or service call
            isValid = true;     // setting to true in lieu of making call. replace with actual code
            
            return isValid;
        }
    }
}
