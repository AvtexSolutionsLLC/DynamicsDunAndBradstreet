var app = angular.module("dnbSearch", ['ngTouch', 'ui.grid', 'ui.grid.selection', 'ui.grid.resizeColumns']);


app.controller('dnbSearchController', ['$scope', '$http', function ($scope, $http) {
    $scope.searchButtonText = "Search";
    $scope.search = {};
    $scope.searchKeyword = '';
    $scope.results = [];
    $scope.link = '';
    $scope.showlink = 'true';
    $scope.token = '';
    $scope.dunsNumber = ''; //from selected row in grid
    $scope.keywordText = ''
    $scope.dunsText = '' //from filter input
    $scope.orgText = ''; //ng-model="searchdata"
    $scope.countryIsoCode = '';
    $scope.cityText = '';
    $scope.industryText = '';
    $scope.postalCode = '';
    $scope.riskScore = '';
    $scope.legalFormCode = '';
    $scope.stockExchangeTickerName = '';
    $scope.ownerControl = '';
    $scope.salesLowRangeAmount = '';
    $scope.salesHighRangeAmount = '';
    $scope.totalAssetLow = '';
    $scope.totalAssetHigh = '';
    $scope.netIncomeLow = '';
    $scope.netIncomeHigh = '';
    $scope.employeeLow = '';
    $scope.employeeHigh = '';
    $scope.premisesAreaLow = '';
    $scope.premisesAreaHigh = '';
    $scope.marketCap = '';
    $scope.foreignTrade = '';
    $scope.subsidiaryStatus = '';
    $scope.yearFounded = '';
    $scope.searchString = '';
    //match
    $scope.mSubject = '';
    $scope.mDuns = '';
    $scope.mStreetAddress = '';
    $scope.mCity = '';
    $scope.mTerritory = '';
    $scope.mCountryIsoCode = '';
    $scope.mPostal = '';
    $scope.mTele = '';

    app.filter('calculateRevenue', function () {
        return function (input) {
            return Math.floor((input * 1000000));
        };
    });

    $scope.gridOptions = {
        enableSorting: false,
        enableRowSelection: true,
        enableFullRowSelection: true,
        showGridFooter: true,
        height: 250,
        data: $scope.results,
        columnDefs: [
                      { name: 'DUNS #', field: 'DUNSNumber', width: 100 },
                      { name: 'Company Name', field: 'OrganizationPrimaryName.OrganizationName.$', width: 250 },
                      { name: 'Trade Name', field: 'TradeStyleName[0].OrganizationName.$', width: 175 },
                      { name: 'Country', field: 'PrimaryAddress.CountryOfficialName', width: 100 },
                      { name: 'City', field: 'PrimaryAddress.PrimaryTownName', width: 100 },
                      { name: 'State', field: 'PrimaryAddress.TerritoryOfficialName', width: 50 },
                      { name: 'Industry', field: 'IndustryCode[0].IndustryCodeDescription[0].$', width: 200 },
                      { name: 'Employees', field: 'ConsolidatedEmployeeDetails.TotalEmployeeQuantity', width: 100 },
                      { name: 'Facility Type', field: 'FamilyTreeMemberRole[0].FamilyTreeMemberRoleText.$', width: 150 },
                      { name: 'Manufacturing Facility', field: 'ManufacturingIndicator', width: 150, cellTemplate: '<div class="ui-grid-cell-contents" title="ManufacturingIndicator">{{COL_FIELD == true ? "Yes" : (COL_FIELD == false ? "No" : "")}}</div>' },
                      { name: 'Sales Revenue', field: 'SalesRevenueAmount.$', width: 175, cellTemplate: '<div class="ui-grid-cell-contents" title="TOOLTIP">{{grid.appScope.calculateRevenue(grid, row)}}</div>' },
                      { name: 'Currency', field: 'SalesRevenueAmount.@CurrencyISOAlpha3Code', width: 75 },
                      { name: 'Street', field: 'PrimaryAddress.StreetAddressLine[0].LineText', width: 200 },
                      { name: 'Postal', field: 'PrimaryAddress.PostalCode', width: 100 },
                      { name: 'Phone', field: 'TelephoneNumber.TelecommunicationNumber', width: 100 },
                      { name: 'Fax', field: 'FacsimileNumber.TelecommunicationNumber', width: 100 },
                      { name: 'Publicly Traded?', field: 'PubliclyTradedCompanyIndicator', width: 150, cellTemplate: '<div class="ui-grid-cell-contents" title="{{COL_FIELD}}">{{COL_FIELD == true ? "Yes" : (COL_FIELD == false ? "No" : "")}}</div>' },
                      { name: 'Marketability Indicator', field: 'MarketabilityIndicator', width: 150, cellTemplate: '<div class="ui-grid-cell-contents" title="MarketabilityIndicator">{{COL_FIELD == true ? "Yes" : (COL_FIELD == false ? "No" : "")}}</div>' },
                      { name: 'Stand Alone Organization', field: 'StandaloneOrganizationIndicator', width: 150, cellTemplate: '<div class="ui-grid-cell-contents" title="StandaloneOrganizationIndicator">{{COL_FIELD == true ? "Yes" : (COL_FIELD == false ? "No" : "")}}</div>' }
        ]
    };
    $scope.buildMatchString = function () {
        var matchString = '';
        var s = [
               { name: 'SubjectName=', value: $scope.mSubject },
               { name: 'DUNSNumber=', value: $scope.mDuns },
               { name: 'StreetAddressLine-1=', value: $scope.mStreetAddress },
               { name: 'PrimaryTownName-1=', value: $scope.mCity },
               { name: 'TerritoryName=', value: $scope.mTerritory },
               { name: 'CountryISOAlpha2Code=', value: $scope.mCountryIsoCode },
               { name: 'PostalCode-1=', value: $scope.mPostal },
               { name: 'Telephone=', value: $scope.mTele },

        ];
        for (var i = 0; i < s.length; i++) {
            if (s[i].value != null && s[i].value.length > 0) {
                matchString += s[i].name + s[i].value + '&';
            }
        }
        return matchString;
    };
    $scope.buildSearchString = function () {
        var searchString = '';
        var s = [
                   { name: 'KeywordText=', value: $scope.keywordText },
                   { name: 'DUNSNumber=', value: $scope.dunsText },
                   { name: 'OrganizationName=', value: $scope.orgText },
                   { name: 'CountryISOAlpha2Code-1=', value: $scope.countryIsoCode },
                   { name: 'PrimaryTownName-1=', value: $scope.cityText },
                   { name: 'IndustryCodeTypeCode-1=', value: $scope.industryCode },
                   { name: 'IndustryCode-1=', value: $scope.industryText },
                   { name: 'PostalCode-1=', value: $scope.postalCode },
                   { name: 'MarketingRiskClassCode-1=', value: $scope.riskScore },
                   { name: 'LegalFormCode-1=', value: $scope.legalFormCode },
                   { name: 'StockExchangeTickerName=', value: $scope.stockExchangeTickerName },
                   { name: 'ControlOwnershipTypeCode-1=', value: $scope.ownerControl },
                   { name: 'SalesLowRangeAmount=', value: $scope.salesLowRangeAmount },
                   { name: 'SalesHighRangeAmount=', value: $scope.salesHighRangeAmount },
                   { name: 'TotalAssetLowRangeAmount=', value: $scope.totalAssetLow },
                   { name: 'TotalAssetHighRangeAmount=', value: $scope.totalAssetHigh },
                   { name: 'NetIncomeLowRangeAmount=', value: $scope.netIncomeLow },
                   { name: 'NetIncomeHighRangeAmount=', value: $scope.netIncomeHigh },
                   { name: 'ConsolidatedEmployeeLowRangeQuantity=', value: $scope.employeeLow },
                   { name: 'ConsolidatedEmployeeHighRangeQuantity=', value: $scope.employeeHigh },
                   { name: 'PremisesAreaLowRangeMeasurement=', value: $scope.premisesAreaLow },
                   { name: 'PremisesAreaHighRangeMeasurement=', value: $scope.premisesAreaHigh },
                   { name: 'MarketCapitalizationLowRangeAmount=', value: $scope.marketCap },
                   { name: 'ExportIndicator=', value: $scope.foreignTrade },
                   { name: 'FranchisingIndicator=', value: $scope.subsidiaryStatus },
                   { name: 'ControlOwnershipFromYear=', value: $scope.yearFounded },
                   { name: 'CandidatePerPageMaximumQuantity=', value: 250 }

        ];
        for (var i = 0; i < s.length; i++) {
            if (s[i].value != null && s[i].value.length > 0) {
                searchString += s[i].name + s[i].value + '&';
            }
        }
        return searchString;
    };
    $scope.importCompanyProfile = function () {

        $scope.getCurrentSelection();
        var duns = $scope.dunsNumber;
        //alert(duns);

        Process.callAction("new_companydbgetprofileforImportstartaction",
         [{
             key: "Target",
             type: Process.Type.EntityReference,
             value: new Process.EntityReference('wf_orderproductresponse', "5012C7D0-0796-E611-9445-005056AC06BB")
         },
     {
         key: "dunsNumber",
         type: Process.Type.String,
         value: duns
     }
         ],
         function (params) {
             var apiResponse = JSON.parse(params["apiResponseDCP"]);
             alert("Import successful.");
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
    $scope.locateCompanyMatch = function () {
        var matchString = $scope.buildMatchString();
        // alert(matchString);
        Process.callAction("new_companydbmatchsearchstartactionb033d02ba995e6119445005056ac06bb",
         [{
             key: "Target",
             type: Process.Type.EntityReference,
             value: new Process.EntityReference("wf_dnbsearch", "DA1B5492-AB95-E611-9445-005056AC06BB")
         },
     {
         key: "searchStringMatch",
         type: Process.Type.String,
         value: matchString
     }
         ],
         function (params) {
             var apiResponse = JSON.parse(params["apiResponseMatch"]);
             $scope.results = apiResponse.GetCleanseMatchResponse.GetCleanseMatchResponseDetail.MatchResponseDetail.MatchCandidate;

             $scope.gridOptions = {
                 enableSorting: false,
                 enableRowSelection: true,
                 enableFullRowSelection: true,
                 showGridFooter: true,
                 data: $scope.results,
                 columnDefs: [
                    { name: 'DUNS ', field: 'DUNSNumber', width: 100 },
                    { name: 'Company Name', field: 'OrganizationPrimaryName.OrganizationName.$', width: 250 },
                    { name: 'Facility Type', field: 'FamilyTreeMemberRole[0].FamilyTreeMemberRoleText.$', width: 100 },
                    { name: 'City', field: 'PrimaryAddress.PrimaryTownName', width: 100 },
                    { name: 'Country', field: 'PrimaryAddress.CountryOfficialName', width: 100 },
                    { name: 'Confidence Level', field: 'MatchQualityInformation.ConfidenceCodeValue', width: 100 }]
             };
             $scope.search.data = "";
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
    $scope.locateCompanyAdvancedFilters = function () {
        var searchString = $scope.buildSearchString();
        //alert(searchString);

        Process.callAction("new_companydbadvancedsearchstartaction",
         [{
             key: "Target",
             type: Process.Type.EntityReference,
             value: new Process.EntityReference("wf_dnbsearch", "B895749B-AB95-E611-9445-005056AC06BB")
         },
     {
         key: "searchStringAdv",
         type: Process.Type.String,
         value: searchString
     }
         ],
         function (params) {
             var apiResponse = JSON.parse(params["apiResponseAdv"]);
             $scope.results = apiResponse.FindCompanyResponse.FindCompanyResponseDetail.FindCandidate;

             $scope.gridOptions = {
                 enableSorting: false,
                 enableRowSelection: true,
                 enableColumnResizing: true,
                 showGridFooter: true,
                 data: $scope.results,
                 columnDefs: [
                      { name: 'DUNS #', field: 'DUNSNumber', width: 100 },
                      { name: 'Company Name', field: 'OrganizationPrimaryName.OrganizationName.$', width: 250 },
                      { name: 'Trade Name', field: 'TradeStyleName[0].OrganizationName.$', width: 175 },
                      { name: 'Country', field: 'PrimaryAddress.CountryOfficialName', width: 100 },
                      { name: 'City', field: 'PrimaryAddress.PrimaryTownName', width: 100 },
                      { name: 'State', field: 'PrimaryAddress.TerritoryOfficialName', width: 50 },
                      { name: 'Industry', field: 'IndustryCode[0].IndustryCodeDescription[0].$', width: 200 },
                      { name: 'Employees', field: 'ConsolidatedEmployeeDetails.TotalEmployeeQuantity', width: 100 },
                      { name: 'Facility Type', field: 'FamilyTreeMemberRole[0].FamilyTreeMemberRoleText.$', width: 150 },
                      { name: 'Manufacturing Facility', field: 'ManufacturingIndicator', width: 150, cellTemplate: '<div class="ui-grid-cell-contents" title="ManufacturingIndicator">{{COL_FIELD == true ? "Yes" : (COL_FIELD == false ? "No" : "")}}</div>' },
                      { name: 'Sales Revenue', field: 'SalesRevenueAmount.$', width: 175, cellTemplate: '<div class="ui-grid-cell-contents" title="TOOLTIP">{{grid.appScope.calculateRevenue(grid, row)}}</div>' },
                      { name: 'Currency', field: 'SalesRevenueAmount.@CurrencyISOAlpha3Code', width: 75 },
                      { name: 'Street', field: 'PrimaryAddress.StreetAddressLine[0].LineText', width: 200 },
                      { name: 'Postal', field: 'PrimaryAddress.PostalCode', width: 100 },
                      { name: 'Phone', field: 'TelephoneNumber.TelecommunicationNumber', width: 100 },
                      { name: 'Fax', field: 'FacsimileNumber.TelecommunicationNumber', width: 100 },
                      { name: 'Publicly Traded?', field: 'PubliclyTradedCompanyIndicator', width: 150, cellTemplate: '<div class="ui-grid-cell-contents" title="{{COL_FIELD}}">{{COL_FIELD == true ? "Yes" : (COL_FIELD == false ? "No" : "")}}</div>' },
                      { name: 'Marketability Indicator', field: 'MarketabilityIndicator', width: 150, cellTemplate: '<div class="ui-grid-cell-contents" title="MarketabilityIndicator">{{COL_FIELD == true ? "Yes" : (COL_FIELD == false ? "No" : "")}}</div>' },
                      { name: 'Stand Alone Organization', field: 'StandaloneOrganizationIndicator', width: 150, cellTemplate: '<div class="ui-grid-cell-contents" title="StandaloneOrganizationIndicator">{{COL_FIELD == true ? "Yes" : (COL_FIELD == false ? "No" : "")}}</div>' }
                 ]
             };
             $scope.search.data = "";
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
    $scope.locateCompanyAdvancedKeyword = function (keywords) {
        // $scope.searchKeyword = angular.copy(searchdata);
        $scope.searchButtonText = "Searching";
        Process.callAction("new_companydbbasicsearchstartaction",
          [{
              key: "Target",
              type: Process.Type.EntityReference,
              value: new Process.EntityReference("wf_dnbsearch", "C5D21F8B-D393-E611-9445-005056AC06BB")
          },
      {
          key: "searchString",
          type: Process.Type.String,
          value: keywords
      }
          ],
          function (params) {
              var apiResponse = JSON.parse(params["apiResponse"]);
              $scope.results = apiResponse.FindCompanyResponse.FindCompanyResponseDetail.FindCandidate;

              $scope.gridOptions = {
                  enableSorting: false,
                  enableRowSelection: true,
                  enableFullRowSelection: true,
                  showGridFooter: true,
                  data: $scope.results,
                  columnDefs: [
                      { name: 'DUNS #', field: 'DUNSNumber', width: 100 },
                      { name: 'Company Name', field: 'OrganizationPrimaryName.OrganizationName.$', width: 250 },
                      { name: 'Sales Revenue', field: 'SalesRevenueAmount.$', width: 125 },
                      { name: 'Currency', field: 'SalesRevenueAmount.$', width: 125 },
                      { name: 'Industry', field: 'IndustryCode[0].IndustryCodeDescription[0].$', width: 200 },
                      { name: 'Employees', field: 'ConsolidatedEmployeeDetails.TotalEmployeeQuantity', width: 100 },
                      { name: 'Facility Type', field: 'FamilyTreeMemberRole[0].FamilyTreeMemberRoleText.$', width: 100 },
                      { name: 'Country', field: 'PrimaryAddress.CountryOfficialName', width: 100 },
                      { name: 'Street', field: 'PrimaryAddress.StreetAddressLine[0].LineText', width: 100 },
                      { name: 'City', field: 'PrimaryAddress.PrimaryTownName', width: 100 },
                      { name: 'State', field: 'PrimaryAddress.TerritoryOfficialName', width: 100 },
                      { name: 'Postal', field: 'PrimaryAddress.PostalCode', width: 100 },
                      { name: 'Phone', field: 'TelephoneNumber.TelecommunicationNumber', width: 100 },
                      { name: 'Fax', field: 'FacsimileNumber.TelecommunicationNumber', width: 100 },
                      { name: 'Public', field: 'PubliclyTradedCompanyIndicator', width: 100 },
                      { name: 'Trade Name', field: 'TradeStyleName[0].OrganizationName.$', width: 175 },
                      { name: 'Legal Status', field: 'PrimaryAddress.LatitudeMeasurement', width: 100 },
                      { name: 'Ownership Control', field: 'PrimaryAddress.LongitudeMeasurement', width: 100 }
                  ]
              };
              $scope.searchButtonText = "Search";
              $scope.search.data = "";
          },
          function (e, t) {
              // Error
              $scope.searchButtonText = "Search";
              alert(e);

              // Write the trace log to the dev console
              if (window.console && console.error) {
                  console.error(e + "\n" + t);
              }
          });
        $scope.searchButtonText = "Search";
    }
    $scope.reset = function () {
        $scope.dunsNumber = ''; //from selected row in grid
        $scope.keywordText = ''
        $scope.dunsText = '' //from filter input
        $scope.orgText = ''; //ng-model="searchdata"
        $scope.countryIsoCode = '';
        $scope.cityText = '';
        $scope.industryText = '';
        $scope.postalCode = '';
        $scope.riskScore = '';
        $scope.legalFormCode = '';
        $scope.stockExchangeTickerName = '';
        $scope.ownerControl = '';
        $scope.salesLowRangeAmount = '';
        $scope.salesHighRangeAmount = '';
        $scope.totalAssetLow = '';
        $scope.totalAssetHigh = '';
        $scope.netIncomeLow = '';
        $scope.netIncomeHigh = '';
        $scope.employeeLow = '';
        $scope.employeeHigh = '';
        $scope.premisesAreaLow = '';
        $scope.premisesAreaHigh = '';
        $scope.marketCap = '';
        $scope.foreignTrade = '';
        $scope.subsidiaryStatus = '';
        $scope.yearFounded = '';
        $scope.searchString = '';
        //match
        $scope.mSubject = '';
        $scope.mDuns = '';
        $scope.mStreetAddress = '';
        $scope.mCity = '';
        $scope.mTerritory = '';
        $scope.mCountryIsoCode = '';
        $scope.mPostal = '';
        $scope.mTele = '';

    }
    $scope.getCurrentSelection = function () {
        var values = [];
        //var currentSelection = $scope.gridApi.cellNav.getCurrentSelection();
        $scope.dunsNumber = $scope.gridApi.grid.selection.lastSelectedRow.entity.DUNSNumber;
    };
    $scope.gridOptions.onRegisterApi = function (gridApi) {
        //set gridApi on scope
        $scope.gridApi = gridApi;
       
    };
    $scope.calculateRevenue = function (grid, row) {
        return (row.entity.SalesRevenueAmount.$ * 1000000).toLocaleString('en-US', { style: 'currency', currency: 'USD' });;
    }

}]);


