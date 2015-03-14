using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IngService.Models
{
    public class Ing
    {
        public string Id { get; set; }
        public List<Segment> Body { get; set; }
        public string PublishTime { get; set; }
        /// <summary>
        /// 来自手机版 //img[@class='img_middle']
        /// </summary>
        public bool IsFromePhone { get; set; }
        /// <summary>
        /// 是否新人闪 //img[@class='ing_icon_newbie']
        /// </summary>
        public bool IsNewbie { get; set; }
        /// <summary>
        /// 是否幸运闪 //img[@class='ing_icon_lucky']
        /// </summary>
        public bool IsLucky { get; set; }
        /// <summary>
        /// 用户id  eg:289132
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 用户名 eg:grj1046
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户昵称 eg:_nil
        /// </summary>
        public string UserNickName { get; set; }
        public string UserAvatarUri { get; set; }
        //public string UserHomeUri { get; set; }
    }
}