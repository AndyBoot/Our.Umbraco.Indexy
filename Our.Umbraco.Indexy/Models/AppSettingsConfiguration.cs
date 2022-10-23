using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Configuration.Models;

namespace Our.Umbraco.Indexy.Models
{
    public class AppSettingsConfiguration
    {
        public IEnumerable<IndexyIndexGroup>? Groups { get; set; }
    }

    public class IndexyIndexGroup
    {
        public string IndexName { get; set; } = string.Empty;
        public string? DocumentTypes { get; set; }
        public IndexyFields? Fields { get; set; }
    }

    public class IndexyFields
    {
        public string? SystemFields { get; set; } = "__Icon,__Key,__Path,__Published,__VariesByCulture,createDate,creatorID,creatorName,email,icon,id,level,nodeName,nodeType,parentID,path,sortOrder,template,templateID,updateDate,urlName,version,writerID,writerName";
        public string? Integer { get; set; }
        public string? Float { get; set; }
        public string? Double { get; set; }
        public string? Long { get; set; }
        public string? DateTime { get; set; }
        public string? DateYear { get; set; }
        public string? DateMonth { get; set; }
        public string? DateDay { get; set; }
        public string? DateHour { get; set; }
        public string? DateMinute { get; set; }
        public string? Raw { get; set; }
        public string? FullText { get; set; }
        public string? FullTextSortable { get; set; }
        public string? InvariantCultureIgnoreCase { get; set; }
        public string? EmailAddress { get; set; }
    }
}
