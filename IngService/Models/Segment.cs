﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace IngService.Models
{
    [KnownType(typeof(SegmentUrl))]
    [KnownType(typeof(SegmentUser))]
    /// <summary>
    /// 闪存内容片段或者回复内容片段
    /// </summary>
    public class Segment
    {
        public SegmentType Type { get; set; }
        public string Text { get; set; }
    }

    public class SegmentUrl : Segment
    {
        public string Url { get; set; }
    }

    public class SegmentUser : Segment
    {
        public string UserId { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SegmentType
    {
        Link,
        Tag,
        Text,
        User
    }
}