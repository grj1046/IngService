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
            long token = GetTimeToken();
            string query = string.Format("IngListType={0}&PageIndex={1}&PageSize={2}&Tag={3}&_={4}", strIngListType, pageIndex, pageSize, tag, token);
            builder.Query = query;
            return builder.Uri;
        }

        public static Uri BuildCommentUri(string ingId, string showCount = "15")
        {
            //http://home.cnblogs.com/ajax/ing/SingleIngComments?ingId=646099&showCount=15&_=1425988069834
            UriBuilder builder = new UriBuilder("http://home.cnblogs.com/ajax/ing/SingleIngComments");
            long token = GetTimeToken();
            string query = string.Format("ingId={0}&showCount={1}&_={2}", ingId, showCount, token);
            builder.Query = query;
            return builder.Uri;
        }

        /// <summary>
        /// 获取自1970年1月1日到现在的毫秒数
        /// </summary>
        /// <returns></returns>
        public static long GetTimeToken()
        {
            DateTime dt = new DateTime(1970, 01, 01, 0, 0, 0);
            return (DateTime.Now - dt).Ticks;
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
            return strHtml;
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

        public static List<Ing> GetIngs(string html, IngType type)
        {
            if (string.IsNullOrEmpty(html))
            {
                return new List<Ing>();
            }
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodeIngs = doc.DocumentNode.SelectNodes("/li");

            List<Ing> ings = new List<Ing>(nodeIngs.Count);

            foreach (HtmlNode node in nodeIngs)
            {
                try
                {
                    HtmlDocument nodeDoc = new HtmlDocument();
                    nodeDoc.LoadHtml(node.InnerHtml);
                    var childNode = nodeDoc.DocumentNode;

                    HtmlNode ingBodyNode = childNode.SelectSingleNode("//span[@class='ing_body']");
                    string strIngId = ingBodyNode.Attributes["id"].Value.Remove(0, "ing_body_".Length);
                    List<Segment> listIngBody = GetIngContent(ingBodyNode.InnerHtml);
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
                    //需要考虑有用户名，但是未修改头像，则从头像无法获取到UserId
                    string strUserId = GetUserIdByUri(strUserName, strAvatarUri);
                    string strUserNickName = childNode.SelectSingleNode("//a[@class='ing-author']").InnerText;

                    if (type == IngType.My)
                    {
                        //isPrivate 是否私有闪存
                        bool isPrivate = childNode.SelectSingleNode("//img[@title='私有闪存']") != null;
                        MyIng ing = new MyIng()
                        {
                            Id = strIngId,
                            Body = listIngBody,
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
                    else
                    {
                        Ing ing = new Ing()
                        {
                            Id = strIngId,
                            Body = listIngBody,
                            PublishTime = strIngTime,
                            IsFromePhone = blIsFromePhone,
                            IsNewbie = blIsNewbie,
                            IsLucky = blIsLucky,
                            UserAvatarUri = strAvatarUri,
                            UserId = strUserId,
                            UserName = strUserName,
                            UserNickName = strUserNickName
                        };
                        ings.Add(ing);
                    }
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
                    //string strReplyerId = GetUserIdByUri(strReplyerName, strReplyerAvatarUri);
                    string commentReplyText = childNode.SelectSingleNode("//a[@class='ing_reply ing-opt-space']").Attributes["onclick"].Value;
                    //commentReply(650460, 902008, 721787, 902008); return false;
                    string strTmp = commentReplyText.Remove(0, "commentReply(".Length);
                    strTmp = strTmp.Substring(0, strTmp.Length - "); return false;".Length);
                    string[] arr = strTmp.Split(',');
                    string strReplyerId = arr[2];//第三个为用户id

                    //IngId
                    HtmlNode replyNode = childNode.SelectSingleNode("//a[@class='comment-body-gray']");
                    string strIngId = replyNode.Attributes["href"].Value.Remove(0, "/ing/".Length);
                    strIngId = strIngId.Substring(0, strIngId.Length - 1);
                    //ReplyComment
                    string strReplyContent = HttpUtility.HtmlDecode(replyNode.InnerHtml);
                    //ReplyMsg
                    string strReplyMsg = HttpUtility.HtmlDecode(childNode.SelectSingleNode("//span[@class='ing_body']").InnerHtml);
                    //ReplyTime
                    string strReplyTime = childNode.SelectSingleNode("//a[@class='ing_time']").Attributes["title"].Value;

                    ReplyToMe replyToMe = new ReplyToMe()
                    {
                        ReplierId = strReplyerId,
                        ReplierName = strReplyerName,
                        ReplierNickName = strReplyerNickName,
                        ReplierAvatarUri = strReplyerAvatarUri,
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

        public static List<IngComment> GetSingleIngComments(string strHtml, string ingId)
        {
            if (string.IsNullOrEmpty(strHtml))
                return new List<IngComment>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(strHtml);

            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("/div/ul/li");

            List<IngComment> list = new List<IngComment>(nodes.Count);

            foreach (HtmlNode htmlNode in nodes)
            {
                HtmlDocument nodeDoc = new HtmlDocument();
                //nodeDoc.LoadHtml(htmlNode.InnerText);
                nodeDoc.LoadHtml(htmlNode.InnerHtml);
                HtmlNode childNode = nodeDoc.DocumentNode;

                //ingId
                //commentId
                string strCommentId = htmlNode.Attributes["id"].Value.Remove(0, "comment_".Length);
                //replierId
                string strReplierId = childNode.SelectSingleNode("//a[@href='#']").Attributes["onclick"].Value;
                strReplierId = strReplierId.Remove(0, strReplierId.LastIndexOf(',') + 1);
                strReplierId = strReplierId.Substring(0, strReplierId.IndexOf(')'));
                //replierName, replierNickName
                HtmlNode replierNode = childNode.SelectSingleNode("//a[@id='comment_author_" + strCommentId + "']");
                string strReplierName = replierNode.Attributes["href"].Value.Remove(0, "/u/".Length);
                strReplierName = strReplierName.Substring(0, strReplierName.Length - 1);
                string strReplierNickName = replierNode.InnerHtml;

                //看来获取评论只能使用while循环了
                //当遇到这些时  停止 class="recycle"(优先) 或者 class="ing_comment_time"
                //ReplyContent
                List<Segment> listReplyContent = new List<Segment>();
                //CanDelete
                bool blCanDelete = false;
                HtmlNode currNode = replierNode.NextSibling;
                HtmlAttribute currNodeClass = currNode.Attributes["class"];
                string strClassName = currNodeClass == null ? "" : currNodeClass.Value;
                while (strClassName != "recycle" && strClassName != "ing_comment_time")
                {
                    if (currNode.NodeType == HtmlNodeType.Text)
                    {
                        Segment segment = new Segment();
                        segment.Type = SegmentType.Text;
                        segment.Text = HttpUtility.HtmlDecode(currNode.InnerHtml.Trim());
                        if (!string.IsNullOrEmpty(segment.Text))
                            listReplyContent.Add(segment);
                    }
                    if (currNode.NodeType == HtmlNodeType.Element && currNode.Name == "a")
                    {
                        SegmentUrl segmentUrl = new SegmentUrl();
                        segmentUrl.Type = SegmentType.Link;
                        segmentUrl.Text = HttpUtility.HtmlDecode(currNode.InnerHtml.Trim());
                        segmentUrl.Url = currNode.Attributes["href"].Value;
                        listReplyContent.Add(segmentUrl);
                    }
                    currNode = currNode.NextSibling;
                    currNodeClass = currNode.Attributes["class"];
                    strClassName = currNodeClass == null ? "" : currNodeClass.Value;
                    blCanDelete = strClassName == "recycle";
                }
                //去除中文的冒号
                Segment first = listReplyContent.FirstOrDefault();
                if (first != null)
                {
                    first.Text = first.Text.Remove(0, 1);
                    if (string.IsNullOrEmpty(first.Text))
                        listReplyContent.Remove(first);
                }
                //replyTime
                string strReplyTime = HttpUtility.HtmlDecode(childNode.SelectSingleNode("//a[@class='ing_comment_time']").InnerHtml);

                IngComment comment = new IngComment()
                {
                    IngId = ingId,
                    CommentId = strCommentId,
                    ReplierId = strReplierId,
                    ReplierName = strReplierName,
                    ReplierNickName = strReplierNickName,
                    ReplyContent = listReplyContent,
                    ReplyTime = strReplyTime,
                    CanDelete = blCanDelete
                };
                list.Add(comment);
            }
            return list;
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
                    //.png?id= or .jpg?id=
                    var extIndex = userId.IndexOf("?id=");
                    if (extIndex > -1)
                    {
                        userId = userId.Substring(1, extIndex - 5);
                    }
                    else if (userId.StartsWith("u") && userId.EndsWith(".png"))
                    {
                        userId = userId.Remove(0, 1);
                        userId = userId.Substring(0, userId.Length - ".png".Length);
                    }
                }
            }
            return userId;
        }

        /// <summary>
        /// 获取闪存内容
        /// </summary>
        /// <returns></returns>
        public static List<Segment> GetIngContent(string strHtml)
        {
            List<Segment> list = new List<Segment>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(strHtml);
            HtmlNode currNode = doc.DocumentNode.FirstChild;
            HtmlAttribute currNodeClass = currNode.Attributes["class"];
            while (currNode != null)
            {
                if (currNode.NodeType == HtmlNodeType.Text)
                {
                    Segment segment = new Segment();
                    segment.Type = SegmentType.Text;
                    segment.Text = HttpUtility.HtmlDecode(currNode.InnerHtml.Trim());
                    if (!string.IsNullOrEmpty(segment.Text))
                        list.Add(segment);
                }
                if (currNode.NodeType == HtmlNodeType.Element && currNode.Name == "a")
                {
                    string strHref = currNode.Attributes["href"].Value;
                    string strText = HttpUtility.HtmlDecode(currNode.InnerHtml.Trim());
                    if (strHref.StartsWith("/ing/tag/"))
                    {
                        //标签
                        Segment segment = new Segment();
                        segment.Type = SegmentType.Tag;
                        string strTmp = strText.Remove(0, 1);// [
                        strTmp = strTmp.Remove(strTmp.Length - 1);// ]
                        segment.Text = strTmp;
                        list.Add(segment);
                    }
                    else if (strHref.StartsWith("/u/"))
                    {
                        //被at的用户
                        SegmentUser segmentUser = new SegmentUser();
                        segmentUser.Type = SegmentType.User;
                        string strTmp = strHref.Remove(0, "/u/".Length);// /u/
                        strTmp = strTmp.Remove(strTmp.Length - 1);// /
                        segmentUser.UserId = strTmp;
                        segmentUser.Text = strText;
                        list.Add(segmentUser);
                    }
                    else
                    {
                        //链接
                        SegmentUrl segmentUrl = new SegmentUrl();
                        segmentUrl.Type = SegmentType.Link;
                        segmentUrl.Text = strText;
                        segmentUrl.Url = HttpUtility.HtmlDecode(strHref);
                        list.Add(segmentUrl);
                    }
                }
                currNode = currNode.NextSibling;
            }
            return list;
        }
    }
}
