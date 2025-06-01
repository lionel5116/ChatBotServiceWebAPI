using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class DirectLineTokenResponse
    {
        public string conversationId { get; set; }
        public string token { get; set; }
        public int expires_in { get; set; }
    }
}