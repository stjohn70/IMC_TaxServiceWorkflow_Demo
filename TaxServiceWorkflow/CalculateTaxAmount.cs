using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using CRM.Workflows.Helpers;

namespace CRM.Workflows
{
     public sealed class CalculateTaxAmount : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                trace = this.GetTracingService(context);
                this.ExecuteBody(context);
                trace.Trace("Executed body");
            }
            catch (Exception ex)
            {
                if (trace != null)
                    trace.Trace(String.Format("{0} - {1}", LogMessage, ex.Message));
                Message.Set(context, String.Format("{0} - {1}", LogMessage, ex.Message));
                IsSuccess.Set(context, false);
            }
        }
        public void ExecuteBody(CodeActivityContext context)
        {
            LogMessage = "Get Service and In Args";
            var service = GetService(context);
            var postalCode = postalcode.Get(context);
            var taxableAmount = taxableamount.Get(context);
            decimal taxRate = new decimal(0.0000);
            decimal taxAmount = new decimal(0.0000);
            string returnMessage = string.Empty;
            bool isSuccess = false;

            LogMessage = "Check for valid postal code";
            if (HelperClass.ValidPostalCode(service, postalCode))
            {
                LogMessage = "Get TaxRate from service and calculate TaxAmount";
                taxRate = HelperClass.GetServiceTaxRate(service, postalCode);
                taxAmount = taxRate * taxableAmount;
                // would normally pull in entire Order entity and run based on header and line item info
            }
            else
            {
                returnMessage = "Invalid Postal Code value or format";
            }

            LogMessage = "Set Out Params";
            TaxAmount.Set(context, taxAmount);
            Message.Set(context, returnMessage);
            IsSuccess.Set(context, isSuccess);
        }


        #region Private Prop
        public ITracingService trace;

        private string _message = String.Empty;

        public string LogMessage
        {
            get
            {
                return _message;
            }
            set
            {
                trace.Trace(value);
                _message = String.Format("{0} // {1}", _message, value);
            }
        }
        #endregion Private prop
        #region In Params

        [RequiredArgument]
        [Input("PostalCode")]
        public InArgument<string> postalcode { get; set; }

        [RequiredArgument]
        [Input("TaxableAmount")]
        public InArgument<decimal> taxableamount { get; set; }

        #endregion In Params
        #region Out Params

        [Output("TaxAmount")]
        public OutArgument<decimal> TaxAmount { get; set; }

        [Output("Message")]
        public OutArgument<string> Message { get; set; }

        [Output("IsSuccess")]
        public OutArgument<bool> IsSuccess { get; set; }
        #endregion Out Params
        #region Constructors
        public IOrganizationService GetService(ActivityContext executionContext, Guid? user)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            var svcFactory = executionContext.GetExtension<IOrganizationServiceFactory>();

            if (user == null)
            {
                var workflowContext = executionContext.GetExtension<IWorkflowContext>();
                return svcFactory.CreateOrganizationService(workflowContext.UserId);
            }
            else
            {
                return svcFactory.CreateOrganizationService(user);
            }
        }

        public IOrganizationService GetService(CodeActivityContext executionContext)
        {
            return this.GetService(executionContext, null);
        }

        public ITracingService GetTracingService(ActivityContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }

            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            return tracingService;
        }
        #endregion Constructors
    }
}
