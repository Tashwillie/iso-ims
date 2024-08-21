using Microsoft.Extensions.Options;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core;
using Mindflur.IMS.Application.Core.Auth;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Data.Models;
using RestSharp;
using System.Data;

namespace Mindflur.IMS.Business
{
    public class KeyClockBusiness : IKeyClockBusiness
    {
        private readonly IOptions<CoreSettings> _coreSettings;
        
        private readonly IMessageService _messageService;

        public KeyClockBusiness(IOptions<CoreSettings> coreSettings, IMessageService messageService)
        {
            _coreSettings = coreSettings;
           
            _messageService = messageService;
        }

        /// <summary>
        /// Issue token if user is authenticated
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<AuthToken> GetAuthToken(string tenantId, string username, string password)
        {
            return await GetToken(tenantId, username, password);
        }

        public async Task<AuthToken> GetRefreshToken(string tenantId, string refreshToken)
        {
            return await FetchRefreshToken(tenantId, refreshToken);
        }

        /// <summary>
        /// Service principle token must only be used to communicate with KeyClock server only.
        /// </summary>
        /// <returns></returns>
        public async Task<AuthToken> GetServicePrinciple()
        {
            //username and password for super admin should taken from configuration
            return await GetToken("", _coreSettings.Value.IdentityConfiguration.SuperAdmin, _coreSettings.Value.IdentityConfiguration.SuperAdminPassword);
        }

        private async Task<AuthToken> FetchRefreshToken(string tenantId, string refreshToken)
        {
            var clientId = tenantId;
            if (string.IsNullOrEmpty(tenantId))
            {
                tenantId = _coreSettings.Value.IdentityConfiguration.SuperAdminRealms;
                clientId = "admin-cli";
            }

            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/realms/{tenantId}/protocol/openid-connect/token")
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);

            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("client_id", clientId);
            request.AddParameter("refresh_token", refreshToken);

            return await client?.PostAsync<AuthToken>(request);
        }

        private async Task<AuthToken> GetToken(string tenantId, string username, string password)
        {
            var clientId = tenantId;
            if (string.IsNullOrEmpty(tenantId))
            {
                tenantId = _coreSettings.Value.IdentityConfiguration.SuperAdminRealms;
                clientId = "admin-cli";
            }

            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/realms/{tenantId}/protocol/openid-connect/token")
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);

