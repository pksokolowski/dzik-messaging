using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.algorithms
{
    internal class RsaTool
    {
        private const int KeySize = 16384;

        public static KeyPair GenerateKeyPair()
        {
            RSA rsa = RSA.Create(KeySize);
            return new KeyPair(rsa.ExportParameters(true));
        }

        public static byte[] Encrypt(DzikPublicKey key, byte[] messageBytes)
        {
            try
            {
                RSA rsa = RSA.Create(KeySize);
                rsa.ImportParameters(key.getRsaParams());

                var encryptedMessageBytes = rsa.Encrypt(messageBytes, RSAEncryptionPadding.OaepSHA512);
                return encryptedMessageBytes;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static byte[] Decrypt(DzikPrivateKey key, byte[] encryptedMessageBytes)
        {
            try
            {
                RSA rsa = RSA.Create(KeySize);
                rsa.ImportParameters(key.getRsaParams());

                var decryptedMessageBytes = rsa.Decrypt(encryptedMessageBytes, RSAEncryptionPadding.OaepSHA512);
                return decryptedMessageBytes;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
    internal class KeyPair
    {
        internal DzikPrivateKey privateKey;
        internal DzikPublicKey publicKey;


        public KeyPair(RSAParameters rsaParameters)
        {
            this.privateKey = new DzikPrivateKey(rsaParameters);
            this.publicKey = new DzikPublicKey(rsaParameters);
        }
    }

    internal class DzikPublicKey
    {
        internal byte[] Exponent;
        internal byte[] Modulus;

        public DzikPublicKey(byte[] exponent, byte[] modulus)
        {
            Exponent = exponent;
            Modulus = modulus;
        }

        public DzikPublicKey(RSAParameters rsaParameters)
        {
            this.Exponent = rsaParameters.Exponent;
            this.Modulus = rsaParameters.Modulus;
        }

        public RSAParameters getRsaParams()
        {
            return new RSAParameters()
            {
                Modulus = Modulus,
                Exponent = Exponent,
            };
        }
    }

    internal class DzikPrivateKey
    {
        internal byte[] DecryptionExponent;
        internal byte[] Modulus;
        internal byte[] DP;
        internal byte[] DQ;
        internal byte[] PublicExponent;
        internal byte[] InverseQ;
        internal byte[] P;
        internal byte[] Q;

        public DzikPrivateKey(byte[] decryptionExponent, byte[] modulus, byte[] dP, byte[] dQ, byte[] publicExponent, byte[] inverseQ, byte[] p, byte[] q)
        {
            DecryptionExponent = decryptionExponent;
            Modulus = modulus;
            DP = dP;
            DQ = dQ;
            PublicExponent = publicExponent;
            InverseQ = inverseQ;
            P = p;
            Q = q;
        }

        public DzikPrivateKey(RSAParameters rsaParameters)
        {
            this.DecryptionExponent = rsaParameters.D;
            this.Modulus = rsaParameters.Modulus;
            this.DP = rsaParameters.DP;
            this.DQ = rsaParameters.DQ;
            this.PublicExponent = rsaParameters.Exponent;
            this.InverseQ = rsaParameters.InverseQ;
            this.P = rsaParameters.P;
            this.Q = rsaParameters.Q;
        }

        public RSAParameters getRsaParams()
        {
            return new RSAParameters()
            {
                D = DecryptionExponent,
                Modulus = Modulus,
                DP = DP,
                DQ = DQ,
                Exponent = PublicExponent,
                InverseQ = InverseQ,
                P = P,
                Q = Q
            };
        }
    }
}
