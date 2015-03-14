using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IngService.Models
{
    /// <summary>
    /// 闪存评论
    /// </summary>
    public class IngComment
    {
        /// <summary>
        /// 闪存id
        /// </summary>
        public string IngId { get; set; }
        /// <summary>
        /// 评论id
        /// </summary>
        public string CommentId { get; set; }
        /// <summary>
        /// 评论人id
        /// </summary>
        public string ReplierId { get; set; }
        /// <summary>
        /// 评论人名称
        /// </summary>
        public string ReplierName { get; set; }
        /// <summary>
        /// 评论人昵称
        /// </summary>
        public string ReplierNickName { get; set; }
        /// <summary>
        /// 评论内容
        /// </summary>
        public List<Segment> ReplyContent { get; set; }
        /// <summary>
        /// 回复时间 ing_comment_time.innerHtml
        /// </summary>
        public string ReplyTime { get; set; }
        /// <summary>
        /// 当前账户是否你够删除该回复
        /// 注：用户不能回复自己的评论，能删除则不能回复。
        /// </summary>
        public bool CanDelete { get; set; }
    }
}