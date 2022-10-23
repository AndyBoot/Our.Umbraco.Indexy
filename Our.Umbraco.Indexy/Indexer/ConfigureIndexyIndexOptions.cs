using Examine.Lucene;
using Examine;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Util;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Core.Scoping;
using Our.Umbraco.Indexy.Models;
using FieldDefinition = Examine.FieldDefinition;
using System.Linq;

namespace Our.Umbraco.Indexy.Indexer
{
    public class ConfigureIndexyIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
    {
        private readonly IOptions<IndexCreatorSettings> _settings;
        private readonly IOptions<AppSettingsConfiguration> _indexySettings;

        public ConfigureIndexyIndexOptions(
            IOptions<IndexCreatorSettings> settings,
            IOptions<AppSettingsConfiguration> indexySettings)
        {
            _settings = settings;
            _indexySettings = indexySettings;
        }

        public void Configure(string name, LuceneDirectoryIndexOptions options)
        {
            if (_indexySettings.Value.Groups == null || _indexySettings.Value.Groups?.Any() == false) return;

            foreach (var item in _indexySettings.Value.Groups!.Where(w => w.IndexName.Equals(name, StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(w.DocumentTypes)).Take(1))
            {
                options.Analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

                if (item.Fields != null)
                {
                    FieldDefinitionCollection fieldDefinitions = new UmbracoFieldDefinitionCollection();
                    var systemFields = item.Fields.SystemFields?.Replace(" ", "").Split(',').ToList();
                    if (!systemFields!.Contains("id"))
                    {
                        systemFields.Add("id");
                    }
                    if (!systemFields!.Contains("nodeName"))
                    {
                        systemFields.Add("nodeName");
                    }

                    fieldDefinitions = new FieldDefinitionCollection(fieldDefinitions.Where(w => systemFields?.Any(x => x.Equals(w.Name, StringComparison.InvariantCultureIgnoreCase)) == true).ToArray());

                    AddFieldDefinition(fieldDefinitions, item.Fields.Integer!, FieldDefinitionTypes.Integer);
                    AddFieldDefinition(fieldDefinitions, item.Fields.Float!, FieldDefinitionTypes.Float);
                    AddFieldDefinition(fieldDefinitions, item.Fields.Double!, FieldDefinitionTypes.Double);
                    AddFieldDefinition(fieldDefinitions, item.Fields.Long!, FieldDefinitionTypes.Long);
                    AddFieldDefinition(fieldDefinitions, item.Fields.DateTime!, FieldDefinitionTypes.DateTime);
                    AddFieldDefinition(fieldDefinitions, item.Fields.DateYear!, FieldDefinitionTypes.DateYear);
                    AddFieldDefinition(fieldDefinitions, item.Fields.DateMonth!, FieldDefinitionTypes.DateMonth);
                    AddFieldDefinition(fieldDefinitions, item.Fields.DateDay!, FieldDefinitionTypes.DateDay);
                    AddFieldDefinition(fieldDefinitions, item.Fields.DateHour!, FieldDefinitionTypes.DateHour);
                    AddFieldDefinition(fieldDefinitions, item.Fields.DateMinute!, FieldDefinitionTypes.DateMinute);
                    AddFieldDefinition(fieldDefinitions, item.Fields.Raw!, FieldDefinitionTypes.Raw);
                    AddFieldDefinition(fieldDefinitions, item.Fields.FullText!, FieldDefinitionTypes.FullText);
                    AddFieldDefinition(fieldDefinitions, item.Fields.FullTextSortable!, FieldDefinitionTypes.FullTextSortable);
                    AddFieldDefinition(fieldDefinitions, item.Fields.InvariantCultureIgnoreCase!, FieldDefinitionTypes.InvariantCultureIgnoreCase);
                    AddFieldDefinition(fieldDefinitions, item.Fields.EmailAddress!, FieldDefinitionTypes.EmailAddress);
                    options.FieldDefinitions = fieldDefinitions;
                }


                options.UnlockIndex = true;
                if (_settings.Value.LuceneDirectoryFactory == LuceneDirectoryFactory.SyncedTempFileSystemDirectoryFactory)
                {
                    // if this directory factory is enabled then a snapshot deletion policy is required
                    options.IndexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
                }
            }
        }

        private static void AddFieldDefinition(FieldDefinitionCollection fieldDefinitions, string fields, string fieldType)
        {
            if (!string.IsNullOrEmpty(fields))
            {
                foreach (var field in fields.Replace(" ", "").Split(','))
                {
                    fieldDefinitions.AddOrUpdate(new FieldDefinition(field, fieldType));
                }
            }
        }

        public void Configure(LuceneDirectoryIndexOptions options)
        {
            throw new System.NotImplementedException();
        }
    }
}
