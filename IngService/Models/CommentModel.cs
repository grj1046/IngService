using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IngService.Models
{
    public class CommentModel
    {
        //{"ContentId":645536,"ReplyTo":463726,"ParentCommentId":892566,"Content":"@枕头妹：沾了星星的光"}
        public string ContentId { get; set; }
        public string ReplyTo { get; set; }
        public string ParentCommentId { get; set; }
        public string Content { get; set; }
    }
}