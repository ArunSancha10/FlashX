using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using outofoffice.App_code;
using System.Net;
using System.Text.Json.Nodes;
using System.DirectoryServices.Protocols;

namespace outofoffice.Service
{
    public class LdapService
    {
        #region Global Variabel
        private static LdapConnection connection;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public LdapService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region This Method Validate UserBased On Active Directory and Store thier Email in Session
        public JObject validateUserByBind(string username, string password)
        {

            JObject responseobj = new JObject();
            string status = null;
            string email = null;// Variable to store the email

            // Retrieve AD connection details from configuration
            // Retrieve AD connection details from configuration with null checks and defaults
            string adHost = _configuration["AD_ConnectionDetails:host"] ?? throw new ArgumentNullException("AD_ConnectionDetails:host", "The host cannot be null.");
            string adPort = _configuration["AD_ConnectionDetails:port"] ?? "389"; // Use a default port if not provided
            string url = $"{adHost}:{adPort}";
            string adUser = _configuration["AD_ConnectionDetails:user"] ?? throw new ArgumentNullException("AD_ConnectionDetails:user", "The service account username cannot be null.");
            string adPassword = _configuration["AD_ConnectionDetails:password"] ?? throw new ArgumentNullException("AD_ConnectionDetails:password", "The service account password cannot be null.");
            string searchBase = _configuration["AD_ConnectionDetails:domain_controller"] ?? throw new ArgumentNullException("AD_ConnectionDetails:domain_controller", "The domain controller cannot be null.");


            // Create network credentials for the service account
            var serviceCredentials = new NetworkCredential(adUser, adPassword, adHost);
            var serverIdentifier = new LdapDirectoryIdentifier(url);

            try
            {
                using (connection = new LdapConnection(serverIdentifier, serviceCredentials))
                {
                    // Attempt to bind to the LDAP server with the service account
                    connection.Bind();

                    // Create network credentials for user authentication and attempt to bind as the user
                    var userCredentials = new NetworkCredential(username, password, adHost);
                    connection.AuthType = AuthType.Negotiate;
                    connection.Bind(userCredentials);

                    // If bind is successful, search for the user's email
                    string searchFilter = $"(sAMAccountName={username})";
                    var searchRequest = new SearchRequest(
                        searchBase, // Replace with your domain components
                        searchFilter,
                        SearchScope.Subtree,
                        "mail" // Attribute to retrieve
                    );

                    var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);
                    if (searchResponse.Entries.Count > 0)
                    {
                        var entry = searchResponse.Entries[0];
                        if (entry.Attributes["mail"] != null)
                        {
                            email = entry.Attributes["mail"][0].ToString();
                            status = "Success";
                        }
                    }


                }
            }
            catch (LdapException ldapEx)
            {
                // Log LDAP specific errors (optional)
                Console.WriteLine($"LDAP Exception: {ldapEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                // Log unauthorized access (optional)
                Console.WriteLine($"Unauthorized Access Exception: {uaEx.Message}");
            }
            catch (Exception ex)
            {
                // Catch any other exceptions that might occur
                Console.WriteLine($"General Exception: {ex.Message}");
            }
            finally
            {
                // Ensure the connection is disposed even if an error occurs
                connection?.Dispose();
            }

            // Populate the JObject response object with status and email
            responseobj["status"] = status;
            responseobj["email"] = email;

            return responseobj;
        }

        #endregion

    }
}
