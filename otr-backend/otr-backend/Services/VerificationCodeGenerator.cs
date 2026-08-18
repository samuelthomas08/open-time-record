namespace otr_backend.Services;

public static class VerificationCodeGenerator
{
    public static string Generate()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }
}
