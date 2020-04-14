﻿using System;
using System.Security.Cryptography;
using Digst.OioIdws.OioWsTrust.Utils;
using Digst.OioIdws.Rest.Client;
using NUnit.Framework;
using OpenMedicus.WebService.SEI2;

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

      var clientSettings = new OioIdwsClientSettings
      {
        ClientCertificate = SEI2Certificates.GetCertificate(SEI2Certificates.Charlotte_Henriksen_RID_18756718),
        SecurityTokenService = stsSettings,
        AudienceUri = new Uri("https://saml.nnit001.dmz.inttest"),
        AccessTokenIssuerEndpoint = new Uri("https://seiidws.test.sundhedsdatastyrelsen.dk")
      };

      var client = new OioIdwsClient(clientSettings);

      var token = client.GetSecurityToken();

      Assert.True(!string.IsNullOrWhiteSpace(token.Id));
    }
  }
}