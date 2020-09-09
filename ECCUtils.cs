using System;
using System.Text;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.IO;
using Org.BouncyCastle.Utilities.Encoders;

namespace VotedDecode
{
    public static class ECCUtils
    {
        public static void AA()
        {
            X9ECParameters ecP = NistNamedCurves.GetByName("P-256");
            ECDomainParameters ecSpec = new ECDomainParameters(ecP.Curve, ecP.G, ecP.N, ecP.H, ecP.GetSeed());
            ECKeyPairGenerator g = new ECKeyPairGenerator();
            g.Init(new ECKeyGenerationParameters(ecSpec, new SecureRandom()));
            AsymmetricCipherKeyPair server = g.GenerateKeyPair();
            ECPublicKeyParameters publicKey = (ECPublicKeyParameters)server.Public;
            ECPrivateKeyParameters privateKey = (ECPrivateKeyParameters)server.Private;

            var ps = Encoding.Default.GetString(Hex.Encode(publicKey.Q.GetEncoded())).ToUpper();
            var ps1 = Encoding.Default.GetString(Hex.Encode(privateKey.D.ToByteArray())).ToUpper();

            string k1 = "";
            string k2 = "";
            using (TextWriter sw = new StringWriter())
            {
                var pemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(sw);
                pemWriter.WriteObject(publicKey);
                pemWriter.Writer.Flush();
                k1 = sw.ToString();
            }
            using (TextWriter sw = new StringWriter())
            {
                var pemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(sw);
                pemWriter.WriteObject(privateKey);
                pemWriter.Writer.Flush();
                k2 = sw.ToString();
            }
            string ss = "Hello";
            byte[] sb = System.Text.Encoding.UTF8.GetBytes(ss);
            var sg = Sign(sb, privateKey);
            var sf = Verify(sg, sb, publicKey);

        }

        public static string PemToKey(string pemKey, bool isPrivateKey)
        {
            string rsaKey = string.Empty;
            object pemObject = null;
            using (StringReader sReader = new StringReader(pemKey))
            {
                var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(sReader);
                pemObject = pemReader.ReadObject();
            }
            if (isPrivateKey)
            {
                ECPrivateKeyParameters key = (ECPrivateKeyParameters)((AsymmetricCipherKeyPair)pemObject).Private;
            }
            else
            {
                ECPublicKeyParameters key = (ECPublicKeyParameters)pemObject;
            }
            return "";
        }

        public static byte[] Sign(byte[] signedData, ECPrivateKeyParameters privateKey)
        {
            if (signedData == null)
            {
                throw new ArgumentNullException(nameof(signedData));
            }
            if (privateKey == null)
            {
                throw new ArgumentNullException(nameof(privateKey));
            }
            var signature = SignerUtilities.GetSigner("SHA-256withECDSA");
            signature.Init(true, privateKey);
            signature.BlockUpdate(signedData, 0, signedData.Length);
            return signature.GenerateSignature();
        }

        public static bool Verify(byte[] data, byte[] signedData, ECPublicKeyParameters publicKey)
        {
            if (signedData == null)
            {
                throw new ArgumentNullException(nameof(signedData));
            }
            if (publicKey == null)
            {
                throw new ArgumentNullException(nameof(publicKey));
            }
            var signature = SignerUtilities.GetSigner("SHA-256withECDSA");
            signature.Init(false, publicKey);
            signature.BlockUpdate(signedData, 0, signedData.Length);
            return signature.VerifySignature(data);
        }

        public static byte[] Encrypt(byte[] data, ECPublicKeyParameters pk)
        {
            byte[] output = null;
            try
            {
                KeyParameter keyparam = ParameterUtilities.CreateKeyParameter("ECIES", pk.PublicKeyParamSet.GetEncoded());
                IBufferedCipher cipher = CipherUtilities.GetCipher("BC");
                cipher.Init(true, keyparam);
                try
                {
                    output = cipher.DoFinal(data);
                    return output;
                }
                catch (System.Exception ex)
                {
                    throw new CryptoException("Invalid Data");
                }
            }
            catch (Exception ex)
            {

            }
            return output;
        }

        public static byte[] Decrypt(byte[] cipherData, byte[] derivedKey)
        {
            byte[] output = null;
            try
            {
                KeyParameter keyparam = ParameterUtilities.CreateKeyParameter("DES", derivedKey);
                IBufferedCipher cipher = CipherUtilities.GetCipher("DES/ECB/ISO7816_4PADDING");
                cipher.Init(false, keyparam);
                try
                {
                    output = cipher.DoFinal(cipherData);
                }
                catch (System.Exception ex)
                {
                    throw new CryptoException("Invalid Data");
                }
            }
            catch (Exception ex)
            {
            }
            return output;
        }
    }
}
