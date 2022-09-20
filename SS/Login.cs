using ProcessorsUtilities;
using ProcessorsUtilities.Model;
using ProcessorsUtilities.Utils;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace SS
{
    public class Login
    {
        private string baseURL;
        private string loginURL;
        private CertType type = CertType.NORMAL;
        private HttpUtilsRq utilsRq = new HttpUtilsRq();
        private CError ctlErr;

        #region Properties
        /// <summary>
        /// Contains all the information of the generated error. Adds information to the exception thrown by the class if it is a handled exception.
        /// </summary>
        public CError Error
        {
            get { return ctlErr; }
        
        }

        /// <summary>
        /// Gets the content of the HttpUtilsRq used to make the connector calls to the host
        /// </summary>
        public HttpUtilsRq UtilsRq
        {
            get { return utilsRq; }

        }

        /// <summary>
        /// Obtains the X509Certificate type certificate, corresponding to the IvSign credentials with which the connector is being used
        /// </summary>
        public X509Certificate2 CertFromIvSign
        {
            get { return utilsRq.GetCertFromIvSign(); }

        }
        #endregion Properties


        #region Init
        /// <summary>
        /// Set the basic connection parameters to be able to start the login process
        /// </summary>
        /// <param name="idService"></param>
        /// <param name="certType"></param>
        /// <param name="urlBase"></param>
        /// <param name="connectorURL">LoginUrl of the connector</param>
        /// <param name="ktoken"></param>
        /// <param name="certid"></param>
        /// <param name="pin"></param>
        /// <param name="entorno"></param>
        /// <param name="sessionId"></param>
        /// <param name="brokerUrl"></param>
        /// <param name="dllVersion"></param>
        public Login(string idService, CertType certType, string urlBase,string connectorURL, string ktoken, string certid, string pin, string entorno, string sessionId, string brokerUrl, double dllVersion)
        {
            baseURL = urlBase;
            loginURL = connectorURL;
            type = certType;
            utilsRq.SetParams(certType, ktoken, certid, pin, entorno);
            utilsRq.StartTracer(sessionId, brokerUrl, new Uri(baseURL).Host, dllVersion);
            utilsRq.SetService(idService);
            //if (certType == CertType.IVSIGN)
            //    Common.IvSignOK(entorno, ktoken, certid, pin);
        }
      
        /// <summary>
        /// Set the basic connection parameters to be able to start the login process
        /// </summary>
        /// <param name="utilsRequest"></param>
        /// <param name="certType"></param>
        /// <param name="urlBase"></param>
        public Login(HttpUtilsRq utilsRequest, CertType certType, string urlBase)
        {
            utilsRq = utilsRequest;
            type = certType;
            baseURL = urlBase;
        }

        #endregion Init

        #region Login
        /// <summary>
        /// Performs the login process on the platform website
        /// </summary>
        /// <param name="clientCertificate">Certificate with which to login the platform website</param>
        /// <param name="cookies"></param>
        /// <param name="referURL">In this connector the first referUrl is harcoded, so the possibility of passing it externally is maintained</param>
        /// <returns></returns>
        public string Process(X509Certificate2 clientCertificate, ref CookieContainer cookies, string referURL = "")
        {
            AIRequests aIRequests = utilsRq.GetAiRequest(clientCertificate, 5000000, baseURL, type);
            aIRequests = Process(aIRequests, referURL);
            cookies = aIRequests.CookieContainer;
           return aIRequests.HtmlContent;
        }

        /// <summary>
        /// Performs the login process on the platform website
        /// </summary>
        /// <param name="aIRequests"></param>
        /// <param name="referURL">In this connector the first referUrl is harcoded, so the possibility of passing it externally is maintained</param>
        /// <returns></returns>
        public AIRequests Process(AIRequests aIRequests, string referURL = "")
        {
            if (aIRequests == null)
            {
                ThrowCustomError(new CException(Code.EXCEPTION, "Petición no inicializada correctamente"), code: Code.EXCEPTION);

            }
            else
            {
                string requestURL = loginURL;
                //if (type == CertType.IVSIGN)
                //    utilsRq.SetCertType(CertType.IVSIGN);

              
                aIRequests.HttpRequest(ref requestURL, ref referURL, AccessType.GET);
                string content = aIRequests.HtmlContent.ToLower();

                //Old error handling. As it is not possible to determine in which element of the html each message is, the check is kept on all the content.
                if (content.Contains("se requiere un certificado de usuario aceptado por la Seguridad Social.".ToLower()))
                    ThrowCustomError(new CException(Code.NO_SEL_CERT_CORRECTLY, "Se requiere un certificado de usuario aceptado por la Seguridad Social."), code: Code.NO_SEL_CERT_CORRECTLY);

                else if (content.Contains("Acceso no autorizado".ToLower()) && content.Contains("en la lista de los certificados admitidos por la Seguridad Social".ToLower()))
                {
                    ThrowCustomError(new CException(Code.BAD_CERTIFICATE, "El certificado que usted está utilizando no esta incluido en la lista de los certificados admitidos por la Seguridad Social. Por favor, consulte la sección de Certificados Digitales de la Sede."), code: Code.BAD_CERTIFICATE);
                }
                else if (content.Contains("sica no figura en la base de datos"))
                    ThrowCustomError(new CException(Code.USER_NOT_REGISTERED, "Usuario no registrado"), code: Code.USER_NOT_REGISTERED);
                else if (content.Contains("sica figura con nombre diferente"))
                    ThrowCustomError(new CException(Code.USER_NOT_REGISTERED, "Usuario no registrado"), code: Code.USER_NOT_REGISTERED);
                else if (content.Contains("no se encuentra suscrito a"))
                    ThrowCustomError(new CException(Code.USER_NOT_SUSCRIBED, "Usuario no suscrito"), code: Code.USER_NOT_SUSCRIBED);
                else if (content.Contains("cerrada temporalmente"))
                    ThrowCustomError(new CException(Code.PLATFORM_UNAVAILABLE, "Plataforma cerrada temporalmente"), code: Code.PLATFORM_UNAVAILABLE);
                else if (content.Contains("no se encuentra disponible"))
                    ThrowCustomError(new CException(Code.PLATFORM_UNAVAILABLE, "Plataforma no disponible temporalmente"), code: Code.PLATFORM_UNAVAILABLE);
                else if (content.Contains("revoked"))
                    ThrowCustomError(new CException(Code.BAD_CERTIFICATE, "Certificado revocado"), code: Code.BAD_CERTIFICATE);
                else if (content.Contains("no se puede atender en este momento el servicio".ToLower()))
                    ThrowCustomError(new CException(Code.PLATFORM_UNAVAILABLE), code: Code.PLATFORM_UNAVAILABLE);
                else if (content.Contains("SVL00001".ToLower()))
                    ThrowCustomError(new CException(Code.USER_NOT_REGISTERED, "Usuario no registrado"), code: Code.USER_NOT_REGISTERED);

                //Llamada innecesaria; líneas con comentario de prueba en el conector. Se deja comentada
                //string postData = "&SPM.CONTEXT=internet&SPM.HAYJS=1&SPM.ISPOPUP=0&SPM.PORTALTYPE=HTML&tipoCodigoDestinatario=01&codigoDestinatario=06017078W&SPM.ACC.BUSCAR=Buscar";
                //HtmlNode ticket = aIRequests.HtmlDocument.GetNode(string.Empty, "//input[@id='ARQ.SPM.TICKET']", true);
                //if (ticket != null)
                //{
                //    postData = "ARQ.SPM.TICKET=" + ticket.GetAttr("value") + postData;
                //}
                //requestURL = "https://w2.seg-social.es/ProsaInternet/OnlineAccess";
                //aIRequests.HttpRequest(ref requestURL, ref referURL, AccessType.POST, postData);
            }
            return aIRequests;
        }
        #endregion Login

        #region Errors
        /// <summary>
        ///Generates a CError type error with all the manually generated exception information, and throws the exception itself.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="exMsg"></param>
        /// <param name="code"></param>
        private void ThrowCustomError(Exception ex, string exMsg = "", Code code = Code.PLATFORM_ERROR)
        {
            ctlErr = new CError(ex, (string.IsNullOrEmpty(exMsg) ? ex.Message : exMsg), code);
            throw ctlErr.Ex;
        }
        #endregion Errors
    }
}
