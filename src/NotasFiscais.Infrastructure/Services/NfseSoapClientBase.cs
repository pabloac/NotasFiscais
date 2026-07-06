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

        // Prefixo do namespace usado no envelope SOAP (ex: "web" → xmlns:web="...", "nfse" → xmlns:nfse="...")
        protected virtual string NsPrefix => "web";

        // Quando false, os elementos dos parâmetros não levam o prefixo de namespace
        protected virtual bool ParamUsaNsPrefix => true;

        // Namespace do elemento <cabecalho> — algumas prefeituras usam um namespace diferente do DataNamespace
        protected virtual string CabecalhoNamespace => DataNamespace;

        // Quando false, o cabecalho é enviado como XML puro (sem CDATA) dentro do parâmetro
        protected virtual bool CabecalhoEmCdata => true;

        // Valor do header HTTP SOAPAction — override para "" quando o servidor não exige ou rejeita o valor completo
        protected virtual string MontarSoapActionHeader(string soapAction) => "\"" + Namespace + "/" + soapAction + "\"";

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

            var signatureXml = ComputarAssinatura(doc, certificado);

            // Signature fica APÓS o elemento raiz (irmã, não filha) — padrão Fintel/ABRASF
            return xmlOriginal + "\n" + signatureXml;
        }

        // Assina o elemento de nome informado inserindo a <Signature> como último filho dele.
        // Usado no GerarNfse onde a Signature fica dentro de <Rps>, não após o elemento raiz.
        protected string AssinarXmlDentroDoElemento(string xmlCompleto, string nomeElemento, X509Certificate2 certificado)
        {
            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.LoadXml(xmlCompleto);

            var elementoAlvo = (XmlElement)doc.GetElementsByTagName(nomeElemento)[0];

            // Extrai o elemento para doc temporário para que a assinatura cubra apenas ele
            var docTemp = new XmlDocument { PreserveWhitespace = true };
            docTemp.LoadXml(elementoAlvo.OuterXml);

            var signatureXml = ComputarAssinatura(docTemp, certificado);

            var signatureDoc = new XmlDocument();
            signatureDoc.LoadXml(signatureXml);
            elementoAlvo.AppendChild(doc.ImportNode(signatureDoc.DocumentElement, true));

            return doc.DocumentElement.OuterXml;
        }

        private string ComputarAssinatura(XmlDocument doc, X509Certificate2 certificado)
        {
            var rsaOriginal = (RSACryptoServiceProvider)certificado.PrivateKey;
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaOriginal.ExportParameters(true));

            var signedXml = new SignedXml(doc) { SigningKey = rsa };
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
            return signedXml.GetXml().OuterXml;
        }

        protected string MontarSoapEnvelope(string soapAction, string cabecalho, string xmlCorpo)
        {
            var p = NsPrefix;
            var pc = ParamUsaNsPrefix ? p + ":" : "";
            var sb = new StringBuilder();
            sb.AppendLine("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:" + p + "=\"" + Namespace + "\">");
            sb.AppendLine("   <soapenv:Header/>");
            sb.AppendLine("   <soapenv:Body>");
            sb.AppendLine("      <" + p + ":" + soapAction + ">");
            if (CabecalhoEmCdata)
            {
                sb.AppendLine("         <" + pc + ParamCabecalho + "><![CDATA[");
                sb.AppendLine(cabecalho);
                sb.AppendLine("         ]]></" + pc + ParamCabecalho + ">");
            }
            else
            {
                sb.AppendLine("         <" + pc + ParamCabecalho + ">");
                sb.AppendLine(cabecalho);
                sb.AppendLine("         </" + pc + ParamCabecalho + ">");
            }
            sb.AppendLine("         <" + pc + ParamDados + "><![CDATA[");
            sb.AppendLine(xmlCorpo);
            sb.AppendLine("         ]]></" + pc + ParamDados + ">");
            sb.AppendLine("      </" + p + ":" + soapAction + ">");
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
                client.DefaultRequestHeaders.Add("SOAPAction", MontarSoapActionHeader(soapAction));
                client.DefaultRequestHeaders.Add("User-Agent", "Apache-HttpClient/4.5.14 (Java/17)");
                client.DefaultRequestHeaders.Add("Accept", "text/xml, multipart/related");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                var response = await client.PostAsync(EndpointUrl, content);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException(
                        $"HTTP {(int)response.StatusCode} {response.ReasonPhrase} — Corpo: {responseBody}");

                return responseBody;
            }
        }

        protected string MontarCabecalho(string versao = "2.02")
        {
            return "<cabecalho versao=\"" + versao + "\" xmlns=\"" + CabecalhoNamespace + "\">\n" +
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

            // Override do elemento raiz pelo nome real do XML — permite reaproveitar a mesma classe
            // de domínio para operações ABRASF diferentes (ex: ConsultarNfseServicoPrestadoResposta
            // e ConsultarNfseServicoTomadoResposta), já que a estrutura interna é idêntica.
            var raizOverride = new XmlRootAttribute(doc.Root.Name.LocalName);
            var serializer = new XmlSerializer(typeof(T), raizOverride);
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
