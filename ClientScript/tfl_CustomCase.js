var tfl_customcase = window.tfl_customcase || {};

(function () {
    this.disableFields = function (executionContext) {
        try {
            var formContext = executionContext.getFormContext();
            var isEscalationAttribute = formContext.getAttribute("isescalated");
            var isConfidentialAttribute = formContext.getAttribute("tfl_isconfidential");
            var addInfoForTask = formContext.getAttribute("tfl_additionalinformationforteam");
            var followupby = formContext.getAttribute("followupby");
            var caseStage = formContext.getAttribute("incidentstagecode");

            if ((isEscalationAttribute.getValue() || isConfidentialAttribute.getValue()) &&
                caseStage.getText() === "Assigned to Manager") {
                addInfoForTask.setRequiredLevel("required");
                followupby.setRequiredLevel("required");
            }
            else {
                addInfoForTask.setRequiredLevel("none");
                followupby.setRequiredLevel("none");
            }
        }
        catch (ex) { console.log("error occured - ", ex.message); }
    }
}).call(tfl_customcase);