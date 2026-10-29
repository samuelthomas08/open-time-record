using System.Security.Cryptography;

namespace otr_backend.Services;

/// <summary>Creates the short numeric codes used for e-mail verification and invitations.</summary>
public static class VerificationCodeGenerator
{
    /// <summary>
    /// Returns a random six-digit code, e.g. <c>"482910"</c>. Uses a cryptographically secure
    /// generator because the code acts as a credential.
    /// </summary>
    public static string Generate()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    }
}
