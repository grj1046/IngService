using IngService.Models;
using IngService.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace IngService.Controllers
{
    public class AccountController : ApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> Signin()
        {
            var user = this.Request.Content.ReadAsAsync<IngAccount>().Result;
            string userName = user.UserName;
            string userPwd = user.UserPwd;
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userPwd))
            {
                var challenge = this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                challenge.Content = new StringContent("用户名或密码不能为空");
                return challenge;
            }

            string strContent = string.Format("tbUserName={0}&tbPassword={1}", HttpUtility.UrlEncode(userName), HttpUtility.UrlEncode(userPwd));

            HttpWebRequest request = HttpWebRequest.CreateHttp("http://m.cnblogs.com/mobileLoginPost.aspx");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.111 Safari/537.36";
            request.CookieContainer = new CookieContainer();
            var stream = await request.GetRequestStreamAsync();

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(strContent);
            request.ContentLength = bytes.LongLength;
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
            stream.Dispose();
            var response = await request.GetResponseAsync() as HttpWebResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string html = string.Empty;
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    html = sr.ReadToEnd();
                }
                if (string.IsNullOrEmpty(html) || html.IndexOf("登录失败！用户名或密码错误！") != -1)
                {
                    var challengeMessage = this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                    challengeMessage.Content = new StringContent("登录失败！用户名或密码错误！");
                    return challengeMessage;
                }
                var cookieCollection = request.CookieContainer.GetCookies(new Uri("http://wwww.cnblogs.com"));
                var httpResponseMessage = this.Request.CreateResponse(HttpStatusCode.OK);
                httpResponseMessage.Content = new StringContent("登录成功");
                List<CookieHeaderValue> cookies = new List<CookieHeaderValue>(cookieCollection.Count);
                foreach (Cookie c in cookieCollection)
                {
                    CookieHeaderValue cookie = new CookieHeaderValue(c.Name, c.Value);
                    //cookie.Domain = c.Domain;//Domain在请求数据的时候再添加
                    cookie.Expires = c.Expires;
                    cookie.Path = c.Path;
                    cookie.HttpOnly = c.HttpOnly;
                    cookies.Add(cookie);
                }
                httpResponseMessage.Headers.AddCookies(cookies);
                return httpResponseMessage;
            }
            else
            {
                string strResult = string.Empty;
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    strResult += sr.ReadToEnd();
                }
                var challengeMessage = this.Request.CreateResponse(response.StatusCode);
                challengeMessage.Content = new StringContent("登录失败！" + strResult);
                return challengeMessage;
            }
        }

        [HttpGet]
        public HttpResponseMessage Signout()
        {
            var responseMessage = this.Request.CreateResponse(HttpStatusCode.OK);
            responseMessage.Content = new StringContent("注销成功");
            List<CookieHeaderValue> cookies = new List<CookieHeaderValue>();
            foreach (string cookieName in HttpContext.Current.Request.Cookies)
            {
                HttpCookie c = HttpContext.Current.Request.Cookies[cookieName];
                CookieHeaderValue cookie = new CookieHeaderValue(c.Name, "");
                cookie.Domain = c.Domain;// ".cnblogs.com";
                cookie.Path = c.Path;
                cookie.Expires = DateTime.Now.AddDays(-1);
                cookie.HttpOnly = c.HttpOnly;
                cookies.Add(cookie);
            }
            responseMessage.Headers.AddCookies(cookies);
            return responseMessage;
        }
    }
}
