using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Xml.Serialization;
using Digst.OioIdws.OioWsTrust.Utils;
using Digst.OioIdws.Rest.Client;
using NUnit.Framework;
using OpenMedicus.WebService.SEI2ReportProxy;
using tempuri.org;

namespace SEI2
{
  [TestFixture]
  public class Tests
  {
    [Test]
    public void Test1()
    {
      CryptoConfig.AddAlgorithm(typeof(RsaPkcs1Sha256SignatureDescription), "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
      
      var stsSettings = new OioIdwsStsSettings
      {
        Certificate = SEI2Certificates.GetCertificate(SEI2Certificates.STS),
        SendTimeout = TimeSpan.FromDays(1.0),
        EndpointAddress = new Uri("https://securetokenservice.test-nemlog-in.dk/SecurityTokenService.svc"),
        TokenLifeTime = TimeSpan.FromDays(5.0),
      };

      var certificate = SEI2Certificates.GetCertificate(SEI2Certificates.XMedicus_Systems_ApS_IDWS_Test);
      var clientSettings = new OioIdwsClientSettings
      {
        ClientCertificate = certificate,
        SecurityTokenService = stsSettings,
        AudienceUri = new Uri("https://wsp.oioidws-net.dk"), //"https://wsc.test.xmedicus.com"),
        AccessTokenIssuerEndpoint = new Uri("https://seiidws.test.sundhedsdatastyrelsen.dk"),
      };

      var client = new OioIdwsClient(clientSettings);

      var token = client.GetSecurityToken();

      var abort = new SEI2SchemaAbortionContract();
      abort.schemaCreatedDate = DateTime.Now;
      abort.schemaUserGroupId = "GRP-0000117";
      abort.city = "København";
      abort.country = "5100";
      abort.firstName = "Nancy";
      abort.gender = SEI2Gender.Female;
      abort.municipalityCode = "147";
      abort.paymentCode = SEI2PaymentCode.PaidByCounty;
      abort.noReference = NoYes.No;
      abort.referenceDate = DateTime.Now;
      abort.sORCode = "223423423423";
      abort.schemaPersonCivilRegistrationIdentifier = "121212-1212";
      abort.streetName = "Test";
      abort.streetNum = "246";
      abort.surName = "Berggren";
      abort.zipCode = "2860";
      
      var res = SEI2Helper.Submit(abort, "report", token);

      Console.WriteLine(res);
      
      Assert.True(res != null);
    }
  }
}