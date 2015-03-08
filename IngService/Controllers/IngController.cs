using IngService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using HtmlAgilityPack;
using System.Web.Security;
using System.Web;
using System.Security.Principal;
using IngService.Services;

namespace IngService.Controllers
{
    /// <summary>
    /// 闪存 http://home.cnblogs.com/ing/
    /// </summary>
    public class IngController : ApiController
    {
        /// <summary>
        /// 全站
        /// IngListType=all
        /// </summary>
        /// <returns></returns>
        /// http://home.cnblogs.com/ajax/ing/GetPagedIngList?IngListType=all&PageIndex=1&PageSize=30&Tag=&_=1420551939473
        [HttpGet]
        //[AllowAnonymous]
        public async Task<HttpResponseMessage> All(int pageIndex = 1, int pageSize = 30)
        {
            List<Ing> ings = new List<Ing>();
            Uri uri = IngServices.BuildUri(ingType: IngType.All, tag: "", pageIndex: pageIndex, pageSize: pageSize);
            string strIngHtml = await IngServices.GetResponseMessage(uri);
            ings = IngServices.GetIngs(strIngHtml);
            return this.Request.CreateResponse<IEnumerable<Ing>>(HttpStatusCode.OK, ings);
        }

        /// <summary>
        /// 回复我
        /// IngListType=ReplyToMe
        /// </summary>
        /// <returns></returns>
        /// http://home.cnblogs.com/ajax/ing/GetPagedIngList?IngListType=ReplyToMe&PageIndex=1&PageSize=30&Tag=&_=1424666047593
        [HttpGet]
        public async Task<HttpResponseMessage> ReplyToMe(int pageIndex = 1, int pageSize = 30)
        {
            List<ReplyToMe> ings = new List<ReplyToMe>();
            Uri uri = IngServices.BuildUri(ingType: IngType.ReplyToMe, tag: "", pageIndex: pageIndex, pageSize: pageSize);
            string strIngHtml = await IngServices.GetResponseMessage(uri); ;
            IngServices.CheckLogin(strIngHtml);
            //如果能够执行下述代码 证明身份已验证
            ings = IngServices.GetReplyToMe(strIngHtml);
            return this.Request.CreateResponse<IEnumerable<ReplyToMe>>(HttpStatusCode.OK, ings);
        }

