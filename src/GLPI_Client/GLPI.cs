using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;


namespace GLPI_Client
{

    public class GLPIUnicodeDecode
    {
        Dictionary<string, string> charmap = new Dictionary<string, string>();

        /* Function Constructor
         * Create the mapping between pt-br/latin characters
         * @input: void
         * @output: void
        */
        public GLPIUnicodeDecode()
        {
            charmap.Add(@"\u00c0", "À");
            charmap.Add(@"\u00c1", "Á");
            charmap.Add(@"\u00c2", "Â");
            charmap.Add(@"\u00c3", "Ã");
            charmap.Add(@"\u00c7", "Ç");
            charmap.Add(@"\u00c8", "È");
            charmap.Add(@"\u00c9", "É");
            charmap.Add(@"\u00ca", "Ê");
            charmap.Add(@"\u00cc", "Ì");
            charmap.Add(@"\u00cd", "Í");
            charmap.Add(@"\u00d2", "Ò");
            charmap.Add(@"\u00d3", "Ó");
            charmap.Add(@"\u00d4", "Ô");
            charmap.Add(@"\u00d5", "Õ");
            charmap.Add(@"\u00d9", "Ù");
            charmap.Add(@"\u00da", "Ú");            
            charmap.Add(@"\u00e0", "à");
            charmap.Add(@"\u00e1", "á");
            charmap.Add(@"\u00e2", "â");
            charmap.Add(@"\u00e3", "ã");
            charmap.Add(@"\u00e7","ç");
            charmap.Add(@"\u00e8", "è");
            charmap.Add(@"\u00e9", "é");
            charmap.Add(@"\u00ea", "ê");
            charmap.Add(@"\u00ec", "ì");
            charmap.Add(@"\u00ed", "í");
            charmap.Add(@"\u00f2", "ò");
            charmap.Add(@"\u00f3", "ó");
            charmap.Add(@"\u00f4", "ô");
            charmap.Add(@"\u00f5", "õ");
            charmap.Add(@"\u00f9", "ù");
            charmap.Add(@"\u00fa", "ú");
        }


        /* Function Decode
         * Normalize a unicode utf-8 encoded string
         * @input: string encoded text
         * @output: string decoded text
        */
        public string Decode(string input)
        {
            StringBuilder output = new StringBuilder(input);
            foreach (var c in charmap)
            {
                output.Replace(c.Key, c.Value);
            }
            return output.ToString();
        }
        
    }




    /*
     * Class GLPI
     * Abstract all communication with glpi website
    */
    public class GLPI
    {
        public string _csrf_token;
        public string _cookie;
        public string _idor_token;
        public string _header_csrf_token;
        public string _ajax_condition;
        public int _entity_id;
        public string baseurl;
        public string login;
        public string password;


        /* Function Construtor
         * Sets properties
         * @input: string baseurl
         * @input: string user login
         * @input: string user password
         * @output: void
        */
        public GLPI(string baseurl, string login, string password)
        {
            this.baseurl = baseurl;
            this.login = login.Replace("@", "%40");
            this.password = password.Replace("@", "%40");

            // preventing except100 http header
            System.Net.ServicePointManager.Expect100Continue = false;
        }


        /* Function extractCsrfToken
         * Extract CSRF Token from request
         * @input: string http body request
         * @output: string crsf token
        */
        public string extractCsrfToken(string request)
        {
            return Regex.Matches(request, @"value=""([a-z0-9]{64})""")[0].Groups[1].Value;
        }


        /* Function extractHeaderCsrfToken
         * Extract CSRF Token found in http header meta
         * @input: string http body request
         * @output: string crsf header token
        */
        public string extractHeaderCsrfToken(string request)
        {
            return Regex.Matches(request, @"property=""glpi:csrf_token"" content=""([a-z0-9]{64})""")[0].Groups[1].Value;
        }


        /* Function extractIdorCsrfToken
         * Extract IDOR  CSRF Token
         * @input: string http body request
         * @output: string crsf idor token
        */
        public string extractIdorCsrfToken(string request)
        {
            return Regex.Matches(request, @"_idor_token: ""([a-z0-9]{64})""")[0].Groups[1].Value;
        }


