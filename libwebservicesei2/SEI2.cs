using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
//using System.Xml.Linq;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Dynamics.AX.Application;

using Org.BouncyCastle.OpenSsl;

using static System.String;

namespace OpenMedicus.WebService.SEI2
{
	public static class SEI2Certificates
	{
		public const string IDWS_WSP_Preprod = "IDWS_WSP_Preprod";
		public const string IDWS_WSP_Prod = "IDWS_WSP_Prod";
		public const string IDWS_WSP_Test = "IDWS_WSP_Test";
		public const string STS = "STS";
		public const string TRUST2048_Systemtest_VII_Primary_CA = "TRUST2048_Systemtest_VII_Primary_CA";
		public const string TRUST2048_Systemtest_XIX_CA = "TRUST2048_Systemtest_XIX_CA";
		public const string TRUST2408_Systemtest_XXII_CA = "TRUST2408_Systemtest_XXII_CA";

		public const string Charlotte_Henriksen_RID_18756718 = "Charlotte_Henriksen_RID_18756718";

		public const string SEI2_IDWS_CLIENT = "SEI2_IDWS_CLIENT";
		public const string seiidws_testclient_sundhedsdata_dk = "seiidws_testclient_sundhedsdata_dk";
		
		public const string XMedicus_Systems_ApS_IDWS_Test = "XMedicus_Systems_ApS_IDWS_Test";

		public static byte[] ReadFully(Stream input)
		{
			byte[] buffer = new byte[16*1024];
			using (MemoryStream ms = new MemoryStream())
			{
				int read;
				while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, read);
				}
				return ms.ToArray();
			}
		}

		public static byte[] GetBytesFromPEM (string pem)
		{
			var pattern = new Regex (@"^[-]{5}BEGIN CERTIFICATE[-]{5}(?<certificate>([^-]*))[-]{5}END CERTIFICATE[-]{5}", RegexOptions.Multiline);

			if (!pattern.IsMatch (pem))
				throw new ArgumentException ("Certificate malformed. (No certitficates found)");

			//Read all certificates to a jagged byte array
			MatchCollection mc = pattern.Matches (pem);
			foreach (Match match in mc)
			{
				return Convert.FromBase64String (match.Groups["certificate"].ToString ().Trim (Environment.NewLine.ToCharArray ()));
			}

			return null;
		}


		public static X509Certificate2 GetCertificate (string name, string password = "Test1234")
		{
			X509Certificate2 cert;
			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
			{
				if (stream == null) return null;

				switch (name)
				{
					case IDWS_WSP_Preprod:
					case IDWS_WSP_Prod:
					case IDWS_WSP_Test:
						using (StreamReader reader = new StreamReader(stream))
						{
							cert = new X509Certificate2(GetBytesFromPEM (reader.ReadToEnd ()));
						}
						break;
					case SEI2_IDWS_CLIENT:
						password = "tHX4MbreM7qWWAXY";
						cert = new X509Certificate2 (ReadFully (stream), password, X509KeyStorageFlags.Exportable);
						break;
					case Charlotte_Henriksen_RID_18756718:
					case XMedicus_Systems_ApS_IDWS_Test:
						//cert = CertificateUtils.Load (stream, password);
						//break;
					default:
						cert = new X509Certificate2 (ReadFully (stream), password, X509KeyStorageFlags.Exportable);
						break;
				}
			}

			if (cert.PrivateKey is RSACryptoServiceProvider privKey)
			{
				var exported = privKey.ToXmlString(true);

				var cspParams = new CspParameters
				{
					ProviderType = 24,
					ProviderName = "Microsoft Enhanced RSA and AES Cryptographic Provider"
				};

				var newPrivKey = new RSACryptoServiceProvider(cspParams);
				newPrivKey.FromXmlString(exported);

				cert.PrivateKey = privKey;
			}

			return cert;
		}
	}

	public class AbortReport : SEI2SchemaAbortionContract
	{
	}

	public class IVFReport : SEI2SchemaIVFContract
	{
	}

	public class SEI2Service : SEI2ReportClient
	{
		//Specify the binding to be used for the client.
		static WSHttpBinding binding = new WSHttpBinding (SecurityMode.Transport);

		static string LogPath;

		SimpleEndpointBehavior behavior = new SimpleEndpointBehavior ();

		SEI2Service (EndpointAddress endpoint) : base (binding, endpoint)
		{
//			address = endpoint;
		}

		public SEI2Service (string url, string logPath = null) : base (binding, new EndpointAddress (url))
		{
			Endpoint.EndpointBehaviors.Add (behavior);

			LogPath = logPath;

			if (!IsNullOrWhiteSpace (LogPath))
				File.Create (LogPath).Close ();
//
//			var r = Assembly.GetExecutingAssembly ().GetCustomAttributes (typeof(AssemblyInformationalVersionAttribute), true);
//			var aiva = (AssemblyInformationalVersionAttribute)r[0];
//			SystemVersion = aiva.InformationalVersion;
		}

		// Endpoint behavior
		public class SEI2MessageInspector : IClientMessageInspector
		{
			public void AfterReceiveReply (ref Message reply, object correlationState)
			{
				// Implement this method to inspect/modify messages after a message
				// is received but prior to passing it back to the client
				Console.WriteLine ("AfterReceiveReply called");

				if (!IsNullOrWhiteSpace (LogPath) && File.Exists (LogPath))
					File.AppendAllText (LogPath, reply.ToString ());

				Console.WriteLine (reply.ToString ());
			}

			public object BeforeSendRequest (ref Message request, IClientChannel channel)
			{
				Console.WriteLine ("BeforeSendRequest");

				if (request != null)
				{
					Console.WriteLine (request.ToString());
				}
				else
				{
					Console.WriteLine ("NULLLLLLLLLLLLLLLLLLL");
				}

				return null;
			}
		}

		public class SimpleEndpointBehavior : IEndpointBehavior
		{
			SEI2MessageInspector inspector = new SEI2MessageInspector ();

			public void ApplyClientBehavior (ServiceEndpoint endpoint, ClientRuntime clientRuntime) => clientRuntime.MessageInspectors.Add (inspector);

			public void AddBindingParameters (ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
			{
			}

			public void ApplyDispatchBehavior (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
			{
			}

			public void Validate (ServiceEndpoint endpoint)
			{
			}
		}
	}
}
