namespace TestSSLError.Server.Services;

/// <summary>
/// Отвечает за работу с сертификатом
/// </summary>
internal static class CertificateProvider
{
    /// <summary>
    /// Создаёт (если ещё не создан) сертификат и возвращает его
    /// </summary>
    /// <returns>Сертификат</returns>
    public static X509Certificate2 GetOrCreateCertficate()
    {
        using RSA rsa = RSA.Create(2048);
        CertificateRequest request = new CertificateRequest("CN=TestSSLError", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") },
                true
            )
        );

        X509Certificate2 certificate = request.CreateSelfSigned(
            DateTimeOffset.Now.AddDays(-1),
            DateTimeOffset.Now.AddYears(1)
        );

        return new X509Certificate2(
            certificate.Export(X509ContentType.Pfx),
            (string?)null,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet
        );
    }
}