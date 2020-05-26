using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Article.ViewModel
{
    public class ArticleRequest : PageSearchRequest
    {
        /// <summary>
        /// 文章标题
        /// </summary>
        public string Title { get; set; }
        public int pageData { get; set; }
        /// <summary>
        /// 1交流板块，2运输板块，3我的收藏
        /// </summary>
        public int type { get; set; }
        public string UserCode { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }
    }
}
