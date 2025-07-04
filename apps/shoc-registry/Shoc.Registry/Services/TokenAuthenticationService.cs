using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shoc.ApiCore;
using Shoc.ApiCore.GrpcClient;
using Shoc.Registry.Crypto;
using Shoc.Registry.Model.Credential;
using Shoc.Registry.Model.Key;
using Shoc.Registry.Model.Registry;
using Shoc.Registry.Model.TokenSpec;
using Shoc.Registry.Utility;

namespace Shoc.Registry.Services;

/// <summary>
/// The token authentication service
/// </summary>
public class TokenAuthenticationService : AuthenticationServiceBase
{
    /// <summary>
    /// Control how many seconds we allowed to pass the refresh token lifetime
    /// </summary>
    private const int REFRESH_TOKEN_CLOCK_SKEW_SECONDS = 60;

    /// <summary>
    /// The lifetime of the access token (default to 5 minutes)
    /// </summary>
    private const int ACCESS_TOKEN_LIFETIME_SECONDS = 5 * 60;

    /// <summary>
    /// The lifetime of the refresh token (default to 6 hours)
    /// </summary>
    private const int REFRESH_TOKEN_LIFETIME_SECONDS = 6 * 60 * 60;

    /// <summary>
    /// The id of the credential
    /// </summary>
    private const string CREDENTIAL_ID_CLAIM = "credential_id";

    /// <summary>
    /// The set of allowed grant types
    /// </summary>
    private static readonly ISet<string> ALLOWED_GRANT_TYPES = new HashSet<string> { "password", "refresh_token" };

    /// <summary>
    /// The self settings
    /// </summary>
    private readonly SelfSettings selfSettings;

    /// <summary>
    /// The credential service
    /// </summary>
    private readonly RegistryCredentialService credentialService;

    /// <summary>
    /// The credential protection provider
    /// </summary>
    private readonly CredentialProtectionProvider credentialProtectionProvider;

    /// <summary>
    /// Creates new instance of the service
    /// </summary>
    /// <param name="registryService">The registry service</param>
    /// <param name="registrySigningKeyService">The signing key service</param>
    /// <param name="keyProviderService">The key provider service</param>
    /// <param name="grpcClientProvider">The grpc client provider</param>
    /// <param name="selfSettings">The self settings</param>
    /// <param name="credentialService">The credential service</param>
    /// <param name="credentialProtectionProvider">The credential protection provider</param>
    public TokenAuthenticationService(RegistryService registryService,
        RegistrySigningKeyService registrySigningKeyService, KeyProviderService keyProviderService,
        IGrpcClientProvider grpcClientProvider, SelfSettings selfSettings, RegistryCredentialService credentialService,
        CredentialProtectionProvider credentialProtectionProvider)
        : base(registryService, registrySigningKeyService, keyProviderService, grpcClientProvider)
    {
        this.selfSettings = selfSettings;
        this.credentialService = credentialService;
        this.credentialProtectionProvider = credentialProtectionProvider;
    }

