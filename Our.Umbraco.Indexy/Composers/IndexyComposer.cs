using Examine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Our.Umbraco.Indexy.Indexer;
using Our.Umbraco.Indexy.Models;
using System.Linq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine;

namespace Our.Umbraco.Indexy.Composers
{
    public class IndexyComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddOptions<AppSettingsConfiguration>().Bind(builder.Config.GetSection("Our.Umbraco.Indexy"));
            var indexySettings = builder.Config.GetSection("Our.Umbraco.Indexy").Get<AppSettingsConfiguration>();

            foreach (IndexyIndexGroup item in (indexySettings.Groups ?? Enumerable.Empty<IndexyIndexGroup>()))
            {
                builder.Services.AddExamineLuceneIndex<IndexyUmbracoExamineIndex, ConfigurationEnabledDirectoryFactory>(item.IndexName);
            }

            builder.Services.ConfigureOptions<ConfigureIndexyIndexOptions>();
            builder.Services.AddSingleton<IndexyIndexValueSetBuilder>();
            builder.Services.AddSingleton<IIndexPopulator, IndexyIndexPopulator>();
        }
    }
}