        /* Function extractAjaxCondition
         * Extract Ajax Condition Token
         * @input: string http body request
         * @output: string ajax conditions token
        */
        public string extractAjaxCondition(string request)
        {
            return Regex.Matches(request, @"condition: ""([a-z0-9]{40})""")[0].Groups[1].Value;
        }


        /* Function extractCurrentEntityId
         * Extract entity_id from a request
         * @input: string http body request
         * @output: Int32 current entity id
        */
        public int extractCurrentEntityId(string request)
        {
            return Int32.Parse(Regex.Matches(request, @"name='entities_id' value='(\d+)'")[0].Groups[1].Value);
        }


        /* Function extractLoginName
         * Extract LoginField from glpi login form
         * @input: string http body request
         * @output: string login input field name
        */
        public string extractLoginName(string request)
        {
            return Regex.Matches(request, @"id=""userNameInput"" name=""([a-z0-9]{19})""")[0].Groups[1].Value;
        }


        /* Function extractPasswordName
         * Extract PasswordField from login form
         * @input: string http body request
         * @output: string password input field name
        */
        public string extractPasswordName(string request)
        {
            return Regex.Matches(request, @"id=""passwordInput"" name=""([a-z0-9]{19})""")[0].Groups[1].Value;
        }


        /* Function doLogin
         * Authenticate on glpi website
         * @input: void
         * @output: bool succeeded
        */
        public bool doLogin()
        {
            // Getting landing page
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(this.baseurl);
            WebResponse resp = req.GetResponse();
            this._cookie = resp.Headers.Get("Set-Cookie").Split(';')[0];
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string landing_page_content = sr.ReadToEnd();
            sr.Close();
            resp.Close();

            // Extracting csrf token
            this._csrf_token = extractCsrfToken(landing_page_content);

            // Post the login form
            req = (HttpWebRequest) WebRequest.Create(this.baseurl + "front/login.php");
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Headers.Add("Origin", this.baseurl);
            req.Headers.Add("Cookie", this._cookie);
            req.Referer = this.baseurl + "index.php?noAUTO=1";
            req.AllowAutoRedirect = false;

            // Constructing POST body request
            StringBuilder body = new StringBuilder();
            body.Append(extractLoginName(landing_page_content) + "=" + this.login);
            body.Append("&noAUTO=1&" + extractPasswordName(landing_page_content) + "=" + this.password);
            body.Append("&AuthMethod=FormsAuthentication&_glpi_csrf_token=" + this._csrf_token);
            StreamWriter sw = new StreamWriter(req.GetRequestStream());
            sw.Write(body.ToString());
            sw.Flush();


            // Validating response. Success if we got an http/302 response (redirect)
            resp = req.GetResponse();
            if (String.IsNullOrEmpty(resp.Headers.Get("Location")))
            {
                return false;
            }
            else
            {
                this._cookie = resp.Headers.Get("Set-Cookie").Split(';')[0];
                this._csrf_token = extractCsrfToken(landing_page_content);
                return true;
            }

        }




