using Newtonsoft.Json;
using OAuthDemo.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OAuthDemo.Controllers
{
    public class HomeController : Controller
    {
        private string ClientId = ConfigurationManager.AppSettings["Google.ClientID"];
        private string SecretKey = ConfigurationManager.AppSettings["Google.SecretKey"];
        private string RedirectUrl = ConfigurationManager.AppSettings["Google.RedirectUrl"];

        public async Task<ActionResult> Index()
        {
            string token = (string)Session["user"];
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.UserDetail = null;
            }
            else
            {
                ViewBag.UserDetail = await GetuserProfile(token);
            }
            return View();
        }

        public void LoginUsingGoogle()
        {
            Response.Redirect($"https://accounts.google.com/o/oauth2/v2/auth?client_id={ClientId}&response_type=code&scope=openid%20email%20profile&redirect_uri={RedirectUrl}&state=abcdef");
        }


        [HttpGet]
        public async Task<ActionResult> GoogleCallback(string code, string state, string session_state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return View("Error");
            }

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.googleapis.com")
            };
            var requestUrl = $"oauth2/v4/token?code={code}&client_id={ClientId}&client_secret={SecretKey}&redirect_uri={RedirectUrl}&grant_type=authorization_code";

            var dict = new Dictionary<string, string>
            {
                { "Content-Type", "application/x-www-form-urlencoded" }
            };
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl) { Content = new FormUrlEncodedContent(dict) };
            var response = await httpClient.SendAsync(request);
            var token = JsonConvert.DeserializeObject<GmailToken>(await response.Content.ReadAsStringAsync());
            Session["user"] = token.AccessToken;
            //var userProfile = await GetuserProfile(token.AccessToken);

            //IdToken property stores user data in Base64Encoded form
            //var data = Convert.FromBase64String(token.IdToken.Split('.')[1]);
            //var base64Decoded = System.Text.ASCIIEncoding.ASCII.GetString(data);

            //ViewBag.UserDetail = userProfile;

            return RedirectToAction("Index");

            //await Index();

            //return View("Index");
        }

        /// <summary>
        /// To fetch User Profile by access token
        /// </summary>
        /// <param name="accesstoken">access token</param>
        /// <returns>User Profile page</returns>
        public async Task<UserProfile> GetuserProfile(string accesstoken)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.googleapis.com")
            };
            string url = $"https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token={accesstoken}";
            var response = await httpClient.GetAsync(url);
            return JsonConvert.DeserializeObject<UserProfile>(await response.Content.ReadAsStringAsync());
        }

        public ActionResult SignOut()
        {
            Session["user"] = null;
            return RedirectToAction("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}