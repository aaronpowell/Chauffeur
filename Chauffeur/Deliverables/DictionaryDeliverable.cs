using Chauffeur.Host;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Core.Services;

namespace Chauffeur.Deliverables
{
    [DeliverableName("dictionary")]
    public class DictionaryDeliverable : Deliverable, IProvideDirections
    {
        private readonly IFileSystem fileSystem;
        private readonly IChauffeurSettings settings;
        private readonly IPackagingService packagingService;

        public DictionaryDeliverable(
            TextReader reader,
            TextWriter writer,
            IFileSystem fileSystem,
            IChauffeurSettings settings,
            IPackagingService packagingService)
            : base(reader, writer)
        {
            this.fileSystem = fileSystem;
            this.settings = settings;
            this.packagingService = packagingService;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            if (!args.Any())
            {
                await Out.WriteLineAsync("No packages were provided, use `help dictionary` to see usage");
                return DeliverableResponse.Continue;
            }

            string chauffeurFolder;
            if (!settings.TryGetChauffeurDirectory(out chauffeurFolder))
                return DeliverableResponse.Continue;

            bool importLangs = false;
            if (args.Length > 1)
            {
                var operation = args[1];
                switch (operation.ToLower())
                {
                    case "y":
                    case "yes":
                        importLangs = true;
                        break;
                }
            }

            await Unpack(args[0], chauffeurFolder, importLangs);

            return DeliverableResponse.Continue;
        }

        private async Task Unpack(string name, string chauffeurFolder, bool includeLanguages)
        {
            var fileLocation = fileSystem.Path.Combine(chauffeurFolder, name + ".xml");
            if (!fileSystem.File.Exists(fileLocation))
            {
                await Out.WriteLineAsync(string.Format("The package '{0}' is not found in the Chauffeur folder", name));
                return;
            }

            using (var stream = fileSystem.File.OpenRead(fileLocation))
            {
                var xml = XDocument.Load(stream);

                IEnumerable<Umbraco.Core.Models.ILanguage> importedLangs = new List<Umbraco.Core.Models.ILanguage>();
                if (includeLanguages)
                {
                    var languageNode = xml.Root.Element("Languages");
                    if (languageNode == null)
                    {
                        await Out.WriteLineAsync(string.Format("No languages found in package '{0}'. Moving on", name));
                    }
                    else
                    {
                        importedLangs = packagingService.ImportLanguages(languageNode);
                    }
                }

                var dictionaryItemsNode = xml.Root.Element("DictionaryItems");
                if (dictionaryItemsNode == null)
                {
                    await Out.WriteLineAsync(string.Format("{0} languages were imported, but no dictionary items found in package '{0}'. Moving on", name));
                    return;
                }

                var importedKeys = packagingService.ImportDictionaryItems(dictionaryItemsNode);

                await Out.WriteLineAsync(string.Format("{0} languages and {1} dictionary items from '{2}'.", importedLangs.Count(), importedKeys.Count(), name));
            }
        }

        public async Task Directions()
        {
            await Out.WriteLineAsync("dictionary");
            await Out.WriteLineAsync("\t - Use `dictionary <package-name>` to only load dictionary keys from the package.");
            await Out.WriteLineAsync("\t - Use `dictionary <package-name> y` to load any languages from the package before loading the dictionary items.");
        }
    }
}
