using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace OpenMedicus
{
	public static class CertificateUtils
	{
		public static X509Certificate2 Load (Stream stream, string password)
		{
			var loadStore = new Pkcs12Store ();
			loadStore.Load (stream, password?.ToCharArray());

			string keyAlias = loadStore.Aliases.Cast<string> ().FirstOrDefault (loadStore.IsKeyEntry);

			if (keyAlias == null)
				throw new NotImplementedException ("Alias");

			var builder = new Pkcs12StoreBuilder ();
			builder.SetUseDerEncoding (true);
			var saveStore = builder.Build ();

			var chain = new X509CertificateEntry (loadStore.GetCertificate (keyAlias).Certificate);
			saveStore.SetCertificateEntry ("Alias", chain);
			saveStore.SetKeyEntry ("Alias", new AsymmetricKeyEntry ((RsaPrivateCrtKeyParameters)loadStore.GetKey (keyAlias).Key), new [] { chain });

			using (var saveStream = new MemoryStream ())
			{
				saveStore.Save (saveStream, new char[0], new SecureRandom ());
				return new X509Certificate2 (Pkcs12Utilities.ConvertToDefiniteLength (saveStream.ToArray ()));
			}
		}

		public static X509Certificate2 LoadFromEncryptedBase64 (string base64, string password)
		{
			using (var stream = new MemoryStream (Convert.FromBase64String (base64)))
			{
				return Load (stream, password);
			}
		}

		public static X509Certificate2 LoadFromUnencryptedPEM (string pem)
		{
			//Extract certificate
			var pattern = new Regex (@"^[-]{5}BEGIN CERTIFICATE[-]{5}(?<certificate>([^-]*))[-]{5}END CERTIFICATE[-]{5}", RegexOptions.Multiline);
			var pvk_pattern = new Regex (@"[-]{5}BEGIN(?<encrypted>( ENCRYPTED)?) PRIVATE KEY[-]{5}(?<key>([^-]*))[-]{5}END( ENCRYPTED)? PRIVATE KEY[-]{5}", RegexOptions.Multiline);

			if (!pattern.IsMatch (pem))
				throw new ArgumentException ("Certificate malformed. (No certitficates found)");
			if (!pvk_pattern.IsMatch (pem))
				throw new ArgumentException ("Certificate malformed. (No private key found)");

			//Read all certificates to a jagged byte array
			MatchCollection mc = pattern.Matches (pem);
			var certificates = new byte[mc.Count][];
			var index = 0;
			foreach (Match match in mc)
			{
				certificates[index] = Convert.FromBase64String (match.Groups["certificate"].ToString ().Trim (Environment.NewLine.ToCharArray ()));
				index++;
			}
			//If the private key is encrypted (check on "encrypted" group) then that need to be handled in future, probably never.
			Match pvk_match = pvk_pattern.Match (pem);
			string pvk = pvk_match.Groups["key"].ToString ();

			var parser = new X509CertificateParser ();
			var parsedCertificate = parser.ReadCertificate (Combine (certificates));

			var builder = new Pkcs12StoreBuilder ();
			builder.SetUseDerEncoding (true);
			var inputKeyStore = builder.Build ();

			inputKeyStore.SetCertificateEntry ("Alias", new X509CertificateEntry (parsedCertificate));
			inputKeyStore.SetKeyEntry ("Alias", new AsymmetricKeyEntry ((RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey (Convert.FromBase64String (pvk))), new [] { new X509CertificateEntry (parsedCertificate) });

			string keyAlias = inputKeyStore.Aliases.Cast<string> ().FirstOrDefault (inputKeyStore.IsKeyEntry);

			if (keyAlias == null)
				throw new InvalidKeyException ("Alias");

			using (var stream = new MemoryStream ())
			{
				//There is no password
				inputKeyStore.Save (stream, new char[0], new SecureRandom ());
				return new X509Certificate2 (Pkcs12Utilities.ConvertToDefiniteLength(stream.ToArray ()));
			}
		}

		/// <summary>
		/// Returns the encrypted Base64 string of the certificate chain+key. Subject is a string containing the name of the owner etc.
		///
		/// Example usage:
		///
		/// var res = TryParseNemID (File.ReadAllBytes ($PATH$));
		/// if (res.Success)
		///  do stuff
		///
		/// </summary>
		public static (bool Success, string Base64, string Subject, string TemporaryPassword) TryParseNemID (byte[] raw, bool keepPassword = false, bool generateTemporaryPassword = false, string certificatePassword = null)
		{
			try
			{
				string ascii = System.Text.Encoding.ASCII.GetString (raw);
				var ex = new Regex ("pkcs12=\"(?<data>[A-Za-z0-9+/=\\s]*?)\";", RegexOptions.Multiline);
				bool isHTML = ex.IsMatch (ascii); //Assume the file was a NemID HTML file.

				string base64;
				if (keepPassword)
				{
					base64 = isHTML ? ex.Match (ascii).Groups["data"].ToString ().Replace ("\n", "") : Convert.ToBase64String (raw);

					return (true, base64, null, null);
				}

				var inputKeyStore = new Pkcs12Store ();

				if (isHTML)
				{
					string data = ex.Match (ascii).Groups["data"].ToString ().Replace ("\n", "");
					var bytes = Convert.FromBase64String (data);

					using (var keyStream = new MemoryStream (bytes))
					{
						inputKeyStore.Load (keyStream, certificatePassword == null ? new char[0] : certificatePassword.ToCharArray ());
					}
				}
				else
				{
					using (var keyStream = new MemoryStream (raw))
					{
						inputKeyStore.Load (keyStream, certificatePassword == null ? new char[0] : certificatePassword.ToCharArray ());
					}
				}

				string keyAlias = inputKeyStore.Aliases.Cast<string> ().FirstOrDefault (inputKeyStore.IsKeyEntry);

				if (keyAlias == null)
					throw new NotImplementedException ("Alias");

				Org.BouncyCastle.X509.X509Certificate certificate = inputKeyStore.GetCertificate (keyAlias).Certificate;
				var certf = new X509Certificate2 (DotNetUtilities.ToX509Certificate (certificate));

				string subject = certf.Subject;

				if (generateTemporaryPassword)
				{
					string temporaryPassword = "Test1234";

					using (var ms = new MemoryStream ())
					{
						inputKeyStore.Save (ms, temporaryPassword.ToCharArray (), new SecureRandom ());

						return (true, Convert.ToBase64String (ms.ToArray ()), subject, temporaryPassword);
					}
				}

				base64 = isHTML ? ex.Match (ascii).Groups["data"].ToString ().Replace ("\n", "") : Convert.ToBase64String (raw);

				return (true, base64, subject, null);
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex);
				Console.WriteLine (ex.InnerException);
			}

			return (false, null, null, null);
		}

		static byte[] Combine (params byte[][] arrays)
		{
			var rv = new byte[arrays.Sum (a => a.Length)];
			var offset = 0;
			foreach (byte[] array in arrays)
			{
				Buffer.BlockCopy (array, 0, rv, offset, array.Length);
				offset += array.Length;
			}
			return rv;
		}
	}
}
