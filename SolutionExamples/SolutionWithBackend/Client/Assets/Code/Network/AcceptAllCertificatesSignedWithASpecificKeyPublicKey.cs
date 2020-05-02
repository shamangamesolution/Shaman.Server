using UnityEngine.Networking;

namespace Code.Network
{
	public class AcceptAllCertificatesSignedWithASpecificKeyPublicKey : CertificateHandler {
 
		// Encoded RSAPublicKey
		//private static string PUB_KEY = "mypublickey";
		protected override bool ValidateCertificate(byte[] certificateData)
		{
//		X509Certificate2 certificate = new X509Certificate2(certificateData);
//		string pk = certificate.GetPublicKeyString();
//		if (pk.ToLower().Equals(PUB_KEY.ToLower()))
			return true;
//		return false;
		}
	}
}