        /* Function getItilCategories
         * Lista ITIL Categories on GLPI
         * @input: void
         * @output: <Dictionary<int, string>
        */
        public Dictionary<int, string> getItilCategories()
        {
            //Acessando a página de novo chamado para pegar o csrf-token 
            HttpWebRequest req;
            WebResponse resp;
            
            // Creating the request
            req = (HttpWebRequest)WebRequest.Create(this.baseurl + "front/helpdesk.public.php?create_ticket=1");
            
            // Setting request default http headers
            req.Method = "GET";
            req.Headers.Add("Cookie", this._cookie);
            req.Referer = this.baseurl + "front/helpdesk.public.php";

            try
            {
                // Getting response stream as string
                resp = req.GetResponse();
                string recv_data;
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                recv_data = sr.ReadToEnd();
                sr.Close();

                // Extracting tokens from response
                this._csrf_token = extractCsrfToken(recv_data);
                this._header_csrf_token = extractHeaderCsrfToken(recv_data);
                this._ajax_condition = extractAjaxCondition(recv_data);
                this._idor_token = extractIdorCsrfToken(recv_data);
                this._entity_id = extractCurrentEntityId(recv_data);

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Erro ao solicitar pagina  de novo chamado.\n" + e.Message);
            }

            //
            // Requesting ITIL Categories in a new http request
            //

            // Creating the request
            req = (HttpWebRequest) WebRequest.Create(this.baseurl + "ajax/getDropdownValue.php");

            // Setting request http headers
            req.Method = "POST";
            req.Accept = "application/json, text/javascript, */*; q=0.0";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.Headers.Add("Cookie", this._cookie);
            req.Headers.Add("X-Glpi-Csrf-Token", this._header_csrf_token);
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.54 Safari/537.36";
            req.Headers.Add("Origin", this.baseurl);
            req.Referer = this.baseurl + "front/helpdesk.public.php?create_ticket=1";

            // Constructing http post request body
            StringBuilder body = new StringBuilder();
            body.Append("itemtype=ITILCategory");
            body.Append("&display_emptychoice=1");
            body.Append("&emptylabel=%20-----%20");
            body.Append("&entity_restrict=" + this._entity_id);
            body.Append("&permit_select_parent=0");
            body.Append("&condition=" + this._ajax_condition);
            body.Append("&_idor_token=" + this._idor_token);
            body.Append("&page=1");
            body.Append("&page_limit=999");

            // ajax_receveided data
            StringBuilder ajax_data = new StringBuilder();
            try
            {
                // Sending the http request
                StreamWriter sw = new StreamWriter(req.GetRequestStream());
                sw.Write(body.ToString());
                sw.Flush();

                // Dealing with http response
                resp = req.GetResponse();

                StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8, true);
                ajax_data.Append( sr.ReadToEnd() );
                sr.Close();

                GLPIUnicodeDecode decode = new GLPIUnicodeDecode();
                string parsed_data = decode.Decode(ajax_data.ToString());
                ajax_data.Clear();
                ajax_data.Append( parsed_data );
                
            }catch (Exception e){
                System.Windows.MessageBox.Show("Erro ao solicitar categorias de ITIL.\n" + e.Message);
                
            }
          
            // Turning received data into a directionary
            Dictionary<int, string> ret = new Dictionary<int, string>();
            MatchCollection matches = Regex.Matches(ajax_data.ToString(), @"""id"":(\d+),""text"":""[\w\d\s_\-\\\>]+"",""level"":\d+,""title"":""[\w\d\s_\-\\\>]+"",""selection_text"":""([\w\d\s_\-\\\>]+)""");
            foreach (Match r in matches)
            {
                ret.Add(int.Parse(r.Groups[1].Value), r.Groups[2].Value);
            }
            return ret;
        }


