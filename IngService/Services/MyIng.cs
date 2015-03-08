using IngService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IngService.Services
{
    /// <summary>
    /// 我的闪存
    /// </summary>
    public class MyIng : Ing
    {
        /// <summary>
        /// 是否是私有闪存
        /// </summary>
        public bool IsPrivate { get; set; }
    }
}