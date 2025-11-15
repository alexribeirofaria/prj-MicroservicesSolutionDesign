using System.Security.Cryptography.X509Certificates;

namespace STS.Server.Autorization;

public static class SigningCredential
{

    public static X509Certificate2? LoadCertificate(string certificateName, string password)
    {
        if (certificateName == null || string.IsNullOrWhiteSpace(certificateName))
            return null;

        string certificatePath = Path.Combine(AppContext.BaseDirectory, certificateName);

        if (!File.Exists(certificatePath))
            return null;

        return X509CertificateLoader.LoadPkcs12FromFile(
            certificatePath,
            password ?? string.Empty,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet
        );
    }
}
