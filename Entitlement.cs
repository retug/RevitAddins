using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net; // for HttpStatusCode
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
// Added for REST API
// We are using C# REST library called RestShap
// See http://restsharp.org/ for detail
//  
using RestSharp;
using RestSharp.Serializers;
/// Revit 2016 has added two methods to help exchange store app publishers
/// to check a store app entitlement, i.e., to check if the user has purchase or not.
/// This is a minimum sample to show the usage.
///

namespace rebarBenderMulti
{
    [Transaction(TransactionMode.Manual)]
    public class Entitlement: IExternalCommand
    {
        // Set values specific to the environment
        public const string _baseApiUrl = @"https://apps.exchange.autodesk.com/";
        // This is the id of your app.
        // e.g.,
        //public const string _appId = @"appstore.exchange.autodesk.com:TransTips-for-Revit:en";
        public const string _appId = @"<the id of your app comes here>";

        // Command to check an entitlement
        public Autodesk.Revit.UI.Result Execute(
            ExternalCommandData commandData,
            ref string message,
            Autodesk.Revit.DB.ElementSet elements)
        {
            // Get hold of the top elements
            UIApplication uiApp = commandData.Application;
            Application rvtApp = uiApp.Application;

            // Check to see if the user is logged in.
            if (!Application.IsLoggedIn)
            {
                TaskDialog.Show("Entitlement API", "Please login to Autodesk 360 first\n");
                return Result.Failed;
            }

            // Get the user id, and check entitlement
            string userId = rvtApp.LoginUserId;
            bool isValid = CheckEntitlement(_appId, userId);

            if (isValid)
            {
                // The usert has a valid entitlement, i.e.,
                // if paid app, purchase the app from the store.
            }

            // For now, display the result
            string msg = "userId = " + userId
                + "\nappId = " + _appId
                + "\nisValid = " + isValid.ToString();
            TaskDialog.Show("Entitlement API", msg);

            return Result.Succeeded;
        }

        ///========================================================
        /// URL: https://apps.exchange.autodesk.com/webservices/checkentitlement
        /// Method: GET
        /// Parameter:
        ///   userid
        ///   appid
        ///   
        /// Sample response
        /// {
        /// "UserId":"2N5FMZW9CCED",
        /// "AppId":"appstore.exchange.autodesk.com:autodesk360:en",
        /// "IsValid":false,
        /// "Message":"Ok"
        /// }
        /// ========================================================

        private bool CheckEntitlement(string appId, string userId)
        {
            // REST API call for the entitlement API.
            // We are using RestSharp for simplicity.
            // You may choose to use other library.

            // (1) Build request
            var client = new RestClient();
            client.BaseUrl = new System.Uri(_baseApiUrl);

            // Set resource/end point
            var request = new RestRequest();
            request.Resource = "webservices/checkentitlement";
            request.Method = Method.Get;

            // Add parameters
            request.AddParameter("userid", userId);
            request.AddParameter("appid", appId);

            // (2) Execute request and get response
            
            //IRestResponse response = client.Execute(request);
            RestResponse response = client.Execute(request);

            // (3) Parse the response and get the value of IsValid.
            bool isValid = false;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JsonDeserializer deserial = new JsonDeserializer();
                EntitlementResponse entitlementResponse = deserial.Deserialize<EntitlementResponse>(response);
                isValid = entitlementResponse.IsValid;
            }

            return isValid;
        }

        [Serializable]
        public class EntitlementResponse
        {
            public string UserId { get; set; }
            public string AppId { get; set; }
            public bool IsValid { get; set; }
            public string Message { get; set; }
        }

    }
}