    /// <summary>
    /// Requests the token based on specified input fields
    /// </summary>
    /// <param name="workspaceName">The workspace name</param>
    /// <param name="registryName">The registry name</param>
    /// <param name="request">The request object</param>
    /// <returns></returns>
    public async Task<ITokenResponseSpec> GetToken(string workspaceName, string registryName, TokenRequestSpec request)
    {
        // get the registry
        var registry = await this.GetRegistry(workspaceName, registryName);

        // gets the authentication context
        var authenticationContext = this.GetAuthenticationContext(workspaceName, registry);

        // get all the keys of the registry
        var keys = (await this.registrySigningKeyService.GetAllPayloads(registry.Id)).ToList();

        // build the validation keys list
        var validationKeys = keys.Select(payload => this.keyProviderService.ToSecurityKey(payload)).ToList();

        // check if grant type is missing or not allowed
        if (string.IsNullOrWhiteSpace(request.GrantType) || !ALLOWED_GRANT_TYPES.Contains(request.GrantType))
        {
            return TokenErrorSpec.Create(TokenErrorCodes.UNSUPPORTED_GRANT_TYPE);
        }

        // not authenticated by default
        var authenticated = default(AuthenticatedSubject);

        // if grant type is password try authenticating with username and password 
        if (string.Equals(request.GrantType, "password"))
        {
            // use password authentication logic
            authenticated = await this.AuthenticatePassword(registry, request.Username, request.Password);
        }

        // if grant type is refresh_token try authenticating with given refresh token
        if (string.Equals(request.GrantType, "refresh_token"))
        {
            // use password authentication logic
            authenticated = await this.AuthenticateRefreshToken(registry, authenticationContext, request.RefreshToken,
                validationKeys);
        }

        // stop process if not authenticated
        if (authenticated == null)
        {
            return TokenErrorSpec.Create(TokenErrorCodes.INVALID_GRANT);
        }

        // the authorized accesses
        var accesses = this.AuthorizeScopes(authenticated, AuthenticationUtility.ParseScopes(request.Scope)).ToList();

        // the granted scope
        var grantedScope = AuthenticationUtility.ToScope(accesses);

        // get the oldest key for signing purposes
        var keyPayload = keys.MinBy(entity => entity.Created);

        // no key detected for signing
        if (keyPayload == null)
        {
            return TokenErrorSpec.Create(TokenErrorCodes.TEMPORARILY_UNAVAILABLE);
        }

        // convert the key payload to a security key
        var key = this.keyProviderService.ToSecurityKey(keyPayload);

        // build the signing key credential
        var signingKey = new SigningCredentials(key, keyPayload.Algorithm);

        // the json web token handler
        var jsonHandler = new JsonWebTokenHandler
        {
            SetDefaultTimesOnTokenCreation = false,
            TokenLifetimeInMinutes = (int)TimeSpan.FromSeconds(ACCESS_TOKEN_LIFETIME_SECONDS).TotalMinutes,
            MapInboundClaims = false
        };

        // the current time
        var now = DateTime.UtcNow;

        // the token descriptor
        var descriptor = new SecurityTokenDescriptor
        {
            Audience = authenticationContext.Audience,
            Expires = now.AddSeconds(ACCESS_TOKEN_LIFETIME_SECONDS),
            Issuer = authenticationContext.Issuer,
            IssuedAt = now,
            NotBefore = now,
            Claims = new Dictionary<string, object>
            {
                { "jti", Guid.NewGuid().ToString("N") },
                { "sub", authenticated.Username },
                { "scope", grantedScope },
                {
                    "access", accesses.Select(access => new Dictionary<string, object>
                    {
                        { "type", access.Type },
                        { "name", access.Name },
                        { "actions", access.Actions }
                    }).ToArray()
                }
            },
            SigningCredentials = signingKey,
        };

        // create the access token
        var accessToken = jsonHandler.CreateToken(descriptor);

        // the new refresh token
        var refreshToken = request.RefreshToken;

        // check if refresh token is requested
        if (request.AccessType == "offline" && request.GrantType == "password")
        {
            var refreshJsonHandler = new JsonWebTokenHandler
            {
                SetDefaultTimesOnTokenCreation = false,
                TokenLifetimeInMinutes = (int)TimeSpan.FromSeconds(REFRESH_TOKEN_LIFETIME_SECONDS).TotalMinutes,
                MapInboundClaims = false
            };

            // generate and sign a refresh token
            refreshToken = refreshJsonHandler.CreateToken(new SecurityTokenDescriptor
            {
                Audience = authenticationContext.Audience,
                Expires = now.AddSeconds(REFRESH_TOKEN_LIFETIME_SECONDS),
                Issuer = authenticationContext.Issuer,
                IssuedAt = now,
                NotBefore = now,
                Claims = new Dictionary<string, object>
                {
                    { "jti", Guid.NewGuid().ToString("N") },
                    { CREDENTIAL_ID_CLAIM, authenticated.CredentialId },
                },
                SigningCredentials = signingKey,
            });
        }

        // return the token
        return new TokenResponseSpec
        {
            Token = accessToken,
            AccessToken = accessToken,
            ExpiresIn = ACCESS_TOKEN_LIFETIME_SECONDS,
            IssuedAt = now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            Scope = grantedScope,
            RefreshToken = refreshToken
        };
    }

    /// <summary>
    /// Authorize the given scopes against the subject
    /// </summary>
    /// <param name="subject">The authenticated subject</param>
    /// <param name="scopes">The scopes</param>
    /// <returns></returns>
    private IEnumerable<AccessTokenAccessClaimSpec> AuthorizeScopes(AuthenticatedSubject subject,
        IEnumerable<RegistryAuthScope> scopes)
    {
        // gets the patterns allowed for the subject
        var patterns = GetAllowedPatterns(subject).ToList();

        // the set of accesses
        var accesses = new List<AccessTokenAccessClaimSpec>();

        // process every scope
        foreach (var scope in scopes)
        {
            // check if there is any allowing pattern matching the scope request
            var valid = patterns.Any(pattern => pattern.IsMatch(scope.Name));

            // skip if not match
            if (!valid)
            {
                continue;
            }

            // allowed actions
            var actions = new List<string>();

            // check if pull is requested and allowed
            if (scope.Actions.Contains("pull") && subject.PullAllowed)
            {
                actions.Add("pull");
            }

            // check if push is requested and allowed
            if (scope.Actions.Contains("push") && subject.PushAllowed)
            {
                actions.Add("push");
            }

            // skip if no allowed actions
            if (actions.Count == 0)
            {
                continue;
            }

            // add the authorized access
            accesses.Add(new AccessTokenAccessClaimSpec
            {
                Type = scope.Type,
                Name = scope.Name,
                Actions = actions.ToArray()
            });
        }

        return accesses;
    }