        /* Function createNewTicket
         * Submit a New Ticket
         * @input: string ticket title
         * @input: string ticket details
         * @input: int itil category id
         * @output: 0-> error, Int32 ticket number on success
        */
        public int createNewTicket(string title, string details, int itilCategoriesId)
        {

            // Creating web request
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(this.baseurl + "front/tracking.injector.php");
            WebResponse resp;
            
            // Setting http headers
            req.Method = "POST";
            req.Accept = "application/json, text/javascript, */*; q=0.0";
            req.ContentType = "multipart/form-data; boundary=----WebKitFormBoundaryIS5CRGMBApVg1DBQ";
            req.Headers.Add("Cookie", this._cookie);
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.54 Safari/537.36";
            req.Headers.Add("Origin", this.baseurl);
            req.Referer = this.baseurl + "front/tracking.injector.php";

            // Constructing the http post body as multipart/form-data
            StringBuilder body = new StringBuilder();

            /*
            body.AppendLine("------WebKitFormBoundaryIS5CRGMBApVg1DBQ");
            body.AppendLine(@"Content-Disposition: form-data; name=""_from_helpdesk""");
            body.AppendLine();
            body.AppendLine("1");
            */

            body.AppendLine("------WebKitFormBoundaryIS5CRGMBApVg1DBQ");
            body.AppendLine(@"Content-Disposition: form-data; name=""requesttypes_id""");
            body.AppendLine();
            body.AppendLine("1");
            // 1->Helpdesk, 2->E-mail, 3->Phone, 4->Direct, 5->Written, 6->Other

            /*
            body.AppendLine("-------WebKitFormBoundaryIS5CRGMBApVg1DBQ");
            body.AppendLine(@"Content-Disposition: form-data; name=""date""");
            body.AppendLine();
            // DateTime datetime = new DateTime();
            // datetime.
            body.AppendLine("2021-11-06 01:19:16");
            */

            /*
            body.AppendLine("------WebKitFormBoundaryIS5CRGMBApVg1DBQ");
            body.AppendLine(@"Content-Disposition: form-data; name=""urgency""");
            body.AppendLine();
            body.AppendLine("3");
            */

            body.AppendLine("------WebKitFormBoundaryIS5CRGMBApVg1DBQ");
            body.AppendLine(@"Content-Disposition: form-data; name=""entities_id""");
            body.AppendLine();
            body.AppendLine(this._entity_id.ToString());

            body.AppendLine("------WebKitFormBoundaryIS5CRGMBApVg1DBQ");
            body.AppendLine(@"Content-Disposition: form-data; name=""type""");
            body.AppendLine();
            body.AppendLine("2");
            // 1-> incident, 2-> request

            body.AppendLine("------WebKitFormBoundaryIS5CRGMBApVg1DBQ");
            body.AppendLine(@"Content-Disposition: form-data; name=""itilcategories_id""");
            body.AppendLine();
            body.AppendLine(itilCategoriesId.ToString());

            body.AppendLine("------WebKitFormBoundaryIS5CRGMBApVg1DBQ");
            body.AppendLine(@"Content-Disposition: form-data; name=""_users_id_requester_notif[use_notification][]""");
            body.AppendLine();
            body.AppendLine("1");

            body.AppendLine("------WebKitFormBoundaryIS5CRGMBApVg1DBQ");
            body.AppendLine(@"Content-Disposition: form-data; name=""name""");
            body.AppendLine();
            body.AppendLine(title);

            body.AppendLine("------WebKitFormBoundaryIS5CRGMBApVg1DBQ");
            body.AppendLine(@"Content-Disposition: form-data; name=""content""");
            body.AppendLine();
            body.AppendLine(details);

            /*
            body.AppendLine("------WebKitFormBoundaryIS5CRGMBApVg1DBQ");
            body.AppendLine(@"Content-Disposition: form-data; name=""_tickettemplate""");
            body.AppendLine();
            body.AppendLine("1");
            */

            body.AppendLine("------WebKitFormBoundaryIS5CRGMBApVg1DBQ");
            body.AppendLine(@"Content-Disposition: form-data; name=""_predefined_fields""");
            body.AppendLine();
            body.AppendLine("eyJfYWxsX3ByZWRlZmluZWRfb3ZlcnJpZGUiOjF9");

            body.AppendLine("------WebKitFormBoundaryIS5CRGMBApVg1DBQ");
            body.AppendLine(@"Content-Disposition: form-data; name=""add""");
            body.AppendLine();
            body.AppendLine("Enviar mensagem");

            body.AppendLine("------WebKitFormBoundaryIS5CRGMBApVg1DBQ");
            body.AppendLine(@"Content-Disposition: form-data; name=""_glpi_csrf_token""");
            body.AppendLine();
            body.AppendLine(this._csrf_token);

            // End Of Mime content (boundary)
            body.AppendLine("------WebKitFormBoundaryIS5CRGMBApVg1DBQ--");

            
            // Sending the post request
            StringBuilder response = new StringBuilder();
            try
            {
                // request body writing to stream
                StreamWriter sw = new StreamWriter(req.GetRequestStream());
                sw.Write(body.ToString());
                sw.Flush();
                sw.Close();

                // Dealing with http response
                resp = req.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8, true);
                response.Append( sr.ReadToEnd() );
                sr.Close();

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            

            // Validate if ticket has been created 
            // on success matches regex (Chamado: <a href='/front/ticket.form.php?id=65'>65</a>
            Dictionary<int, string> ret = new Dictionary<int, string>();
            MatchCollection matches = Regex.Matches(response.ToString(), @"Seu chamado foi registrado \(Chamado: \<a href='\/front\/ticket\.form\.php\?id=(\d+)'\>\d+\<\/a\>");
            if (matches.Count < 1)
            {
                // not created
                return 0;
            }else {
                // successfully created
                return Int32.Parse( matches[0].Groups[1].Value );
            }
            
            
        }



    }
}
