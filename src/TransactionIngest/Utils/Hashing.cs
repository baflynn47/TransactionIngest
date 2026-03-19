using System.Security.Cryptography;
using System.Text;

namespace TransactionIngest.Utils;

public static class Hashing
{
    public static string Last4(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
            return "????";

        return cardNumber[^4..];
    }
}