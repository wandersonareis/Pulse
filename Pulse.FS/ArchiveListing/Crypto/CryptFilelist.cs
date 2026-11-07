using System;
using System.IO;

namespace Pulse.FS
{
    /// <summary>
    /// High level operations of the filelist crypto.
    /// Port of <c>WhiteBinTools.Crypto.CryptFilelist</c> and <c>WhiteBinTools.Filelist.FilelistCrypto</c>.
    /// <para>
    /// The crypto header of a filelist is 32 bytes long:
    /// 16 bytes of the MD5 seed, the big endian size of the body, the little endian
    /// encryption tag and 8 empty bytes. The body itself is followed by a 16 bytes
    /// trailer: the little endian body size, the checksum and 8 empty bytes.
    /// </para>
    /// </summary>
    internal static class CryptFilelist
    {
        public const uint EncryptionTag = 0x1DE03478;
        public const int CryptHeaderSize = 32;
        public const int TrailerSize = 16;

        /// <summary>
        /// Checks the presence of the crypto header by its encryption tag.
        /// </summary>
        public static bool HasCryptHeader(byte[] data)
        {
            if (data == null || data.Length < CryptHeaderSize)
                return false;

            return BitConverter.ToUInt32(data, 20) == EncryptionTag;
        }

        /// <summary>
        /// The size of the filelist body, stored as a big endian uint32 at the offset 16.
        /// </summary>
        public static uint GetBodySize(byte[] data)
        {
            if (data == null || data.Length < 20)
                throw new InvalidDataException("The filelist is too small to contain a crypto header.");

            return (uint)((data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19]);
        }

        /// <summary>
        /// Extracts the 8 bytes seed of the crypto from the MD5 hash stored in the crypto header.
        /// </summary>
        public static byte[] GetSeed(byte[] data)
        {
            if (data == null || data.Length < 16)
                throw new InvalidDataException("The filelist is too small to contain a crypto header.");

            byte[] md5 = new byte[16];
            Buffer.BlockCopy(data, 0, md5, 0, 16);

            return GetSeedFromMD5(md5);
        }

        public static byte[] GetSeedFromMD5(byte[] md5)
        {
            if (md5 == null || md5.Length < 16)
                throw new ArgumentException("The MD5 hash must be at least 16 bytes long.", nameof(md5));

            // The original implementation computes the seed with the signed 32 bits
            // arithmetic and converts the result to the 64 bits unsigned value,
            // so a negative seed is sign extended.
            int raw = (md5[9] << 24) | (md5[12] << 16) | (md5[2] << 8) | md5[0];

            return BitConverter.GetBytes((long)raw);
        }

        /// <summary>
        /// Checks if the body of the filelist is already decrypted.
        /// A decrypted filelist keeps the crypto header, so the little endian body size
        /// stored in the trailer is readable and matches the size stored in the header.
        /// </summary>
        public static bool IsPlainText(byte[] data)
        {
            if (!HasCryptHeader(data))
                return true;

            uint bodySize = GetBodySize(data);
            long checkOffset = CryptHeaderSize + bodySize;
            if (checkOffset + 4 > data.Length)
                return false;

            return BitConverter.ToUInt32(data, (int)checkOffset) == bodySize;
        }

        /// <summary>
        /// Decrypts the body of the filelist in memory. The crypto header is preserved as is.
        /// </summary>
        public static byte[] Decrypt(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (!HasCryptHeader(data))
                return data;

            uint bodySize = GetBodySize(data);
            uint cryptBodySize = bodySize + 8;

            if (cryptBodySize % 8 != 0)
                throw new InvalidDataException("The length of the body to decrypt is not valid.");

            byte[] result = new byte[data.Length];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);

            byte[] xorTable = Generator.GenerateXorTable(GetSeed(data));
            Decryption.DecryptBlocks(xorTable, cryptBodySize / 8, result, CryptHeaderSize);

            long hashOffset = CryptHeaderSize + bodySize + 4;
            if (hashOffset + 4 <= result.Length)
            {
                uint storedHash = BitConverter.ToUInt32(result, (int)hashOffset);
                uint computedHash = CryptoFunctions.ComputeCheckSum(result, bodySize / 4, CryptHeaderSize);

                if (storedHash != computedHash)
                    throw new InvalidDataException("The filelist was not decrypted correctly (checksum mismatch).");
            }

            return result;
        }

        /// <summary>
        /// Prepares the plaintext filelist for the encryption: pads the body to the multiple
        /// of 8 bytes, appends the trailer with the body size and the checksum.
        /// </summary>
        public static byte[] Encrypt(byte[] plaintext)
        {
            if (plaintext == null)
                throw new ArgumentNullException(nameof(plaintext));

            if (plaintext.Length < CryptHeaderSize)
                throw new InvalidDataException("The filelist is too small to contain a crypto header.");

            byte[] result = new byte[plaintext.Length];
            Buffer.BlockCopy(plaintext, 0, result, 0, plaintext.Length);

            uint bodySize = (uint)(result.Length - CryptHeaderSize);
            if (bodySize % 8 != 0)
            {
                int padding = 8 - (int)(bodySize % 8);
                Array.Resize(ref result, result.Length + padding);
                bodySize += (uint)padding;
            }

            Array.Resize(ref result, result.Length + TrailerSize);

            int sizeOffset = CryptHeaderSize + (int)bodySize;

            // the big endian body size of the crypto header
            result[16] = (byte)(bodySize >> 24);
            result[17] = (byte)(bodySize >> 16);
            result[18] = (byte)(bodySize >> 8);
            result[19] = (byte)bodySize;

            // the little endian body size of the trailer
            result[sizeOffset + 0] = (byte)bodySize;
            result[sizeOffset + 1] = (byte)(bodySize >> 8);
            result[sizeOffset + 2] = (byte)(bodySize >> 16);
            result[sizeOffset + 3] = (byte)(bodySize >> 24);

            uint checksum = CryptoFunctions.ComputeCheckSum(result, bodySize / 4, CryptHeaderSize);
            result[sizeOffset + 4] = (byte)checksum;
            result[sizeOffset + 5] = (byte)(checksum >> 8);
            result[sizeOffset + 6] = (byte)(checksum >> 16);
            result[sizeOffset + 7] = (byte)(checksum >> 24);

            byte[] xorTable = Generator.GenerateXorTable(GetSeed(result));
            Encryption.EncryptBlocks(xorTable, (bodySize + 8) / 8, result, CryptHeaderSize);

            return result;
        }
    }
}
