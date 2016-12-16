using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
namespace Avtex.Xrm.Integrations
{
    public class Helpers
    {
        public static OptionSetValue getOptionSetValue(IOrganizationService service, string entityName, string attributeName, string optionsetText)
        {
            OptionSetValue optionSetValue = new OptionSetValue();
            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest();
            retrieveAttributeRequest.EntityLogicalName = entityName;
            retrieveAttributeRequest.LogicalName = attributeName;
            retrieveAttributeRequest.RetrieveAsIfPublished = true;

            try
            {
                RetrieveAttributeResponse retrieveAttributeResponse =
                  (RetrieveAttributeResponse)service.Execute(retrieveAttributeRequest);
                PicklistAttributeMetadata picklistAttributeMetadata =
                  (PicklistAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

                OptionSetMetadata optionsetMetadata = picklistAttributeMetadata.OptionSet;

                foreach (OptionMetadata optionMetadata in optionsetMetadata.Options)
                {
                    if (optionMetadata.Label.UserLocalizedLabel.Label.ToLower() == optionsetText.ToLower())
                    {
                        optionSetValue.Value = optionMetadata.Value.Value;
                        return optionSetValue;
                    }

                }
                return optionSetValue;
            }
            catch (Exception)
            { return optionSetValue; }
        }

        public static string getOptionSetText(IOrganizationService service, string entityName, string attributeName, int optionsetValue)
        {
            string optionsetText = string.Empty;
            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest();
            retrieveAttributeRequest.EntityLogicalName = entityName;
            retrieveAttributeRequest.LogicalName = attributeName;
            retrieveAttributeRequest.RetrieveAsIfPublished = true;
            try
            {
                RetrieveAttributeResponse retrieveAttributeResponse =
                  (RetrieveAttributeResponse)service.Execute(retrieveAttributeRequest);
                PicklistAttributeMetadata picklistAttributeMetadata =
                  (PicklistAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

                OptionSetMetadata optionsetMetadata = picklistAttributeMetadata.OptionSet;

                foreach (OptionMetadata optionMetadata in optionsetMetadata.Options)
                {
                    if (optionMetadata.Value == optionsetValue)
                    {
                        optionsetText = optionMetadata.Label.UserLocalizedLabel.Label;
                        return optionsetText;
                    }

                }
                return optionsetText;
            }
            catch (Exception)
            { return null; }
        }



    }
}
