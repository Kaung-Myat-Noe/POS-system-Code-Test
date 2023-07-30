using System.Security.Cryptography.X509Certificates;

namespace pos.sys.Common
{
    public class Security
    {
        public static X509Certificate2 GetCertificateFromStore(string thumbprint)
        {
            X509Store store = new X509Store(StoreLocation.LocalMachine); // comment
                                                                         //var x509Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(AppDomain.CurrentDomain.BaseDirectory + "/publickey.cer"); // used docker
            try
            {
                store.Open(OpenFlags.ReadOnly); // comment
                X509Certificate2Collection certCollection = store.Certificates;
                //X509Certificate2Collection certCollection = new X509Certificate2Collection();  // when used docker
                //certCollection.Add(x509Certificate); // when used docker
                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (signingCert.Count == 0)
                    return null;
                return signingCert[0];
            }
            finally
            {
                store.Close();
            }
        }
    }
}
