// Call the Advanced Search action

$scope.locateCompanyAdvanced = function () {

    var Id = "770C630E-1292-E611-80DC-C4346BACBF10";
    var serverURL = window.parent.Xrm.Page.context.getClientUrl();
    var data = {
        searchString: $scope.searchdata        
    }
    var req = new XMLHttpRequest();
    req.open("POST",serverURL + "/api/data/v8.1/wf_dnbsearchs(" + Id + ")/Microsoft.Dynamics.CRM.new_companylocatecompanyadvanced", true);
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.onreadystatechange = function () {
        if (this.readyState == 4 /* complete */) {
            req.onreadystatechange = null;
            if (this.status == 200) {
                //var data = JSON.parse(this.response);
                apiResponse = this.response;
                $scope.results = apiResponse.data.OrderProductResponse.OrderProductResponseDetail.Product.Organization;
                $scope.organizationName = $scope.results.OrganizationName.OrganizationPrimaryName[0].OrganizationName.$;
                $scope.primaryAddress = $scope.results.Location.PrimaryAddress[0];
                $scope.gridOptions = {
                    enableRowSelection: true,
                    selectionRowHeaderWidth: 35,
                    rowHeight: 35,
                    showGridFooter: true,
                    columnDefs: [
                      { name: 'Name', field: 'organization' },
                      { name: 'DUNS Number', field: 'duns' },
                      { name: 'City', field: 'city' },
                      { name: 'Country', field: 'country' },
                      { name: 'Summary', field: 'summary' },
                      { name: 'Last Updated', field: 'last-updated' }
                    ],
                    data: [{
                        "organization": $scope.organizationName,
                        "duns": $scope.results.SubjectHeader.DUNSNumber,
                        "city": $scope.primaryAddress.PrimaryTownName,
                        "country": $scope.primaryAddress.CountryISOAlpha2Code,
                        "summary": $scope.results.SubjectHeader.OrganizationSummaryText,
                        "last-updated": $scope.results.SubjectHeader.LastUpdateDate.$
                    }
                    ]

                };
                $scope.searchdata = "";
                
            }
        };
 
    }
    req.send(window.JSON.stringify(data));
}



[Organization URI]/api/data/v8.1/$metadata


    Process.callAction("new_CompanyLocateCompanyAdvanced",
    [{
        key: "Target",
        type: Process.Type.EntityReference,
        value: new Process.EntityReference("wf_dnbsearch", "770C630E-1292-E611-80DC-C4346BACBF10")
    },
{
    key: "searchString",
    type: Process.Type.String,
    value: "url"
}],
    function (params) {

        var apiResponse = params["apiResponse"];
        $scope.results = apiResponse.data.OrderProductResponse.OrderProductResponseDetail.Product.Organization;
        $scope.organizationName = $scope.results.OrganizationName.OrganizationPrimaryName[0].OrganizationName.$;
        $scope.primaryAddress = $scope.results.Location.PrimaryAddress[0];
        $scope.gridOptions = {
            enableRowSelection: true,
            selectionRowHeaderWidth: 35,
            rowHeight: 35,
            showGridFooter: true,
            columnDefs: [
              { name: 'Name', field: 'organization' },
              { name: 'DUNS Number', field: 'duns' },
              { name: 'City', field: 'city' },
              { name: 'Country', field: 'country' },
              { name: 'Summary', field: 'summary' },
              { name: 'Last Updated', field: 'last-updated' }
            ],
            data: [{
                "organization": $scope.organizationName,
                "duns": $scope.results.SubjectHeader.DUNSNumber,
                "city": $scope.primaryAddress.PrimaryTownName,
                "country": $scope.primaryAddress.CountryISOAlpha2Code,
                "summary": $scope.results.SubjectHeader.OrganizationSummaryText,
                "last-updated": $scope.results.SubjectHeader.LastUpdateDate.$
            }
            ]

        };
        $scope.searchdata = "";

    },
    function (e, t) {
        // Error
        alert(e);

        // Write the trace log to the dev console
        if (window.console && console.error) {
            console.error(e + "\n" + t);
        }
    });



























    Process.callAction("wf_CompanyLocateCompanyAdvanced",
    [{
        key: "searchString",
        type: Process.Type.String,
        value: "url"
    }],
    function (params) {
       
        apiResponse = params["apiResponse"];
        $scope.results = apiResponse.data.OrderProductResponse.OrderProductResponseDetail.Product.Organization;
        $scope.organizationName = $scope.results.OrganizationName.OrganizationPrimaryName[0].OrganizationName.$;
        $scope.primaryAddress = $scope.results.Location.PrimaryAddress[0];
        $scope.gridOptions = {
            enableRowSelection: true,
            selectionRowHeaderWidth: 35,
            rowHeight: 35,
            showGridFooter: true,
            columnDefs: [
              { name: 'Name', field: 'organization' },
              { name: 'DUNS Number', field: 'duns' },
              { name: 'City', field: 'city' },
              { name: 'Country', field: 'country' },
              { name: 'Summary', field: 'summary' },
              { name: 'Last Updated', field: 'last-updated' }
            ],
            data: [{
                "organization": $scope.organizationName,
                "duns": $scope.results.SubjectHeader.DUNSNumber,
                "city": $scope.primaryAddress.PrimaryTownName,
                "country": $scope.primaryAddress.CountryISOAlpha2Code,
                "summary": $scope.results.SubjectHeader.OrganizationSummaryText,
                "last-updated": $scope.results.SubjectHeader.LastUpdateDate.$
            }
            ]

        };
        $scope.searchdata = "";

    },
    function (e, t) {
        // Error
        alert(e);

        // Write the trace log to the dev console
        if (window.console && console.error) {
            console.error(e + "\n" + t);
        }
    });
}