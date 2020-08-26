An extremely simple version of a tax calculator. This uses postal codes and amounts. Full implementation would leverage full order tax calculation and use multiple tax rates and tax rate providers.

This package compiles into TaxServiceWorkflow.dll   
Deploy to D365 CE using Plugin Registration Tool

Callable methods as Custom Workflow Activities from within a workflow are:   
Crm.Workflows.CalculateTaxRate   
* Param PostalCode - string value of the Postal Code to calculate the Tax Rate   
* Response TaxRate - decimal value of the Tax Rate   
* Response Message - string value of a message explaining errors   
* Response IsSuccess - boolean value of whether the operation was successful or not   

Crm.Workflows.CalculateTaxAmount   
* Param PostalCode - string value of the Postal Code to calculate the tax rate   
* Param TaxableAmount - decimal value of the amount to be taxed   
* Response TaxAmount - decimal value of the Tax Amount   
* Response Message - string value of a message explaining errors   
* Response IsSuccess - boolean value of whether the operation was successful or not   
