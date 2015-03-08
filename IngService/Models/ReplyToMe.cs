using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IngService.Models
{
    public class ReplyToMe
    {
        /// <summary>
        /// 回复者id eg:289132
        /// </summary>
        public string ReplyerId { get; set; }
        /// <summary>
        /// 回复者用户名 //a[@class="big_font blue"]
        /// eg:grj1046
        /// </summary>
        public string ReplyerName { get; set; }
        /// <summary>
        /// 回复者昵称
        /// </summary>
        public string ReplyerNickName { get; set; }
        /// <summary>
        /// 回复者头像uri //div[@class='feed_avatar']/a/img
        /// </summary>
        public string ReplyerAvatarUri { get; set; }

        /// <summary>
        /// 回复的闪存id //a[@class='comment-body-gray'].attribute["href"]
        /// </summary>
        public string IngId { get; set; }
        /// <summary>
        /// 回复的闪存回复的id
        /// </summary>
        public string ReplyContentId { get; set; }
        /// <summary>
        /// 回复的闪存评论 //a[@class='comment-body-gray']
        /// </summary>
        public string ReplyContent { get; set; }
        /// <summary>
        /// 回复的内容 //span[@class='ing_body']
        /// </summary>
        public string ReplyMsg { get; set; }
        /// <summary>
        /// 回复时间 //a[@class="ing_time"]
        /// </summary>
        public string ReplyTime { get; set; }
    }
}