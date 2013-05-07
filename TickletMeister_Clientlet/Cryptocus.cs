using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace TickletMeister_Clientlet
{

    public class PublicKey
    {
        public const String KEYCOMMAND = "zzTop";
    }

    public class Cryptocus
    {
        private RSACryptoServiceProvider rsa;
        private String myPrivateKey;
        private String myPublicKey;

        public Cryptocus()
        {
            //const int KEYSIZE = 1912;
            //const int KEYSIZE = 3072;
            //const int KEYSIZE = 1536; //max of 181 characters 
            //const int KEYSIZE = 1600; //max of 189 characters 
           // const int KEYSIZE = 1824; //max of 217 characters
           // const int KEYSIZE = 2864;
            //const int KEYSIZE = 2928;
            const int KEYSIZE = 3528 + 88;
            
            
            /*
             * RSA tutorial code (for initiation)
             * */
            //const int PROVIDER_RSA_FULL = 1;
            //const string CONTAINER_NAME = "KeyContainer";
            //CspParameters cspParams;
            //cspParams = new CspParameters(PROVIDER_RSA_FULL);
            //cspParams.KeyContainerName = CONTAINER_NAME;
            //cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            //cspParams.ProviderName = "Microsoft Strong Cryptographic Provider";
            rsa = new RSACryptoServiceProvider(KEYSIZE);
           
            /*
             * store mah keys
             * */
            myPrivateKey = rsa.ToXmlString(true);
            myPublicKey = rsa.ToXmlString(false);
        }
        public byte[] encrypt(byte[] raw, String key)
        {
            lock (rsa)
            {
                rsa.FromXmlString(key);
                byte[] encrypted = rsa.Encrypt(raw, false);
                return encrypted;
            }
        }
        public byte[] decrypt(byte[] encrypted)
        {
            lock (rsa)
            {
                rsa.FromXmlString(myPrivateKey);
                byte[] decrypted = rsa.Decrypt(encrypted, false);
                return decrypted;
            }

        }
        public String getPublicKey()
        {
            return myPublicKey;
        }
        
    }
}
