using System.Security.Cryptography;
using System.Text;

namespace backend.Algorithms;

public class SHAImplementation
{
    public static string Hash(string randomString)
    {
        byte[] crypto = SHA256.HashData(Encoding.ASCII.GetBytes(randomString));
        return Convert.ToHexStringLower(crypto);
    }
}