using HtmlAgilityPack;
using IngService.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace IngService.Services
{
    public static class IngServices
    {
        public static Uri BuildUri(IngType ingType, string tag, int pageIndex = 1, int pageSize = 30)
        {
            //http://home.cnblogs.com/ajax/ing/GetPagedIngList?
            //IngListType=all&
            //PageIndex=1&
            //PageSize=30&
            //Tag=
            //&_=1420551939473
            //(new Date).getTime() => 1420552726102 返回 1970 年 1 月 1 日至今的毫秒数。
            UriBuilder builder = new UriBuilder("http://home.cnblogs.com/ajax/ing/GetPagedIngList");
            string strIngListType = Enum.GetName(typeof(IngType), ingType);
            DateTime dt = new DateTime(1970, 01, 01, 0, 0, 0);
            long token = (DateTime.Now - dt).Ticks;
            string query = string.Format("IngListType={0}&PageIndex={1}&PageSize={2}&Tag={3}&_={4}", strIngListType, pageIndex, pageSize, tag, token);
            builder.Query = query;
            return builder.Uri;
        }

        public async static Task<string> GetResponseMessage(Uri uri)
        {
            string strHtml = string.Empty;
            HttpWebRequest request = HttpWebRequest.CreateHttp(uri);
            request.Method = "GET";
            request.ContentType = HttpContext.Current.Request.ContentType;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.111 Safari/537.36";
            request.CookieContainer = new CookieContainer();
            foreach (string cookieName in HttpContext.Current.Request.Cookies)
            {
                HttpCookie c = HttpContext.Current.Request.Cookies[cookieName];
                Cookie cookie = new Cookie(c.Name, c.Value);
                cookie.Domain = ".cnblogs.com";
                cookie.Path = c.Path;
                cookie.Expires = c.Expires;
                cookie.HttpOnly = c.HttpOnly;
                request.CookieContainer.Add(cookie);
            }
            var response = await request.GetResponseAsync() as HttpWebResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream stream = response.GetResponseStream();
                using (StreamReader sr = new StreamReader(stream))
                {
                    strHtml = sr.ReadToEnd();
                }
            }
            return HttpUtility.HtmlDecode(strHtml);
        }

        public static void CheckLogin(string html)
        {
            string str = "当前处于未登录状态，请先<a href='javascript:void(0)' onclick='login();'>登录</a>。";
            if (str == html)
            {
                var challengeMessage = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                challengeMessage.Content = new StringContent("当前处于未登录状态，请先登录。");
                throw new System.Web.Http.HttpResponseException(challengeMessage);
            }
        }

        public static List<Ing> GetIngs(string html)
        {
            List<Ing> ings = new List<Ing>();
            List<MyIng> myIngs = GetMyIngs(html);
            foreach (var myIng in myIngs)
            {
                Ing ing = new Ing()
                {
                    Id = myIng.Id,
                    Body = myIng.Body,
                    PublishTime = myIng.PublishTime,
                    IsFromePhone = myIng.IsFromePhone,
                    IsNewbie = myIng.IsNewbie,
                    IsLucky = myIng.IsLucky,
                    UserAvatarUri = myIng.UserAvatarUri,
                    UserId = myIng.UserId,
                    UserName = myIng.UserName,
                    UserNickName = myIng.UserNickName
                };
                ings.Add(ing);
            }
            return ings;
        }

        public static List<MyIng> GetMyIngs(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return new List<MyIng>();
            }
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodeIngs = doc.DocumentNode.SelectNodes("/li");
            List<MyIng> ings = new List<MyIng>(nodeIngs.Count);

            foreach (HtmlNode node in nodeIngs)
            {
                try
                {
                    HtmlDocument nodeDoc = new HtmlDocument();
                    nodeDoc.LoadHtml(node.InnerHtml);
                    var childNode = nodeDoc.DocumentNode;

                    HtmlNode ingBodyNode = childNode.SelectSingleNode("//span[@class='ing_body']");
                    string strIngId = ingBodyNode.Attributes["id"].Value.Remove(0, "ing_body_".Length);
                    string strIngBody = ingBodyNode.InnerHtml;
                    string strIngTime = childNode.SelectSingleNode("//a[@class='ing_time']").Attributes["title"].Value;
                    strIngTime = strIngTime.Remove(0, "发布于".Length);
                    strIngTime = strIngTime.Remove(strIngTime.Length - "，点击进入详细页面".Length);
                    //img_middle
                    bool blIsFromePhone = childNode.SelectSingleNode("//img[@class='img_middle']") != null;
                    bool blIsNewbie = childNode.SelectSingleNode("//img[@class='ing_icon_newbie']") != null;
                    bool blIsLucky = childNode.SelectSingleNode("//img[@class='ing_icon_lucky']") != null;
                    string strAvatarUri = childNode.SelectSingleNode("//div[@class='feed_avatar']/a/img").Attributes["src"].Value;

                    string strUserName = childNode.SelectSingleNode("//a[@class='ing-author']").Attributes["href"].Value;
                    strUserName = strUserName.Remove(0, "/u/".Length);
                    strUserName = strUserName.Remove(strUserName.Length - 1);
                    string strUserId = GetUserIdByUri(strUserName, strAvatarUri);
                    string strUserNickName = childNode.SelectSingleNode("//a[@class='ing-author']").InnerText;
                    //isPrivate 是否私有闪存
                    bool isPrivate = childNode.SelectSingleNode("//img[@title='私有闪存']") != null;

                    MyIng ing = new MyIng()
                    {
                        Id = strIngId,
                        Body = strIngBody,
                        PublishTime = strIngTime,
                        IsFromePhone = blIsFromePhone,
                        IsNewbie = blIsNewbie,
                        IsPrivate = isPrivate,
                        IsLucky = blIsLucky,
                        UserAvatarUri = strAvatarUri,
                        UserId = strUserId,
                        UserName = strUserName,
                        UserNickName = strUserNickName
                    };
                    ings.Add(ing);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            return ings;
        }

        public static List<ReplyToMe> GetReplyToMe(string strHtml)
        {
            //commentReply(645283,892469,577767, 892469)
            //commentReply(IngId, //span[@class='ing_cm_box'].attribute['id'].Value.remove("panel_".length) *currentParentCommentId*, 回复者的id)
            if (string.IsNullOrEmpty(strHtml))
                return new List<ReplyToMe>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(strHtml);

            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("/div[@id='feed_list']/ul/li");
            List<ReplyToMe> list = new List<ReplyToMe>(nodes.Count);
            foreach (HtmlNode htmlNode in nodes)
            {
                try
                {
                    HtmlDocument nodeDoc = new HtmlDocument();
                    nodeDoc.LoadHtml(htmlNode.InnerHtml);
                    HtmlNode childNode = nodeDoc.DocumentNode;

                    HtmlNode replyerNode = childNode.SelectSingleNode("//a[@class='big_font blue']");
                    //ReplyerName
                    string strReplyerName = replyerNode.Attributes["href"].Value.Remove(0, "/u/".Length);
                    strReplyerName = strReplyerName.Substring(0, strReplyerName.Length - 1);
                    //ReplyerNickName
                    string strReplyerNickName = replyerNode.InnerHtml;
                    //ReplyContentId
                    string strReplyContentId = replyerNode.Attributes["id"].Value.Remove(0, "comment_author_".Length);
                    //ReplyerAvatarUri
                    string strReplyerAvatarUri = childNode.SelectSingleNode("//div[@class='feed_avatar']/a/img").Attributes["src"].Value;
                    //ReplyerId
                    string strReplyerId = GetUserIdByUri(strReplyerName, strReplyerAvatarUri);
                    //IngId
                    HtmlNode replyNode = childNode.SelectSingleNode("//a[@class='comment-body-gray']");
                    string strIngId = replyNode.Attributes["href"].Value.Remove(0, "/ing/".Length);
                    strIngId = strIngId.Substring(0, strIngId.Length - 1);
                    //ReplyComment
                    string strReplyContent = replyNode.InnerHtml;
                    //ReplyMsg
                    string strReplyMsg = childNode.SelectSingleNode("//span[@class='ing_body']").InnerHtml;
                    //ReplyTime
                    string strReplyTime = childNode.SelectSingleNode("//a[@class='ing_time']").Attributes["title"].Value;

                    ReplyToMe replyToMe = new ReplyToMe()
                    {
                        ReplyerId = strReplyerId,
                        ReplyerName = strReplyerName,
                        ReplyerNickName = strReplyerNickName,
                        ReplyerAvatarUri = strReplyerAvatarUri,
                        IngId = strIngId,
                        ReplyContentId = strReplyContentId,
                        ReplyContent = strReplyContent,
                        ReplyMsg = strReplyMsg,
                        ReplyTime = strReplyTime
                    };
                    list.Add(replyToMe);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }

            return list;
        }
        /// <summary>
        /// 根据图像uri来获取用户id
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private static string GetUserIdByUri(string userName, string url)
        {
            //一共要处理三种url来获取userId
            //http://pic.cnitblog.com/face/sample_face.gif id: the same as userName
            //http://pic.cnitblog.com/face/577767/20150123103550.png id:577767
            //http://pic.cnitblog.com/face/u1.png?id=202208179510 id: 1
            string userId = string.Empty;
            if (url.IndexOf("sample_face.gif") != -1)
            {
                userId = userName;
            }
            else
            {
                userId = url.Remove(0, "http://pic.cnitblog.com/face/".Length);
                int uIndex = userId.IndexOf('u');
                if (uIndex == -1)
                {
                    userId = userId.Substring(0, userId.IndexOf('/'));
                }
                else
                {
                    userId = userId.Substring(1, userId.IndexOf(".png") - 1);
                }
            }
            return userId;
        }

        /// <summary>
        /// 回复闪存
        /// </summary>
        /// <returns></returns>
        public static async Task<string> PostComment(CommentModel model)
        {
            //http://home.cnblogs.com/ing/645536/

            //raw
            //POST http://home.cnblogs.com/ajax/ing/PostComment HTTP/1.1
            //Host: home.cnblogs.com
            //Connection: keep-alive
            //Content-Length: 106
            //Accept: application/json, text/javascript, */*; q=0.01
            //Origin: http://home.cnblogs.com
            //X-Requested-With: XMLHttpRequest
            //User-Agent: Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.111 Safari/537.36
            //Content-Type: application/json; charset=UTF-8
            //Referer: http://home.cnblogs.com/ing/
            //Accept-Encoding: gzip, deflate
            //Accept-Language: zh-CN,zh;q=0.8
            //Cookie: __gads=ID=863c16356a733dc7:T=1425135364:S=ALNI_MZyHBcV9SSL3YV-RKDHLRMM7ah5RQ; .DottextCookie=A11F45B3E3169574D36E6D6470E55F21A3EFC7BE179B798E7E6C588974E6FF4D4B62A87775FCFDFFE58BFBA3A551F5C3CD316E62ADF950E30041F6D8D5604D10A56898A3541841A3CF211730BAEE119A6B55FF79ACEC576506C6C8E26C855F0C24005D4E9426C166F0B4A584; SERVERID=73ea7682c79ff5c414f1e6047449c5c1|1425817968|1425815813

            //{"ContentId":645536,"ReplyTo":463726,"ParentCommentId":892566,"Content":"@枕头妹：沾了星星的光"}

            var strContent = Newtonsoft.Json.JsonConvert.SerializeObject(model);

            HttpWebRequest request = HttpWebRequest.CreateHttp("http://home.cnblogs.com/ajax/ing/PostComment");
            request.Method = "POST";
            request.ContentType = "application/json; charset=UTF-8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.111 Safari/537.36";
            request.CookieContainer = new CookieContainer();
            foreach (string cookieName in HttpContext.Current.Request.Cookies)
            {
                HttpCookie c = HttpContext.Current.Request.Cookies[cookieName];
                Cookie cookie = new Cookie(c.Name, c.Value);
                cookie.Domain = ".cnblogs.com";
                cookie.Path = c.Path;
                cookie.Expires = c.Expires;
                cookie.HttpOnly = c.HttpOnly;
                request.CookieContainer.Add(cookie);
            }
            var stream = await request.GetRequestStreamAsync();

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(strContent);
            request.ContentLength = bytes.LongLength;
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
            stream.Dispose();
            var response = await request.GetResponseAsync() as HttpWebResponse;
            string html = string.Empty;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    html = sr.ReadToEnd();
                }
            }
            return HttpUtility.HtmlDecode(html);
        }
    }
}
