using Dapper;
using STS.ServerData.Interfaces;
using STS.ServerData.Options;
using STS.ServerModel;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace STS.ServerData;

internal class IdentityRepository : IIdentityRepository
{
    private readonly string? connectionString;

    public IdentityRepository(IOptions<DataBaseOptions> options)
    {
        this.connectionString = options.Value.DefaultConnectionString;
    }

    public async Task<User> FindByIdAsync(Guid Id)
    {
        using (var connection = new MySqlConnection(this.connectionString))
        {
            var user = await connection.QueryFirstAsync<User>(IdentityQuery.FindById(), new { id = Id });
            return user;
        }
    }

    public async Task<User> FindByEmail(string email)
    {
        using (var connection = new MySqlConnection(this.connectionString))
        {
            var user = await connection.QueryFirstOrDefaultAsync<User>(IdentityQuery.FindByEmail(), new { email = email });
            return user;
        }
    }
}

internal static class IdentityQuery
{
    public static string FindById() =>
        @"SELECT Id
                ,PerfilUsuarioId
	            ,Email
          FROM usuario  
          WHERE id = @id";

    public static string FindByEmail() =>
        @"SELECT 
            u.Id,
            u.PerfilUsuarioId,
            u.Email,
            c.Senha
          FROM usuario AS u
          LEFT JOIN controleacesso AS c 
               ON u.Id = c.UsuarioId 
          WHERE email = @email";
}