        /// <summary>
        /// 提到我
        /// </summary>
        /// <returns></returns>
        /// http://home.cnblogs.com/ajax/ing/GetPagedIngList?IngListType=MentionedMe&PageIndex=1&PageSize=30&Tag=&_=1425799861982
        [HttpGet]
        public async Task<HttpResponseMessage> MentionedMe(int pageIndex = 1, int pageSize = 30)
        {
            List<Ing> ings = new List<Ing>();
            Uri uri = IngServices.BuildUri(ingType: IngType.MentionedMe, tag: "", pageIndex: pageIndex, pageSize: pageSize);
            string strIngHtml = await IngServices.GetResponseMessage(uri);
            IngServices.CheckLogin(strIngHtml);
            //如果能够执行下述代码 证明身份已验证
            ings = IngServices.GetIngs(strIngHtml);
            return this.Request.CreateResponse<IEnumerable<Ing>>(HttpStatusCode.OK, ings);
        }
        /// <summary>
        /// 新回应
        /// </summary>
        /// <returns></returns>
        /// http://home.cnblogs.com/ajax/ing/GetPagedIngList?IngListType=reply&PageIndex=1&PageSize=30&Tag=&_=1425800311558
        [HttpGet]
        public async Task<HttpResponseMessage> Reply(int pageIndex = 1, int pageSize = 30)
        {
            List<Ing> ings = new List<Ing>();
            Uri uri = IngServices.BuildUri(ingType: IngType.Reply, tag: "", pageIndex: pageIndex, pageSize: pageSize);
            string strIngHtml = await IngServices.GetResponseMessage(uri);
            IngServices.CheckLogin(strIngHtml);
            //如果能够执行下述代码 证明身份已验证
            ings = IngServices.GetIngs(strIngHtml);
            return this.Request.CreateResponse<IEnumerable<Ing>>(HttpStatusCode.OK, ings);
        }
        /// <summary>
        /// 我回应
        /// </summary>
        /// <returns></returns>
        /// http://home.cnblogs.com/ajax/ing/GetPagedIngList?IngListType=myreply&PageIndex=1&PageSize=30&Tag=&_=1425800338198
        [HttpGet]
        public async Task<HttpResponseMessage> MyReply(int pageIndex = 1, int pageSize = 30)
        {
            List<Ing> ings = new List<Ing>();
            Uri uri = IngServices.BuildUri(ingType: IngType.MyReply, tag: "", pageIndex: pageIndex, pageSize: pageSize);
            string strIngHtml = await IngServices.GetResponseMessage(uri);
            IngServices.CheckLogin(strIngHtml);
            //如果能够执行下述代码 证明身份已验证
            ings = IngServices.GetIngs(strIngHtml);
            return this.Request.CreateResponse<IEnumerable<Ing>>(HttpStatusCode.OK, ings);
        }
        /// <summary>
        /// 我的
        /// </summary>
        /// <returns></returns>
        /// http://home.cnblogs.com/ajax/ing/GetPagedIngList?IngListType=my&PageIndex=1&PageSize=30&Tag=&_=1425800376510
        [HttpGet]
        public async Task<HttpResponseMessage> My(int pageIndex = 1, int pageSize = 30)
        {
            List<MyIng> ings = new List<MyIng>();
            Uri uri = IngServices.BuildUri(ingType: IngType.My, tag: "", pageIndex: pageIndex, pageSize: pageSize);
            string strIngHtml = await IngServices.GetResponseMessage(uri);
            IngServices.CheckLogin(strIngHtml);
            //如果能够执行下述代码 证明身份已验证
            ings = IngServices.GetMyIngs(strIngHtml);
            return this.Request.CreateResponse<IEnumerable<MyIng>>(HttpStatusCode.OK, ings);
        }
        /// <summary>
        /// 关注
        /// </summary>
        /// <returns></returns>
        /// http://home.cnblogs.com/ajax/ing/GetPagedIngList?IngListType=recent&PageIndex=1&PageSize=30&Tag=&_=1425800402549
        [HttpGet]
        public async Task<HttpResponseMessage> Recent(int pageIndex = 1, int pageSize = 30)
        {
            List<Ing> ings = new List<Ing>();
            Uri uri = IngServices.BuildUri(ingType: IngType.Recent, tag: "", pageIndex: pageIndex, pageSize: pageSize);
            string strIngHtml = await IngServices.GetResponseMessage(uri);
            IngServices.CheckLogin(strIngHtml);
            //如果能够执行下述代码 证明身份已验证
            ings = IngServices.GetIngs(strIngHtml);
            return this.Request.CreateResponse<IEnumerable<Ing>>(HttpStatusCode.OK, ings);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> PostComment()
        {
            CommentModel model = this.Request.Content.ReadAsAsync<CommentModel>().Result;

            if (string.IsNullOrEmpty(model.ContentId)
                || string.IsNullOrEmpty(model.ReplyTo)
                || string.IsNullOrEmpty(model.ParentCommentId)
                || string.IsNullOrEmpty(model.Content))
            {
                var errorResponse = this.Request.CreateResponse(HttpStatusCode.BadRequest);
                errorResponse.Content = new StringContent("发送信息不完整");
                return errorResponse;
            }

            string html = await IngServices.PostComment(model);
            var successResponse = this.Request.CreateResponse(HttpStatusCode.OK);
            successResponse.Content = new StringContent(html);
            return successResponse;
        }

    }
}
