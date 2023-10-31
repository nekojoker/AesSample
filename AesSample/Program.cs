using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace AesSample;

class Program
{
    private const string StrKey = "9ojffFV/YtBvZZJ48ytVv5zkPyJIb0m0teXE8M3xd0U=";

    static async Task Main(string[] args)
    {
        // Generate Base64 Key Sample
        //using (var aes = Aes.Create())
        //{
        //    Console.WriteLine(Convert.ToBase64String(aes.Key));
        //}

        var plainText = "Hello World!!";
        Console.WriteLine($"PlainText: {plainText}");

        var encrypt = await EncryptAsync(plainText);
        Console.WriteLine($"Encrypt: {encrypt}");

        var cipherText = Convert.FromBase64String(encrypt);
        var decrypt = await DecryptAsync(cipherText);
        Console.WriteLine($"Decrypt: {decrypt}");
    }


    private static async ValueTask<string> EncryptAsync(string plainText, CancellationToken cancellationToken = default)
    {
        var key = Convert.FromBase64String(StrKey);

        using (var aes = Aes.Create())
        {
            var encryptor = aes.CreateEncryptor(key, aes.IV);
            using (var ms = new MemoryStream())
            await using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                await ms.WriteAsync(aes.IV, cancellationToken);
                await using (var sw = new StreamWriter(cs))
                {
                    await sw.WriteAsync(plainText);
                }
                var bytes = ms.ToArray();
                return Convert.ToBase64String(bytes);
            }
        }
    }


    private static async ValueTask<string> DecryptAsync(byte[] cipherText, CancellationToken cancellationToken = default)
    {
        var key = Convert.FromBase64String(StrKey);
        var iv = new byte[16];
        Array.Copy(cipherText, 0, iv, 0, iv.Length);

        using (var aes = Aes.Create())
        {
            var decryptor = aes.CreateDecryptor(key, iv);
            using (var ms = new MemoryStream())
            {
                await using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                await using (var bw = new BinaryWriter(cs))
                {
                    bw.Write(cipherText, iv.Length, cipherText.Length - iv.Length);
                }
                var bytes = ms.ToArray();
                return System.Text.Encoding.Default.GetString(bytes);
            }
        }
    }
}