using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sas.api
{
	public static class Roles
	{
		[FunctionName("Roles")]
		public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", "POST", Route = "Roles")]
			HttpRequest req, ILogger log)
		{
			RolesResult rr = new RolesResult();

			// Request body is supposed to contain the user's identity claim
			if (req.Body.Length > 0)
			{
				IdentityToken it = await JsonSerializer.DeserializeAsync<IdentityToken>(req.Body);

				log.LogInformation($"Looking for custom roles to assign to '{it.UserDetails}'.");

				string[] additionalRoles = it.Claims
					.Where(c => c.Type.Equals("http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
					.Select(c => c.Value)
					.ToArray();

				log.LogInformation($"Assigning additional roles '{additionalRoles}' to '{it.UserDetails}'");

				rr = new RolesResult()
				{
					Roles = additionalRoles
				};
			}

			return new OkObjectResult(rr);
		}

		private class Claim
		{
			[JsonPropertyName("typ")]
			public string Type { get; set; }

			[JsonPropertyName("val")]
			public string Value { get; set; }
		}

		private class IdentityToken
		{
			[JsonPropertyName("identityProvider")]
			public string IdentityProvider { get; set; }

			[JsonPropertyName("userId")]
			public string UserId { get; set; }

			[JsonPropertyName("userDetails")]
			public string UserDetails { get; set; }

			[JsonPropertyName("claims")]
			public List<Claim> Claims { get; } = new List<Claim>();

			[JsonPropertyName("accessToken")]
			public string AccessToken { get; set; }
		}

		private class RolesResult
		{
			public string[] Roles { get; set; }
		}
	}
}