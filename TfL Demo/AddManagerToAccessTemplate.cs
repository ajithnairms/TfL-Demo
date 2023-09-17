using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace TfL_Demo
{
    public class AddManagerToAccessTemplate : IPlugin
    {
        private string accessTeam { get; set; }

        public AddManagerToAccessTemplate(string unsecureConfig, string secureConfig)
        {
            this.accessTeam = unsecureConfig;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context == null)
                return;

            if (!(context.MessageName == "Create" && context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity))
                return;

            Entity taskRecord = (Entity)context.InputParameters["Target"];
            
            if (!taskRecord.Contains("regardingobjectid"))
                return;

            EntityReference caseReference = taskRecord.GetAttributeValue<EntityReference>("regardingobjectid");

            if (caseReference.LogicalName == "incident")
            {
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.
                GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                QueryExpression query = new QueryExpression("teamtemplate");
                query.Criteria.AddCondition("teamtemplatename", ConditionOperator.Equal, this.accessTeam);
                query.ColumnSet.AddColumn("teamtemplatename");
                query.TopCount = 1;
                EntityCollection records = service.RetrieveMultiple(query);

                if (records.Entities.Count > 0)
                {
                    Entity caseRecord = service.Retrieve("incident", caseReference.Id, new ColumnSet("tfl_casemanager"));
                    foreach (Entity accessTeam in records.Entities)
                    {
                        AddUserToRecordTeamRequest addUserToRecordTeam = new AddUserToRecordTeamRequest()
                        {
                            Record = new EntityReference("task", taskRecord.Id),
                            SystemUserId = caseRecord.GetAttributeValue<EntityReference>("tfl_casemanager").Id,
                            TeamTemplateId = (Guid)accessTeam.Attributes["teamtemplateid"]
                        };
                        service.Execute(addUserToRecordTeam);
                    }

                }
            }
        }
    }
}
