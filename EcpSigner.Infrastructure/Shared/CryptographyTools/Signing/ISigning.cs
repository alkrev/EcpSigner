namespace CryptographyTools.Signing
{
    public interface ISigning
    {
        string Sign(CAPICOM.ICertificate certificate, string docBase64);
    }
}
