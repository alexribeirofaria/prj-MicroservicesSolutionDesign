namespace STS.ServerModel;

internal class User
{
    internal enum UserTypeValues
    {
        Invalid = 0,
        Administrador = 1,
        Usuario = 2
    }

    public string Id { get; set; }
    public string? Email {  get; set; }
    public string? Senha {  get; set; }
    public int PerfilUsuarioId { get; set; }
    public UserTypeValues UserType { get => (UserTypeValues)this.PerfilUsuarioId; }
}