            var request = new RestRequest();

            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "password");
            request.AddParameter("client_id", clientId);
            request.AddParameter("username", username);
            request.AddParameter("password", password);

            return await client?.PostAsync<AuthToken>(request);
        }

        /// <summary>
        /// Add userId to newly created user (its a user put call to keyclock)
        /// </summary>
        /// <returns></returns>
        public async Task AddUserIdToKeyClockUser(string tenantName, string keyClockUserId, string userId)
        {
            var updateKeyClockUser = await GetKeyClockUser(tenantName, keyClockUserId);

            var userIdAttribute = new UpdateKeyClockUserModelAttributes();
            userIdAttribute.UserId = new List<string> { userId };

            if (updateKeyClockUser != null)
            {
                updateKeyClockUser.Attributes = userIdAttribute;
            }

            var token = await GetServicePrinciple();

            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/{tenantName}/users/{keyClockUserId}")
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);

            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + token.AccessToken);
            request.AddHeader("Content-Type", "application/json");
            string body = System.Text.Json.JsonSerializer.Serialize(updateKeyClockUser);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            var response = await client.PutAsync(request);
        }

        public async Task<UpdateKeyClockUserModel> GetKeyClockUser(string tenantName, string keyClockUserId)
        {
            var token = await GetServicePrinciple();

            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/{tenantName}/users/{keyClockUserId}")
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var request = new RestRequest();

            request.AddHeader("Authorization", "Bearer " + token.AccessToken);
            return await client.GetAsync<UpdateKeyClockUserModel>(request);
        }

        /// <summary>
        /// Service principle token must only be used to communicate with KeyClock server only.
        /// </summary>
        /// <returns></returns>
        public async Task CreateRealm(string tenantName)
        {
            var realm = new KeyClockCreateRealmModel();
            realm.Realm = tenantName;
            realm.Enabled = true;

            var token = await GetServicePrinciple();

            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms")
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var request = new RestRequest();

            request.AddHeader("Authorization", "Bearer " + token.AccessToken);
            request.AddHeader("Content-Type", "application/json");
            string body = System.Text.Json.JsonSerializer.Serialize(realm);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            await client.PostAsync(request);
        }

        public async Task CreateClient(string clientName, string tenantName)
        {
            var token = await GetServicePrinciple();
            var keyClockClient = new CreateClientModel();
            keyClockClient.Protocol = "openid-connect";
            keyClockClient.ClientId = clientName;
            keyClockClient.Name = clientName;
            keyClockClient.Description = "";
            keyClockClient.PublicClient = true;
            keyClockClient.AuthorizationServicesEnabled = false;
            keyClockClient.ServiceAccountsEnabled = false;
            keyClockClient.ImplicitFlowEnabled = false;
            keyClockClient.DirectAccessGrantsEnabled = true;
            keyClockClient.StandardFlowEnabled = true;
            keyClockClient.FrontchannelLogout = true;
            keyClockClient.AlwaysDisplayInConsole = false;
            keyClockClient.attributes = new Attributes();
            keyClockClient.attributes.AuthorizationGrantEnabled = true;
            keyClockClient.attributes.CibaGrantEnabled = false;
            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenantName + "/clients")
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + token.AccessToken);
            request.AddHeader("Content-Type", "application/json");

            string body = System.Text.Json.JsonSerializer.Serialize(keyClockClient);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            await client.PostAsync(request);		
		}

        public async Task<KeyClockClient> GetClient(string clientName, string tenantName)
        {
            var token = await GetServicePrinciple();
            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenantName + "/clients")
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var request = new RestRequest();

            request.AddHeader("Authorization", "Bearer " + token.AccessToken);

            var kc_clients = await client.GetAsync<List<KeyClockClient>>(request);

            return kc_clients.Where(c => c.ClientId == clientName).FirstOrDefault();
        }

        public async Task<KeyClockRole> CreateRole(TenanttMaster tenant, string roleName)
        {
            var token = await GetServicePrinciple();
            var clientRole = new KeyClockCreateRoleModel();
            clientRole.Name = roleName;

            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenant.TenantId + "/clients/" + tenant.ClientId + "/roles")
            {
                ThrowOnAnyError = true,

                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var request = new RestRequest();

            request.AddHeader("Authorization", "Bearer " + token.AccessToken);

            request.AddHeader("Content-Type", "application/json");
            string body = System.Text.Json.JsonSerializer.Serialize(clientRole);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            var response = await client.PostAsync(request);

            var roles = await GetRoles(tenant);

            return roles.FirstOrDefault(role => role.Name == roleName);
			await _messageService.SendNotificationMessage(new NotificationMessage()
			{
				SourceIdUserId = tenant.TenantId,
				Action = IMSControllerActionCategory.Create,
				Module = IMSControllerCategory.Role,
				ItemId = tenant.TenantId,
				Description = "Role is Created",
				Title = roleName,
			});
		}

        public async Task CreateUserIdTokenProtocalMap(TenanttMaster tenant)
        {
            var token = await GetServicePrinciple();
            KeyClockAttributeMap keyClockAttributeMap = new KeyClockAttributeMap()
            {
                Protocol = "openid-connect",
                ProtocolMapper = "oidc-usermodel-attribute-mapper",
                Name = "userId",
                Config = new KeyClockAttributeConfig()
                {
                    UserAttribute = "userId",
                    ClaimName = "userId",
                    JsonTypeLabel = "",
                    IdtokenClaim = "true",
                    AccessTokenClaim = "true",
                    multivalued = false,
                    AggregateAttrs = false,
                    UserInfoTokenClaim = "true",
                }
            };

            //https://20.204.150.53:8443/admin/realms/12/clients/ea66f615-8711-4e8f-ba3f-5caeb145e860/protocol-mappers/models
            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenant.TenantId + "/clients/" + tenant.ClientId + "/protocol-mappers/models")
            {
                ThrowOnAnyError = true,

                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var request = new RestRequest();

            request.AddHeader("Authorization", "Bearer " + token.AccessToken);

            request.AddHeader("Content-Type", "application/json");
            string body = System.Text.Json.JsonSerializer.Serialize(keyClockAttributeMap);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            await client.PostAsync(request);
        }

        public async Task<IList<KeyClockRole>> GetRoles(TenanttMaster tenant)
        {
            var token = await GetServicePrinciple();

            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenant.TenantId + "/clients/" + tenant.ClientId + "/roles")
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var getRoleRequest = new RestRequest();

            getRoleRequest.AddHeader("Authorization", "Bearer " + token.AccessToken);

            return await client.GetAsync<IList<KeyClockRole>>(getRoleRequest);
        }

        public async Task RemoveUserFromRole(TenanttMaster tenant, KeyClockRole keyClockRole, string userId)
        {
            IList<KeyClockRole> keyClockRoles = new List<KeyClockRole>
            {
                keyClockRole
            };

            var token = await GetServicePrinciple();
            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenant.TenantId + "/users/" + userId + "/role-mappings/clients/" + tenant.ClientId)
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var request = new RestRequest();

            request.AddHeader("Authorization", "Bearer " + token.AccessToken);
            request.AddHeader("Content-Type", "application/json");

            var body = System.Text.Json.JsonSerializer.Serialize(keyClockRoles);

            request.AddParameter("application/json", body, ParameterType.RequestBody);

            await client.DeleteAsync(request);
        }

        public async Task<KeyClockUser> CreateUser(string tenantName, string username, string emailAddress, string firstName, string lastName)
        {
            var token = await GetServicePrinciple();
            var user = new KeyClockCreateUserModel();
            user.UserName = username;
            user.Email = emailAddress;
            user.FirstName = firstName;
            user.LastName = lastName;
            user.EmailVerified = true;
            user.Enabled = true;
            user.RequiredActions = new List<string>();
            user.Groups = new List<string>();

            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenantName + "/users")
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var request = new RestRequest();

            request.AddHeader("Authorization", "Bearer " + token.AccessToken);

            request.AddHeader("Content-Type", "application/json");
            string body = System.Text.Json.JsonSerializer.Serialize(user);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = await client.PostAsync(request);

            var options1 = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenantName + "/users")
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client1 = new RestClient(options);
            var request1 = new RestRequest();

            request1.AddHeader("Authorization", "Bearer " + token.AccessToken);

            var response1 = await client1.GetAsync<List<KeyClockUser>>(request1);

            return response1.Where(T => T.userName.ToLower() == username.ToLower()).FirstOrDefault();
        }

        public async Task DeleteUser(string tenantName, string userId)
        {
            var token = await GetServicePrinciple();

            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenantName + "/users/" + userId)
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");

            request.AddHeader("Origin", $"{_coreSettings.Value.IdentityConfiguration.Endpoint}");
            request.AddHeader("authority", "20.204.150.53:8443");
            request.AddHeader("Authorization", "Bearer " + token.AccessToken);
            await client.DeleteAsync(request);
        }

        public async Task AddUserToRole(TenanttMaster tenant, KeyClockRole keyClockRole, string userId)
        {
            IList<KeyClockRole> keyClockRoles = new List<KeyClockRole>
            {
                keyClockRole
            };

            var token = await GetServicePrinciple();

            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenant.TenantId + "/users/" + userId + "/role-mappings/clients/" + tenant.ClientId)
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var request = new RestRequest();

            request.AddHeader("Authorization", "Bearer " + token.AccessToken);
            request.AddHeader("Content-Type", "application/json");

            var body = System.Text.Json.JsonSerializer.Serialize(keyClockRoles);

            request.AddParameter("application/json", body, ParameterType.RequestBody);

            await client.PostAsync(request);
        }

        public async Task CreateUserPassword(string tenantName, string userId, string password)
        {
            var token = await GetServicePrinciple();
            var data = new KeyClockCreatePasswordModel();
            data.Temporary = false;
            data.Type = "password";
            data.Value = password;

            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenantName + "/users/" + userId + "/reset-password")
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var request = new RestRequest();

            request.AddHeader("Authorization", "Bearer " + token.AccessToken);
            request.AddHeader("Content-Type", "application/json");
            var body = System.Text.Json.JsonSerializer.Serialize(data);

            request.AddParameter("text/plain", body, ParameterType.RequestBody);

            await client.PutAsync(request);
        }

        public async Task<KeyClockRealmKeys> GetRealmKeys(string tenantName)
        {
            var token = await GetServicePrinciple();
            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/{tenantName}/keys")
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var request = new RestRequest();

            request.AddHeader("Authorization", "Bearer " + token.AccessToken);

            return await client.GetAsync<KeyClockRealmKeys>(request);
        }

        public async Task DeleteRealm(string tenantName)
        {
            var token = await GetServicePrinciple();
            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenantName)
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var request = new RestRequest();

            request.AddHeader("Authorization", "Bearer " + token.AccessToken);
            await client.DeleteAsync(request);
        }

        public async Task<KeyClockUser> UpdateUser(string tenantName, string username, string emailAddress, string firstName, string lastName, string userId)
        {
            var token = await GetServicePrinciple();
            var user = new KeyClockUpdateUserModel();
            user.UserName = username;
            user.Email = emailAddress;
            user.FirstName = firstName;
            user.LastName = lastName;
            user.EmailVerified = true;
            user.Enabled = true;
            user.RequiredActions = new List<string>();
            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenantName + "/users/" + userId)
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };
            var client = new RestClient(options);
            var request = new RestRequest();

            request.AddHeader("Authorization", "Bearer " + token.AccessToken);
            request.AddHeader("Content-Type", "application/json");
            string body = System.Text.Json.JsonSerializer.Serialize(user);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = await client.PutAsync(request);
            var options1 = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenantName + "/users")
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };

            var client1 = new RestClient(options);
            var request1 = new RestRequest();

            request1.AddHeader("Authorization", "Bearer " + token.AccessToken);

            var response1 = await client1.GetAsync<List<KeyClockUser>>(request1);

            return response1.Where(T => T.userName.ToLower() == emailAddress.ToLower()).FirstOrDefault();
        }

        public async Task UnAssignRole(string tenantName, string userId, string clientId)
        {
            var token = await GetServicePrinciple();

            var options = new RestClientOptions($"{_coreSettings.Value.IdentityConfiguration.Endpoint}/admin/realms/" + tenantName + "/users/" + userId + "/role-mappings/" + "clients/" + clientId)
            {
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                MaxTimeout = 10000
            };
            var client = new RestClient(options);
            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token.AccessToken);
            await client.DeleteAsync(request);
        }
    }
}