using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;


namespace WpfApplication1
{

    public class GLPIUnicodeDecode
    {
        Dictionary<string, string> charmap = new Dictionary<string, string>();

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


    public class GLPI
    {
        public string _csrf_token;
        public string _cookie;
        public string _idor_token;
        public string _header_csrf_token;
        public string _ajax_condition;
        public string baseurl;
        public string login;
        public string password;

        // Constructor
        public GLPI(string baseurl, string login, string password)
        {
            this.baseurl = baseurl;
            this.login = login.Replace("@", "%40");
            this.password = password.Replace("@", "%40");

            // preventing except100 http header
            System.Net.ServicePointManager.Expect100Continue = false;
    
        }

        // Extract CSRF Token from request
        public string extractCsrfToken(string request)
        {
            return Regex.Matches(request, @"value=""([a-z0-9]{64})""")[0].Groups[1].Value;
        }

        // Extract HEADER (meta) CSRF Token
        public string extractHeaderCsrfToken(string request)
        {
            // System.Windows.MessageBox.Show(request);
            return Regex.Matches(request, @"property=""glpi:csrf_token"" content=""([a-z0-9]{64})""")[0].Groups[1].Value;
        }

        // Extract IDOR  CSRF Token
        public string extractIdorCsrfToken(string request)
        {
            return Regex.Matches(request, @"_idor_token: ""([a-z0-9]{64})""")[0].Groups[1].Value;
        }

        
        // Extract Ajax Condition Token
        public string extractAjaxCondition(string request)
        {
            return Regex.Matches(request, @"condition: ""([a-z0-9]{40})""")[0].Groups[1].Value;
        }
        
        


        // Extract LoginField from login form
        public string extractLoginName(string request)
        {
            return Regex.Matches(request, @"id=""userNameInput"" name=""([a-z0-9]{19})""")[0].Groups[1].Value;
        }


        // Extract PasswordField from login form
        public string extractPasswordName(string request)
        {
            return Regex.Matches(request, @"id=""passwordInput"" name=""([a-z0-9]{19})""")[0].Groups[1].Value;
        }



        

        /* Function doLogin
         * Realiza o login no GLPI para inicar a comunicação com o site.
         *  @nput: void
         *  @output: bool (login sucedido)
        */
        public bool doLogin()
        {


            // getting landing page
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(this.baseurl);
            WebResponse resp = req.GetResponse();
            this._cookie = resp.Headers.Get("Set-Cookie").Split(';')[0];
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string landing_page_content = sr.ReadToEnd();
            sr.Close();
            resp.Close();

            // extracting csrf token
            this._csrf_token = extractCsrfToken(landing_page_content);

            // Enviando o formulário de login
            req = (HttpWebRequest) WebRequest.Create(this.baseurl + "front/login.php");
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Headers.Add("Origin", this.baseurl);
            req.Headers.Add("Cookie", this._cookie);
            req.Referer = this.baseurl + "index.php?noAUTO=1";
            req.AllowAutoRedirect = false;

            // construindo body post
            StringBuilder body = new StringBuilder();
            body.Append(extractLoginName(landing_page_content) + "=" + this.login);
            body.Append("&noAUTO=1&" + extractPasswordName(landing_page_content) + "=" + this.password);
            body.Append("&AuthMethod=FormsAuthentication&_glpi_csrf_token=" + this._csrf_token);
            StreamWriter sw = new StreamWriter(req.GetRequestStream());
            sw.Write(body.ToString());
            sw.Flush();


            // validando resposta se possui header 302 Location redirect
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
         * Lista as categorias ITIL presentes no GLPI
         *  @nput: void
         *  @output: <Dictionary<int, string>
        */
        public Dictionary<int, string> getItilCategories()
        {

            
            //Acessando a página de novo chamado para pegar o csrf-token 
            
            try {
                // Creating the request
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(this.baseurl + "front/helpdesk.public.php?create_ticket=1");
                req.Method = "GET";
                req.Headers.Add("Cookie", this._cookie);
                req.Referer = this.baseurl + "front/helpdesk.public.php";
                WebResponse resp = req.GetResponse();
                string recv_data;

                // Dealing with http response
                using (var sr = new StreamReader(resp.GetResponseStream()))
                {
                    recv_data = sr.ReadToEnd();
                }
                this._csrf_token = extractCsrfToken(recv_data);
                this._header_csrf_token = extractHeaderCsrfToken(recv_data);
                this._ajax_condition = extractAjaxCondition(recv_data);
                this._idor_token = extractIdorCsrfToken(recv_data);
                
            } catch {
                System.Windows.MessageBox.Show("Erro ao solicitar pagina  de novo chamado");
            }
            
      
            // Requisitando as categorias via Ajax (simulação
            StringBuilder ajax_data = new StringBuilder();
            try{
                // Creating the request
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(this.baseurl + "ajax/getDropdownValue.php");
                req.Method = "POST";
                req.Accept = "application/json, text/javascript, */*; q=0.0";
                req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                req.Headers.Add("Cookie", this._cookie);
                req.Headers.Add("X-Glpi-Csrf-Token", this._header_csrf_token);
                req.Headers.Add("X-Requested-With", "XMLHttpRequest");
                req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.54 Safari/537.36";
                req.Headers.Add("Origin", this.baseurl);
                req.Referer = this.baseurl + "front/helpdesk.public.php?create_ticket=1";

                // post body
                StringBuilder body = new StringBuilder();
                body.Append("itemtype=ITILCategory");
                body.Append("&display_emptychoice=1");
                body.Append("&emptylabel=%20-----%20");
                body.Append("&entity_restrict=2");
                body.Append("&permit_select_parent=0");
                body.Append("&condition=" + this._ajax_condition);
                body.Append("&_idor_token=" + this._idor_token);
                body.Append("&page=1");
                body.Append("&page_limit=999");
                using (var sw = new StreamWriter(req.GetRequestStream()))
                {
                    sw.Write(body.ToString());
                    sw.Flush();
                }

                // Dealing with http response
                WebResponse resp = req.GetResponse();

                StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8, true);
                ajax_data.Append( sr.ReadToEnd() );
                sr.Close();

                GLPIUnicodeDecode decode = new GLPIUnicodeDecode();
                string parsed_data = decode.Decode(ajax_data.ToString());
                ajax_data.Clear();
                ajax_data.Append( parsed_data );
                


            }catch {
                System.Windows.MessageBox.Show("Erro ao solicitar ajax cat. itil");
                
            }
          
            Dictionary<int, string> ret = new Dictionary<int, string>();
            MatchCollection matches = Regex.Matches(ajax_data.ToString(), @"""id"":(\d+),""text"":""[\w\d\s_\-\\\>]+"",""level"":\d+,""title"":""[\w\d\s_\-\\\>]+"",""selection_text"":""([\w\d\s_\-\\\>]+)""");
            foreach (Match r in matches)
            {
                ret.Add(int.Parse(r.Groups[1].Value), r.Groups[2].Value);
            }
            return ret;
        }



    }
}
