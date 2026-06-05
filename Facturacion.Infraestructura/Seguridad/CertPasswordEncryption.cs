using System.Security.Cryptography;
using System.Text;

namespace Facturacion.Infraestructura.Seguridad;

/// <summary>
/// Cifrado AES-256-GCM para la contraseña del certificado P12.
/// Inicializar una sola vez al arranque con la clave de 32 bytes desde el secrets manager.
/// Formato almacenado: base64(nonce[12] + tag[16] + ciphertext)
/// </summary>
public static class CertPasswordEncryption
{
    private static byte[] _key = [];

    public static void Initialize(byte[] key)
    {
        if (key.Length != 32)
            throw new InvalidOperationException("La clave de cifrado debe ser de 32 bytes (AES-256).");
        _key = key;
    }

    public static string Encrypt(string plaintext)
    {
        if (_key.Length == 0)
            throw new InvalidOperationException("CertPasswordEncryption no fue inicializado. Configura Encryption:CertPasswordKey.");

        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        var result = new byte[nonce.Length + tag.Length + ciphertext.Length];
        nonce.CopyTo(result, 0);
        tag.CopyTo(result, nonce.Length);
        ciphertext.CopyTo(result, nonce.Length + tag.Length);

        return Convert.ToBase64String(result);
    }

    public static string Decrypt(string encryptedBase64)
    {
        if (_key.Length == 0)
            throw new InvalidOperationException("CertPasswordEncryption no fue inicializado. Configura Encryption:CertPasswordKey.");

        var data = Convert.FromBase64String(encryptedBase64);
        var nonceSize = AesGcm.NonceByteSizes.MaxSize;
        var tagSize = AesGcm.TagByteSizes.MaxSize;

        var nonce = data[..nonceSize];
        var tag = data[nonceSize..(nonceSize + tagSize)];
        var ciphertext = data[(nonceSize + tagSize)..];
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, tagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    // Formato: nonce[12] + tag[16] + ciphertext
    public static byte[] EncryptBytes(byte[] data)
    {
        if (_key.Length == 0)
            throw new InvalidOperationException("CertPasswordEncryption no fue inicializado. Configura Encryption:CertPasswordKey.");

        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[data.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, data, ciphertext, tag);

        var result = new byte[nonce.Length + tag.Length + ciphertext.Length];
        nonce.CopyTo(result, 0);
        tag.CopyTo(result, nonce.Length);
        ciphertext.CopyTo(result, nonce.Length + tag.Length);
        return result;
    }

    public static byte[] DecryptBytes(byte[] encrypted)
    {
        if (_key.Length == 0)
            throw new InvalidOperationException("CertPasswordEncryption no fue inicializado. Configura Encryption:CertPasswordKey.");

        var nonceSize = AesGcm.NonceByteSizes.MaxSize;
        var tagSize = AesGcm.TagByteSizes.MaxSize;

        var nonce = encrypted[..nonceSize];
        var tag = encrypted[nonceSize..(nonceSize + tagSize)];
        var ciphertext = encrypted[(nonceSize + tagSize)..];
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, tagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return plaintext;
    }
}
