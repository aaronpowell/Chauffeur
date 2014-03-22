using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;

namespace Chauffeur.Deliverables
{
    [DeliverableName("content-type")]
    [DeliverableAlias("ct")]
    public sealed class ContentTypeDeliverable : Deliverable, IProvideDirections
    {
        public ContentTypeDeliverable(TextReader reader, TextWriter writer)
            : base(reader, writer)
        {
        }

        public override async Task<DeliverableResponse> Run(string[] args)
        {
            var operation = args[0];

            switch (operation.ToLower())
            {
                case "get-all":
                    await GetAll();
                    break;

                case "get":
                    await Get(args.Skip(1).ToArray());
                    break;

                default:
                    await Out.WriteLineAsync(string.Format("The operation `{0}` is not supported", operation));
                    break;
            }

            return await base.Run(args);
        }

        private async Task Get(string[] args)
        {
            if (!args.Any())
            {
                await Out.WriteLineAsync("Please provide the numerical id or alias if the doc type to get");
                return;
            }

            int id;
            if (int.TryParse(args[0], out id))
            {
                var cts = CreateContentTypeService();
                var contentType = cts.GetContentType(id);

                if (contentType == null)
                {
                    await Out.WriteLineAsync(string.Format("No content type found with id of '{0}'", id));
                }
                else
                {
                    await Out.WriteLineAsync("\tId\tAlias\tName\tParent Id");
                    await Out.WriteLineAsync(
                        string.Format(
                            "\t{0}\t{1}\t{2}\t{3}",
                            contentType.Id,
                            contentType.Alias,
                            contentType.Name,
                            contentType.ParentId
                        )
                    );

                    await Out.WriteLineAsync("\tProperty Types");
                    await Out.WriteLineAsync("\tId\tName\tAlias\tMandatory\tData Type Id");
                    foreach (var propertyType in contentType.PropertyTypes)
                    {
                        await Out.WriteLineAsync(
                            string.Format(
                                "\t{0}\t{1}\t{2}\t{3}\t{4}",
                                propertyType.Id,
                                propertyType.Alias,
                                propertyType.Name,
                                propertyType.Mandatory,
                                propertyType.DataTypeId
                            )
                        );
                    }
                }

                return;
            }
        }

        private async Task GetAll()
        {
            var cts = CreateContentTypeService();

            var types = cts.GetAllContentTypes();

            if (!types.Any())
            {
                await Out.WriteLineAsync("No content types found.");
                return;
            }

            await Out.WriteLineAsync("\tId\tAlias\tName");
            foreach (var type in types)
                await Out.WriteLineAsync(string.Format("\t{0}\t{1}\t{2}", type.Id, type.Alias, type.Name));
        }

        private static ContentTypeService CreateContentTypeService()
        {
            var rf = new RepositoryFactory(true);
            var cs = new ContentService(rf);
            var ms = new MediaService(rf);
            var cts = new ContentTypeService(rf, cs, ms);

            var providerName = ConfigurationManager.ConnectionStrings["umbracoDbDSN"].ProviderName;

            var provider = TypeFinder.FindClassesWithAttribute<SqlSyntaxProviderAttribute>()
                .FirstOrDefault(p => p.GetCustomAttribute<SqlSyntaxProviderAttribute>(false).ProviderName == providerName);

            if (provider == null)
                throw new FileNotFoundException(string.Format("Unable to find SqlSyntaxProvider that is used for the provider type '{0}'", providerName));

            SqlSyntaxContext.SqlSyntaxProvider = (ISqlSyntaxProvider)Activator.CreateInstance(provider);
            return cts;
        }

        public async Task Directions()
        {
            await Out.WriteLineAsync("content-type");
            await Out.WriteLineAsync("\tPerform operations against Umbraco Content Types");
            await Out.WriteLineAsync("");
        }
    }
}
