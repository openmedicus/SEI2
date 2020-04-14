using System;
using System.IO;
using System.IdentityModel.Tokens;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Dynamics.AX.Application;

namespace OpenMedicus.WebService.SEI2
 {
    public class SEI2Helper
    {
        public static string Submit(byte[] fileBytes, string fileName, SecurityToken employeeToken)
        {
            string response = string.Empty;
            Func<object, string, string> formatOutPut = (responseObject, message) =>
            {
                StringBuilder responseText = new StringBuilder(message);
                responseText.Append(Environment.NewLine);
                responseText.Append(Object2String(responseObject));
                return responseText.ToString();
            };
            try
            {
                Func<ISEI2Report, object, Func< object, string, string>, string > invokeService = null;
                string tmpName = Path.GetFileNameWithoutExtension(fileName).ToLower();
                if (tmpName.Contains("report"))
                {
                    invokeService = (ISEI2Report client, object _contract, Func<object, string, string> output) =>
                    {
                        var responseObject = client.SchemaReport(_contract as SEI2SchemaBaseMembersContract);
                        return output(responseObject, "Operation: SchemaReport");
                    };
                }
                else if (tmpName.Contains("cancel"))
                {
                    invokeService = (ISEI2Report client, object _contract, Func<object, string, string> output) =>
                    {
                        var responseObject = client.SchemaCancel(_contract as SEI2SchemaCancelContract);
                        return output(responseObject, "Operation: SchemaCancel");
                    };
                }
                else if (tmpName.Contains("prefillmrsa"))
                {
                    invokeService = (ISEI2Report client, object _contract, Func<object, string, string> output) =>
                    {
                        var responseObject = client.SchemaPrefillMRSA(_contract as MRSAPrefillContract);
                        return output(responseObject, "Operation: SchemaPrefillMRSA");
                    };
                }
                else if (tmpName.Contains("getstatusmrsa"))
                {
                    invokeService = (ISEI2Report client, object _contract, Func<object, string, string> output) =>
                    {
                        var responseObject = client.SchemaGetStatusMRSA((Guid)_contract);
                        return output(responseObject, "Operation: SchemaGetStatusMRSA");
                    };
                }
                else if (tmpName.Contains("usergetdetails"))
                {
                    invokeService = (ISEI2Report client, object _contract, Func<object, string, string> output) =>
                    {
                        var responseObject = client.UserGetDetails((SEI2UserDetailsRequestContract)_contract);
                        return output(responseObject, "Operation: UserGetDetails");
                    };
                }
                else if (tmpName.Contains("usercreate"))
                {
                    invokeService = (ISEI2Report client, object _contract, Func<object, string, string> output) =>
                    {
                        var responseObject = client.UserCreate(_contract as SEI2UserCreateRequestContract);
                        return output(responseObject, "Operation: UserCreate");
                    };
                }

                var contract = Deserialize(fileBytes);
                if (contract != null)
                {
                    if (invokeService == null)
                        throw new ApplicationException("Could not generate delegate to invoke service.");
                    /*IStsTokenService stsTokenService = new StsTokenServiceCache(TokenServiceConfigurationFactory.CreateConfiguration());

                    SecurityToken securityToken = employeeToken != null
                        ? stsTokenService.GetTokenWithBootstrapToken(employeeToken)
                        : stsTokenService.GetToken();

                    using (var client = new SEI2ReportClient("SoapBinding_ISEI2Report"))
                    {
                        var channelWithIssuedToken = client.ChannelFactory.CreateChannelWithIssuedToken(securityToken);
                        response = invokeService.Invoke(channelWithIssuedToken, contract, formatOutPut);
                    }
                    */
                }
            }
            catch (Exception ex)
            {
                response = string.Format("Unexpected error: {0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
            return response;
        }

        private static object Deserialize(byte[] fileBytes)
        {
            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;
            using (var stream = new MemoryStream(fileBytes))
            {
                document.Load(stream);
            }

            XmlNode contractNode = document.DocumentElement;

            switch (contractNode.LocalName)
            {
                case "SEI2SchemaBedPlacesContract":
                    return DeserializeBody<SEI2SchemaBedPlacesContract>(contractNode);

                case "SEI2SchemaChildrensDatabaseContract":
                    return DeserializeBody<SEI2SchemaChildrensDatabaseContract>(contractNode);

                case "SEI2SchemaCoercion1Contract":
                    return DeserializeBody<SEI2SchemaCoercion1Contract>(contractNode);

                case "SEI2SchemaCoercion2Contract":
                    return DeserializeBody<SEI2SchemaCoercion2Contract>(contractNode);

                case "SEI2SchemaCoercion3Contract":
                    return DeserializeBody<SEI2SchemaCoercion3Contract>(contractNode);

                case "SEI2SchemaCoercion4Contract":
                    return DeserializeBody<SEI2SchemaCoercion4Contract>(contractNode);

                case "SEI2SchemaCoercion5Contract":
                    return DeserializeBody<SEI2SchemaCoercion5Contract>(contractNode);

                case "SEI2SchemaAbortionContract":
                    return DeserializeBody<SEI2SchemaAbortionContract>(contractNode);

                case "SEI2SchemaRitualCircumcisionContract":
                    return DeserializeBody<SEI2SchemaRitualCircumcisionContract>(contractNode);

                case "SEI2SchemaInjHeroinContract":
                    return DeserializeBody<SEI2SchemaInjHeroinContract>(contractNode);

                case "SEI2SchemaReleaseFollowupContract":
                    return DeserializeBody<SEI2SchemaReleaseFollowupContract>(contractNode);

                case "SEI2SchemaMortality1Contract":
                    return DeserializeBody<SEI2SchemaMortality1Contract>(contractNode);

                case "SEI2SchemaMortality2Contract":
                    return DeserializeBody<SEI2SchemaMortality2Contract>(contractNode);

                case "SEI2SchemaCancerContract":
                    return DeserializeBody<SEI2SchemaCancerContract>(contractNode);

                case "SEI2SchemaIVFContract":
                    return DeserializeBody<SEI2SchemaIVFContract>(contractNode);

                case "SEI2SchemaMRSAContract":
                    return DeserializeBody<SEI2SchemaMRSAContract>(contractNode);

                case "SEI2SchemaRehabilitationContract":
                    return DeserializeBody<SEI2SchemaRehabilitationContract>(contractNode);

                case "SEI2SchemaWaitInfoContract":
                    return DeserializeBody<SEI2SchemaWaitInfoContract>(contractNode);

                case "SEI2SchemaCoercionIncapacitatedSomatikContract":
                    return DeserializeBody<SEI2SchemaCoercionIncapacitatedSomatikContract>(contractNode);

                case "SEI2SchemaSSIContract":
                    return DeserializeBody<SEI2SchemaSSIContract>(contractNode);

                case "SEI2SchemaNABContract":
                    return DeserializeBody<SEI2SchemaNABContract>(contractNode);

                //GetStatusMRSA
                case "Value":
                    return Guid.Parse(contractNode.InnerText);
                //PrefillMRSA
                case "MRSAPrefillContract":
                    return DeserializeBody<MRSAPrefillContract>(contractNode);
                //Cancel
                case "SEI2SchemaCancelContract":
                    return DeserializeBody<SEI2SchemaCancelContract>(contractNode);
                //UserGetDetails
                case "SEI2UserDetailsRequestContract":
                    return DeserializeBody<SEI2UserDetailsRequestContract>(contractNode);
                //UserCreate
                case "SEI2UserCreateRequestContract":
                    return DeserializeBody<SEI2UserCreateRequestContract>(contractNode);
                default:
                    throw new ApplicationException(contractNode.LocalName + " is not supported by this client.");
            }
        }

        private static T DeserializeBody<T>(XmlNode bodyNode) where T : class
        {
            T contract;
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            using (XmlNodeReader reader = new System.Xml.XmlNodeReader(bodyNode))
            {
                contract = serializer.ReadObject(reader) as T;
            }
            if (contract == null)
                Console.WriteLine("Could not deserialize as " + typeof(T).ToString());
            return contract;
        }

        private static T String2Object<T>(string value) where T : class
        {
            if (value == null)
                return default(T);
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream, System.Text.Encoding.UTF8))
                {
                    writer.Write(value);
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                    return serializer.ReadObject(stream) as T;
                }
            }
        }

        private static string Object2String(object value)
        {
            if (value == null)
                return string.Empty;
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = true,
                CloseOutput = false
            };
            DataContractSerializer serializer = new DataContractSerializer(value.GetType());
            StringBuilder sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                serializer.WriteObject(writer, value);
            }
            return sb.ToString();
        }
    }
}
