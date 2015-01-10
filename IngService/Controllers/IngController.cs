using IngService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using HtmlAgilityPack;

namespace IngService.Controllers
{
    /// <summary>
    /// 闪存 http://home.cnblogs.com/ing/
    /// </summary>
    public class IngController : ApiController
    {
        private HttpClient _client;
        public IngController()
        {
            _client = new HttpClient();
        }

        /// <summary>
        /// 全站
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<Ing>> All(int pageIndex = 1, int pageSize = 30)
        {
            //http://home.cnblogs.com/ajax/ing/GetPagedIngList?IngListType=all&PageIndex=1&PageSize=30&Tag=&_=1420551939473
            //(new Date).getTime() => 1420552726102 返回 1970 年 1 月 1 日至今的毫秒数。
            int token = DateTime.Now.GetHashCode();
            List<Ing> ings = new List<Ing>();
            string strUrl = string.Format("http://home.cnblogs.com/ajax/ing/GetPagedIngList?IngListType=all&PageIndex={0}&PageSize={1}&Tag=&_={2}", pageIndex, pageSize, token);
            Uri uri = new Uri(strUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await _client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string strIngHtml = await response.Content.ReadAsStringAsync();
                ings = GetIngs(strIngHtml);
            }

            return ings;
        }
        /// <summary>
        /// 回复我
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Ing> ReplyToMe()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 提到我
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Ing> MentionedMe()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 新回应
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Ing> Reply()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 我回应
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Ing> MyReply()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 我的
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Ing> My()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 关注
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Ing> Recent()
        {
            throw new NotImplementedException();
        }

        private List<Ing> GetIngs(string html)
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
                    string strIngBody = ingBodyNode.InnerHtml;
                    string strIngTime = childNode.SelectSingleNode("//a[@class='ing_time']").Attributes["title"].Value;
                    strIngTime = strIngTime.Remove(0, "发布于".Length);
                    strIngTime = strIngTime.Remove(strIngTime.Length - "，点击进入详细页面".Length);
                    //img_middle
                    bool blIsFromePhone = childNode.SelectSingleNode("//img[@class='img_middle']") != null;
                    bool blIsNewbie = childNode.SelectSingleNode("//img[@class='ing_icon_newbie']") != null;
                    bool blIsLucky = childNode.SelectSingleNode("//img[@class='ing_icon_lucky']") != null;
                    string strAvatarUri = childNode.SelectSingleNode("//div[@class='feed_avatar']/a/img").Attributes["src"].Value;
                    string strUserId = strAvatarUri.Remove(0, "http://pic.cnitblog.com/face/".Length);
                    if (strUserId.IndexOf("sample_face.gif") == -1)
                    {
                        strUserId = strUserId.Substring(0, strUserId.IndexOf("/"));
                    }
                    else
                    {
                        strUserId = childNode.SelectSingleNode("//div[@class='feed_avatar']/a").Attributes["href"].Value;
                        strUserId = strUserId.Remove(0, "/u/".Length);
                        strUserId = strUserId.Remove(strUserId.Length - 1);
                    }

                    string strUserName = childNode.SelectSingleNode("//a[@class='ing-author']").Attributes["href"].Value;
                    strUserName = strUserName.Remove(0, "/u/".Length);
                    strUserName = strUserName.Remove(strUserName.Length - 1);
                    string strUserNickName = childNode.SelectSingleNode("//a[@class='ing-author']").InnerText;
                    Ing ing = new Ing()
                    {
                        Id = strIngId,
                        Body = strIngBody,
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
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            return ings;
        }
    }
}
