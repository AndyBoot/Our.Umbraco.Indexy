using Examine;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Our.Umbraco.Indexy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static Lucene.Net.Queries.Function.ValueSources.MultiFunction;
using static Umbraco.Cms.Core.Constants;
using static Umbraco.Cms.Core.Constants.Conventions;

namespace Our.Umbraco.Indexy.Indexer
{
    public class IndexyIndexValueSetBuilder : IValueSetBuilder<IContent>
    {
        private static readonly object[] NoValue = new[] { "n" };
        private static readonly object[] YesValue = new[] { "y" };
        private readonly IScopeProvider _scopeProvider;
        private readonly IOptions<AppSettingsConfiguration> _indexySettings;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly UrlSegmentProviderCollection _urlSegmentProviders;
        private readonly IUserService _userService;


        //public IndexyIndexValueSetBuilder(
        //    PropertyEditorCollection propertyEditors,
        //    IOptions<AppSettingsConfiguration> indexySettings,
        //    IShortStringHelper shortStringHelper,
        //    UrlSegmentProviderCollection urlSegmentProviders,
        //    IUserService userService,
        //    IScopeProvider scopeProvider,
        //    bool publishedValuesOnly) : base(propertyEditors, publishedValuesOnly)        
            public IndexyIndexValueSetBuilder(
            IOptions<AppSettingsConfiguration> indexySettings,
            IShortStringHelper shortStringHelper,
            UrlSegmentProviderCollection urlSegmentProviders,
            IUserService userService,
            IScopeProvider scopeProvider)
        {
            _indexySettings = indexySettings;
            _shortStringHelper = shortStringHelper;
            _urlSegmentProviders = urlSegmentProviders;
            _userService = userService;
            _scopeProvider = scopeProvider;
        }

        public IEnumerable<ValueSet> GetValueSets(params IContent[] contents)
        {
            Dictionary<int, IProfile> creatorIds;
            Dictionary<int, IProfile> writerIds;

            // We can lookup all of the creator/writer names at once which can save some
            // processing below instead of one by one.
            using (IScope scope = _scopeProvider.CreateScope())
            {
                creatorIds = _userService.GetProfilesById(contents.Select(x => x.CreatorId).ToArray())
                    .ToDictionary(x => x.Id, x => x);
                writerIds = _userService.GetProfilesById(contents.Select(x => x.WriterId).ToArray())
                    .ToDictionary(x => x.Id, x => x);
                scope.Complete();
            }

            foreach (var content in contents)
            {
                var isVariant = content.ContentType.VariesByCulture();

                var urlValue = content.GetUrlSegment(_shortStringHelper, _urlSegmentProviders); // Always add invariant urlName
                var indexValues = new Dictionary<string, IEnumerable<object?>>
            {
                    { "icon", content.ContentType.Icon?.Yield() ?? Enumerable.Empty<string>() },
                {
                    UmbracoExamineFieldNames.PublishedFieldName, content.Published ? YesValue : NoValue
                }, // Always add invariant published value
                { "id", new object[] { content.Id } },
                { UmbracoExamineFieldNames.NodeKeyFieldName, new object[] { content.Key } },
                { "parentID", new object[] { content.Level > 1 ? content.ParentId : -1 } },
                { "level", new object[] { content.Level } },
                { "creatorID", new object[] { content.CreatorId } },
                { "sortOrder", new object[] { content.SortOrder } },
                { "createDate", new object[] { content.CreateDate } }, // Always add invariant createDate
                { "updateDate", new object[] { content.UpdateDate } }, // Always add invariant updateDate
                { "nodeName", new object[] { (content.PublishName ?? string.Empty) } },
                //{
                //    UmbracoExamineFieldNames.NodeNameFieldName, (PublishedValuesOnly // Always add invariant nodeName
                //        ? content.PublishName?.Yield()
                //        : content.Name?.Yield()) ?? Enumerable.Empty<string>()
                //},
                { "urlName", urlValue?.Yield() ?? Enumerable.Empty<string>() }, // Always add invariant urlName
                { "path", content.Path.Yield() },
                { "nodeType", content.ContentType.Id.ToString().Yield() },
                {
                    "creatorName",
                    (creatorIds.TryGetValue(content.CreatorId, out IProfile? creatorProfile) ? creatorProfile.Name! : "??")
                    .Yield()
                },
                {
                    "writerName",
                    (writerIds.TryGetValue(content.WriterId, out IProfile? writerProfile) ? writerProfile.Name! : "??")
                    .Yield()
                },
                { "writerID", new object[] { content.WriterId } },
                { "templateID", new object[] { content.TemplateId ?? 0 } },
                { UmbracoExamineFieldNames.VariesByCultureFieldName, NoValue },
            };



                //var indexValues = new Dictionary<string, object>();

                foreach (var item in (_indexySettings.Value.Groups ?? Enumerable.Empty<IndexyIndexGroup>()))
                {
                    if (item.Fields == null) continue;
                    var systemFields = item.Fields!.SystemFields?.Replace(" ", "").Split(',');
                    if (systemFields != null)
                    {
                        indexValues = indexValues.Where(w => 
                        systemFields!.Contains(w.Key) || 
                        w.Key.Equals("id", StringComparison.InvariantCultureIgnoreCase) || 
                        w.Key.Equals("nodeName", StringComparison.InvariantCultureIgnoreCase)
                        ).ToDictionary(w => w.Key, w => w.Value);
                    }

                    AddIndexValues(indexValues, item.Fields!.Integer!, content);
                    AddIndexValues(indexValues, item.Fields!.Float!, content);
                    AddIndexValues(indexValues, item.Fields!.Double!, content);
                    AddIndexValues(indexValues, item.Fields!.Long!, content);
                    AddIndexValues(indexValues, item.Fields!.DateTime!, content);
                    AddIndexValues(indexValues, item.Fields!.DateYear!, content);
                    AddIndexValues(indexValues, item.Fields!.DateMonth!, content);
                    AddIndexValues(indexValues, item.Fields!.DateDay!, content);
                    AddIndexValues(indexValues, item.Fields!.DateHour!, content);
                    AddIndexValues(indexValues, item.Fields!.DateMinute!, content);
                    AddIndexValues(indexValues, item.Fields!.Raw!, content);
                    AddIndexValues(indexValues, item.Fields!.FullText!, content);
                    AddIndexValues(indexValues, item.Fields!.FullTextSortable!, content);
                    AddIndexValues(indexValues, item.Fields!.InvariantCultureIgnoreCase!, content);
                    AddIndexValues(indexValues, item.Fields!.EmailAddress!, content);
                }

                yield return new ValueSet(content.Id.ToInvariantString(), IndexTypes.Content, content.ContentType.Alias, indexValues);
            }
        }

        private static void AddIndexValues(Dictionary<string, IEnumerable<object?>> indexValues, string fields, IContent content)
        {
            if (!string.IsNullOrEmpty(fields))
            {
                foreach (var field in fields.Replace(" ", "").Split(','))
                {
                    if (!indexValues.ContainsKey(field))
                    {
                        indexValues.Add(field, new object[] { content.GetValue(field) ?? string.Empty });
                    }
                }
            }
        }
    }
}
