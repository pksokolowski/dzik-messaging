using Dzik.crypto.algorithms;
using Dzik.crypto.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.protocols
{
    internal static class KeyAgreementPacker
    {
        internal static byte[] PackChallenge(DzikPublicKey publicKey, byte[] exchangeSymmetricKey)
        {
            var builder = new StringBuilder();

            builder.AppendLine(Base64PL.StringFromBytes(publicKey.Exponent));
            builder.AppendLine(Base64PL.StringFromBytes(publicKey.Modulus));
            builder.AppendLine(Base64PL.StringFromBytes(exchangeSymmetricKey));

            return Encoding.UTF8.GetBytes(builder.ToString());
        }

        internal static (DzikPublicKey publicKey, byte[] exchangeSymmetricKey) UnpackChallenge(byte[] challenge)
        {
            var lines = Encoding.UTF8.GetString(challenge).Split('\n');

            var exponent = Base64PL.BytesFromString(lines[0]);
            var modulus = Base64PL.BytesFromString(lines[1]);
            var exchangeSymKey = Base64PL.BytesFromString(lines[2]);

            return (new DzikPublicKey(exponent, modulus), exchangeSymKey);
        }


        internal static byte[] PackPrivateKey(DzikPrivateKey privateKey)
        {
            var builder = new StringBuilder();

            builder.AppendLine(Base64PL.StringFromBytes(privateKey.DecryptionExponent));
            builder.AppendLine(Base64PL.StringFromBytes(privateKey.Modulus));
            builder.AppendLine(Base64PL.StringFromBytes(privateKey.DP));
            builder.AppendLine(Base64PL.StringFromBytes(privateKey.DQ));
            builder.AppendLine(Base64PL.StringFromBytes(privateKey.PublicExponent));
            builder.AppendLine(Base64PL.StringFromBytes(privateKey.InverseQ));
            builder.AppendLine(Base64PL.StringFromBytes(privateKey.P));
            builder.AppendLine(Base64PL.StringFromBytes(privateKey.Q));

            return Encoding.UTF8.GetBytes(builder.ToString());
        }

        internal static DzikPrivateKey UnpackPrivateKey(byte[] privateKey)
        {
            var lines = Encoding.UTF8.GetString(privateKey).Split('\n');

            var DecryptionExponent = Base64PL.BytesFromString(lines[0]);
            var modulus = Base64PL.BytesFromString(lines[1]);
            var DP = Base64PL.BytesFromString(lines[2]);
            var DQ = Base64PL.BytesFromString(lines[3]);
            var PublicExponent = Base64PL.BytesFromString(lines[4]);
            var InverseQ = Base64PL.BytesFromString(lines[5]);
            var P = Base64PL.BytesFromString(lines[6]);
            var Q = Base64PL.BytesFromString(lines[7]);

            return new DzikPrivateKey(DecryptionExponent, modulus, DP, DQ, PublicExponent, InverseQ, P, Q);
        }
    }
}