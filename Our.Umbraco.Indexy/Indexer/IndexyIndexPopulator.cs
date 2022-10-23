using Examine;
using Microsoft.Extensions.Options;
using Our.Umbraco.Indexy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;

namespace Our.Umbraco.Indexy.Indexer
{
    public class IndexyIndexPopulator : IndexPopulator<IUmbracoContentIndex>
    {
        private readonly IContentService _contentService;
        private readonly IndexyIndexValueSetBuilder _productIndexValueSetBuilder;
        private readonly IOptions<AppSettingsConfiguration> _indexySettings;
        private readonly IContentValueSetBuilder _contentValueSetBuilder;
        private readonly IContentTypeService _contentTypeService;


        public IndexyIndexPopulator(
            IContentService contentService,
            IndexyIndexValueSetBuilder productIndexValueSetBuilder,
            IOptions<AppSettingsConfiguration> indexySettings,
            IContentValueSetBuilder contentValueSetBuilder,
            IContentTypeService contentTypeService)
        {
            _contentService = contentService;
            _productIndexValueSetBuilder = productIndexValueSetBuilder;
            _indexySettings = indexySettings;
            _contentValueSetBuilder = contentValueSetBuilder;
            _contentTypeService = contentTypeService;

            if (_indexySettings.Value.Groups != null)
            {
                foreach (var item in _indexySettings.Value.Groups!)
                {
                    RegisterIndex(item.IndexName);
                }
            }
        }
        protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
        {
            if (_indexySettings.Value.Groups == null) return;

            foreach (var index in indexes
                .Where(i => _indexySettings.Value.Groups.Any(g => g.IndexName.Equals(i.Name, StringComparison.InvariantCultureIgnoreCase)))
                .Select(i => new Tuple<IIndex, IndexyIndexGroup>(i, _indexySettings.Value.Groups!.FirstOrDefault(g => g.IndexName.Equals(i.Name, StringComparison.InvariantCultureIgnoreCase))))
                )
            {
                if (index.Item2 == null) continue;
                if (index.Item2.DocumentTypes == null) continue;

                var contentTypeIds = _contentTypeService.GetAllContentTypeIds(index.Item2.DocumentTypes!.Split(','));
                var valueSets = _productIndexValueSetBuilder.GetValueSets(_contentService.GetPagedOfTypes(contentTypeIds.ToArray(), 0, Int32.MaxValue, out _, null).ToArray());
                index.Item1.IndexItems(valueSets);
            }

        }
    }
}