    /// <summary>
    /// Authenticates the subject based on the refresh token
    /// </summary>
    /// <param name="registry">The registry</param>
    /// <param name="authenticationContext">The authentication context</param>
    /// <param name="refreshToken">The refresh token</param>
    /// <param name="validationKeys">The validation keys</param>
    /// <returns></returns>
    private async Task<AuthenticatedSubject> AuthenticateRefreshToken(RegistryModel registry,
        AuthenticationContext authenticationContext, string refreshToken,
        ICollection<AsymmetricSecurityKey> validationKeys)
    {
        // no refresh token supplied
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        // no key to validate hence invalid
        if (validationKeys.Count == 0)
        {
            return null;
        }

        // the token validation parameters
        var validationParams = new TokenValidationParameters
        {
            IssuerSigningKeys = validationKeys,
            RequireAudience = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            TryAllIssuerSigningKeys = true,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true,
            ValidAlgorithms = RegistrySigningKeyAlgorithms.ALL,
            ValidAudience = authenticationContext.Audience,
            ValidIssuer = authenticationContext.Issuer,
            ClockSkew = TimeSpan.FromSeconds(REFRESH_TOKEN_CLOCK_SKEW_SECONDS)
        };

        // the token handler
        var tokenHandler = new JwtSecurityTokenHandler();

        // the principal
        ClaimsPrincipal principal;

        try
        {
            // validate the token with the given parameters
            principal = tokenHandler.ValidateToken(refreshToken, validationParams, out _);
        }
        catch (Exception)
        {
            // authentication failed
            return null;
        }

        // handle if no principal
        if (principal == null)
        {
            return null;
        }

        // get the credential id from the refresh token
        var credentialId = principal.FindFirst(claim => claim.Type == CREDENTIAL_ID_CLAIM)?.Value;

        // the target credential
        RegistryCredentialModel credential;

        try
        {
            // gets the credential by id for the refresh
            credential = await this.credentialService.GetById(registry.Id, credentialId);
        }
        catch (Exception)
        {
            credential = null;
        }

        // check if the credential is still valid
        if (credential == null)
        {
            return null;
        }

        // build the authenticated principal
        return new AuthenticatedSubject
        {
            RegistryId = registry.Id,
            CredentialId = credential.Id,
            Username = credential.Username,
            WorkspaceId = credential.WorkspaceId,
            UserId = credential.UserId,
            PullAllowed = credential.PullAllowed,
            PushAllowed = credential.PushAllowed
        };
    }

    /// <summary>
    /// Authenticates the user to the registry based on the username and the password
    /// </summary>
    /// <param name="registry">The target registry</param>
    /// <param name="username">The username</param>
    /// <param name="password">The password</param>
    /// <returns></returns>
    private async Task<AuthenticatedSubject> AuthenticatePassword(RegistryModel registry, string username,
        string password)
    {
        // if no username or password then not authenticated
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        // gets the credentials by username
        var credentials = await this.credentialService.GetBy(registry.Id, new RegistryCredentialFilter
        {
            Username = username
        });

        // filter the credentials having a matching password (the first one)
        var validCredential = credentials.FirstOrDefault(credential =>
        {
            // create a credential protector specific for the target credential's workspace if any
            var protector = this.credentialProtectionProvider.Create(credential.WorkspaceId);

            // unprotect and check
            return string.Equals(password, protector.Unprotect(credential.PasswordEncrypted));
        });

        // no credential with matching username and password
        if (validCredential == null)
        {
            return null;
        }

        // build an authenticated subject
        return new AuthenticatedSubject
        {
            RegistryId = registry.Id,
            CredentialId = validCredential.Id,
            Username = validCredential.Username,
            WorkspaceId = validCredential.WorkspaceId,
            UserId = validCredential.UserId,
            PullAllowed = validCredential.PullAllowed,
            PushAllowed = validCredential.PushAllowed
        };
    }

    /// <summary>
    /// Gets the allowed patterns for the subject
    /// </summary>
    /// <param name="subject">The subject</param>
    /// <returns></returns>
    private static IEnumerable<Regex> GetAllowedPatterns(AuthenticatedSubject subject)
    {
        // the resulting prefixes
        var result = new List<Regex>
        {
            new(@"^public\/.+$")
        };

        // if both workspace and user are given allow following path
        if (!string.IsNullOrWhiteSpace(subject.WorkspaceId) && !string.IsNullOrWhiteSpace(subject.UserId))
        {
            result.Add(new(@$"^w\/{subject.WorkspaceId}\/u\/{subject.UserId}\/[^\/]+$"));
        }

        // if workspace is given but no user specified allow the following path
        if (!string.IsNullOrWhiteSpace(subject.WorkspaceId))
        {
            result.Add(new(@$"^w\/{subject.WorkspaceId}\/[^\/]+$"));
        }
        
        return result;
    }

    /// <summary>
    /// Creates an authentication context 
    /// </summary>
    /// <param name="workspaceName">The workspace name</param>
    /// <param name="registry">The registry model</param>
    /// <returns></returns>
    private AuthenticationContext GetAuthenticationContext(string workspaceName, RegistryModel registry)
    {
        // the base url
        var baseUrl = selfSettings.ExternalBaseAddress;

        // ensure it ends with a slash
        if (!baseUrl.EndsWith('/'))
        {
            baseUrl = $"{baseUrl}/";
        }

        // build the context
        return new AuthenticationContext
        {
            Issuer = $"{baseUrl}api/authentication/{workspaceName}/{registry.Name}",
            Audience = registry.Registry
        };
    }
}