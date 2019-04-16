using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Task = System.Threading.Tasks.Task;

namespace Chauffeur.Deliverables
{
    [DeliverableName("data-type")]
    public class DataTypeDeliverable : Deliverable, IProvideDirections
    {
        private readonly IDataTypeService dataTypeService;
        private readonly IFileSystem fileSystem;
        private readonly IChauffeurSettings settings;
        private readonly IEntityXmlSerializer serializer;

        public DataTypeDeliverable(
            TextReader reader,
            TextWriter writer,
            IDataTypeService dataTypeService,
            IFileSystem fileSystem,
            IChauffeurSettings settings,
            IEntityXmlSerializer serializer) : base(reader, writer)
        {
            this.dataTypeService = dataTypeService;
            this.fileSystem = fileSystem;
            this.settings = settings;
            this.serializer = serializer;
        }

        public async Task<bool> Directions()
        {
            await Out.WriteLineAsync("data-type");
            await Out.WriteLineAsync("\tPerform operations against Data Type Definitions");

            await Out.WriteLineAsync("Available Operations:");
            await Out.WriteLineAsync("\texport ?<...ids>");
            await Out.WriteLineAsync("\t\tExports all data type definitions that match the provided ID's");
            await Out.WriteLineAsync("\t\tIf no ids are provided all data type definitions are exported");
            return true;
        }

        public async override Task<DeliverableResponse> Run(string command, string[] args)
        {
            if (!args.Any())
            {
                await Out.WriteLineAsync("Please specify a command to run and any arguments it requires");
                return DeliverableResponse.Continue;
            }

            var operation = args[0];

            switch (operation.ToLower())
            {
                case "export":
                    await Export(args.Skip(1));
                    return DeliverableResponse.Continue;

                case "import":
                    await Import(args.Skip(1).ToArray());
                    return DeliverableResponse.Continue;

                default:
                    await Out.WriteLineAsync($"The operation `{operation}` is not currently supported");
                    return DeliverableResponse.Continue;
            }
        }

        private async Task Import(string[] args)
        {
            //if (!args.Any())
            //{
            //    await Out.WriteLineAsync("No import target defined");
            //    return;
            //}

            //var deliveryName = args[0].Trim();

            //string directory;
            //if (!settings.TryGetChauffeurDirectory(out directory))
            //    return;

            //var file = fileSystem.Path.Combine(directory, deliveryName + ".xml");
            //if (!fileSystem.File.Exists(file))
            //{
            //    await Out.WriteLineAsync($"Unable to located the import script '{deliveryName}'");
            //    return;
            //}

            //var xml = XDocument.Load(file);

            //packagingService.ImportDataTypeDefinitions(xml.Elements().First());

            //await Out.WriteLineAsync("Data Type Definitions have been imported");

            throw new NotImplementedException();
        }

        private async Task Export(IEnumerable<string> dataTypes)
        {
            IEnumerable<IDataType> dataTypeDefinitions;

            if (dataTypes.Any())
            {
                var ids = dataTypes.Select(int.Parse);
                dataTypeDefinitions = dataTypeService.GetAll(ids.ToArray());
            }
            else
            {
                dataTypeDefinitions = dataTypeService.GetAll();
            }

            if (!settings.TryGetChauffeurDirectory(out string exportDirectory))
                return;

            var xml = new XDocument();

            xml.Add(serializer.Serialize(dataTypeDefinitions));

            var fileName = DateTime.UtcNow.ToString("yyyyMMdd") + "-data-type-definitions.xml";
            fileSystem.File.WriteAllText(fileSystem.Path.Combine(exportDirectory, fileName), xml.ToString());
            await Out.WriteLineAsync($"Data Type Definitions have been exported with file name '{fileName}'");
        }
    }
}