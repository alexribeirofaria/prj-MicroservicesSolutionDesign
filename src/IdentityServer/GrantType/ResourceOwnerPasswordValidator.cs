using IdentityModel;
using IdentityServer4.Validation;
using EasyCryptoSalt;
using STS.ServerData.Interfaces;

namespace STS.ServerGrantType;

internal class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
{
    private readonly IIdentityRepository repository;
    private readonly ICrypto _crypto;

    public ResourceOwnerPasswordValidator(IIdentityRepository repository, ICrypto crypto)
    {
        this.repository = repository;
        this._crypto = crypto;
    }

    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        var email = context.UserName;
        var user = await this.repository.FindByEmail(email);

        if (user is not null && _crypto.Verify(context.Password, user.Senha))
        {
            context.Result = new GrantValidationResult(user.Id.ToString(), OidcConstants.AuthenticationMethods.Password);
        }
    }
}
