using STS.ServerModel;

namespace STS.ServerData.Interfaces;

internal interface IIdentityRepository
{
    Task<User> FindByEmail(string email);
    Task<User> FindByIdAsync(Guid Id);
}