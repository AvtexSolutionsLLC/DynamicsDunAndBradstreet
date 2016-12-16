# Dynamics Dun & Bradstreet Integration
### Overview
This project provides an integration between Microsoft Dynamics CRM (2016) and the Dun & Bradstreet Direct 2.0 API. It can be used to find a company DUNS number using the Cleanse/Match and advanced Build-A-List search criteria. Using the returned DUNS number, a detailed company profile may be imported into CRM as a record and associated with an account.
### Requirements
In order to use this Dynamics solution, you must have a subscription license with Dun & Bradstreet that includes the following D&B Direct 2.0 API service products:
* On-Demand Single Entity Resolution (Cleanse, then Match)* Detailed Search & Build-a-List - Company/Industry* Detailed Company Profile (Premium Level)
### Get Started
Start by downloading and importing the provided Dynamics solution.
The solution will provide you with the required entities, web resources, views, and search dashboard for finding companies and saving the D&B detailed company profile to CRM.
*Custom Entities:*
D&B – Configurations
D&B – Detailed Company Profile
D&B – Saved Search
*Set Up*
Before you can begin searching for companies a required configuration entity must be created. This entity will store D&B connection and user subscription data needed by CRM.
To create the configuration entity, navigate to Settings > Extensions from the CRM ribbon.
Select New from the Active Configurations view, a blank configuration entity will appear.
Enter a Name for the configuration in the “Name” field. This can be any name you select to help you identify the configuration.
Next set the “Account ID” and “Account Pwd” fields to the subscription id and password used when you created your D&B account.
Retrieve the D&B authentication url from the [D&B Direct API](https://direct.dnb.com/rest/Authentication) and enter the value into the “Authentication Url” field.
Next enter the D&B product code ‘DCP PREM’ (Detailed Company Profile) into the “DNB Product Code” field. Finally, add the base api url from [D&B](https://direct.dnb.com/)
