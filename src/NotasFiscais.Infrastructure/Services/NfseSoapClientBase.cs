using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NotasFiscais.Infrastructure.Services
{
    public abstract class NfseSoapClientBase
    {
        private const string PastaCertificados = "C:/certificados";

        protected abstract string EndpointUrl { get; }
        protected abstract string Namespace { get; }     // namespace do serviço SOAP (Fintel, ABRASF, etc.)
        protected abstract string DataNamespace { get; } // namespace ABRASF dos XMLs de dados internos

        // Nomes dos parâmetros do método SOAP — cada prefeitura pode ter nomes diferentes
        protected virtual string ParamCabecalho => "cabecalho";
        protected virtual string ParamDados => "xml";

        protected X509Certificate2 CarregarCertificado(string cnpj, string senha)
        {
            var cnpjLimpo = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
            var caminhoPfx = Path.Combine(PastaCertificados, $"{cnpjLimpo}.pfx");

            if (!File.Exists(caminhoPfx))
                throw new FileNotFoundException($"Certificado não encontrado: {caminhoPfx}");

            return new X509Certificate2(
                caminhoPfx,
                senha,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable
            );
        }

        protected string AssinarXml(string xmlOriginal, X509Certificate2 certificado)
        {
            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.LoadXml(xmlOriginal);

            // Reimporta a chave RSA para evitar bloqueio do container Windows CryptoAPI
            var rsaOriginal = (RSACryptoServiceProvider)certificado.PrivateKey;
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaOriginal.ExportParameters(true));

            var signedXml = new SignedXml(doc) { SigningKey = rsa };

            // SHA1 — padrão exigido pelo ABRASF para prefeituras brasileiras
            signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

            var reference = new Reference { Uri = "" };
            reference.DigestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigC14NTransform());
            signedXml.AddReference(reference);

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificado));
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();

            // Signature fica APÓS o elemento raiz (irmã, não filha)
            // — o servidor Fintel/ABRASF espera os dois como fragmento dentro do CDATA
            var signatureXml = signedXml.GetXml().OuterXml;
            return xmlOriginal + "\n" + signatureXml;
        }

        protected string MontarSoapEnvelope(string soapAction, string cabecalho, string xmlCorpo)
        {
            // tns: é o prefixo local para o namespace do serviço — os parâmetros herdam o mesmo namespace
            var sb = new StringBuilder();
            sb.AppendLine("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:web=\"" + Namespace + "\">");
            sb.AppendLine("   <soapenv:Header/>");
            sb.AppendLine("   <soapenv:Body>");
            sb.AppendLine("      <web:" + soapAction + ">");
            sb.AppendLine("         <web:" + ParamCabecalho + "><![CDATA[");
            sb.AppendLine(cabecalho);
            sb.AppendLine("         ]]></web:" + ParamCabecalho + ">");
            sb.AppendLine("         <web:" + ParamDados + "><![CDATA[");
            sb.AppendLine(xmlCorpo);
            sb.AppendLine("         ]]></web:" + ParamDados + ">");
            sb.AppendLine("      </web:" + soapAction + ">");
            sb.AppendLine("   </soapenv:Body>");
            sb.Append("</soapenv:Envelope>");
            return sb.ToString();
        }

        protected async Task<string> EnviarSoapAsync(string soapEnvelope, string soapAction, X509Certificate2 certificado)
        {
            var handler = new WebRequestHandler();
            handler.ClientCertificates.Add(certificado);
            handler.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

            using (var client = new HttpClient(handler))
            {
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Add("SOAPAction", "\"" + Namespace + "/" + soapAction + "\"");

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                var response = await client.PostAsync(EndpointUrl, content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        protected string MontarCabecalho(string versao = "2.02")
        {
            return "<cabecalho versao=\"" + versao + "\" xmlns=\"" + DataNamespace + "\">\n" +
                   "   <versaoDados>" + versao + "</versaoDados>\n" +
                   "</cabecalho>";
        }

        /// <summary>
        /// Extrai o conteúdo de um elemento de resultado (ex: "ConsultarNfseServicoPrestadoResult")
        /// dentro do envelope SOAP de retorno. O conteúdo já vem decodificado (sem &lt;/&gt; escapados),
        /// pois é exatamente assim que o XmlDocument/XDocument expõe o texto de um elemento.
        /// </summary>
        protected static string ExtrairResultadoSoap(string soapResponseXml, string nomeElementoResultado)
        {
            var doc = XDocument.Parse(soapResponseXml);
            var elemento = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == nomeElementoResultado);
            return elemento?.Value;
        }

        /// <summary>
        /// Desserializa um XML para o tipo T, ignorando namespaces — necessário porque cada
        /// prefeitura/ABRASF declara namespaces de formas diferentes e isso quebraria o XmlSerializer
        /// se as classes de domínio precisassem mapear cada variação.
        /// </summary>
        protected static T DeserializarSemNamespace<T>(string xml) where T : class
        {
            if (string.IsNullOrWhiteSpace(xml))
                return null;

            var doc = XDocument.Parse(xml);
            RemoverNamespaces(doc.Root);

            var serializer = new XmlSerializer(typeof(T));
            using (var reader = doc.CreateReader())
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        private static void RemoverNamespaces(XElement elemento)
        {
            if (elemento == null)
                return;

            elemento.Name = elemento.Name.LocalName;

            var atributos = elemento.Attributes()
                .Where(a => !a.IsNamespaceDeclaration)
                .Select(a => new XAttribute(a.Name.LocalName, a.Value))
                .ToList();
            elemento.ReplaceAttributes(atributos);

            foreach (var filho in elemento.Elements())
                RemoverNamespaces(filho);
        }
    }
}
