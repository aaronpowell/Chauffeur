using Chauffeur.Host;
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
        private readonly IPackagingService packagingService;
        private readonly IFileSystem fileSystem;
        private readonly IChauffeurSettings settings;

        public DataTypeDeliverable(
            TextReader reader,
            TextWriter writer,
            IDataTypeService dataTypeService,
            IPackagingService packagingService,
            IFileSystem fileSystem,
            IChauffeurSettings settings) : base(reader, writer)
        {
            this.dataTypeService = dataTypeService;
            this.packagingService = packagingService;
            this.fileSystem = fileSystem;
            this.settings = settings;
        }

        public async Task Directions()
        {
            await Out.WriteLineAsync("data-type");
            await Out.WriteLineAsync("\tPerform operations against Data Type Definitions");

            await Out.WriteLineAsync("Available Operations:");
            await Out.WriteLineAsync("\texport ?<...ids>");
            await Out.WriteLineAsync("\t\tExports all data type definitions that match the provided ID's");
            await Out.WriteLineAsync("\t\tIf no ids are provided all data type definitions are exported");
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
                    await ExportDataType(args.Skip(1));
                    return DeliverableResponse.Continue;

                default:
                    await Out.WriteLineAsync($"The operation `{operation}` is not currently supported");
                    return DeliverableResponse.Continue;
            }
        }

        private async Task ExportDataType(IEnumerable<string> dataTypes)
        {
            IEnumerable<IDataTypeDefinition> dataTypeDefinitions;

            if (dataTypes.Any())
            {
                var ids = dataTypes.Select(int.Parse);
                dataTypeDefinitions = dataTypeService.GetAllDataTypeDefinitions(ids.ToArray());
            }
            else
            {
                dataTypeDefinitions = dataTypeService.GetAllDataTypeDefinitions();
            }

            if (!settings.TryGetChauffeurDirectory(out string exportDirectory))
                return;

            var xml = new XDocument();

            xml.Add(packagingService.Export(dataTypeDefinitions, false));

            var fileName = DateTime.UtcNow.ToString("yyyyMMdd") + "-data-type-definitions.xml";
            fileSystem.File.WriteAllText(fileSystem.Path.Combine(exportDirectory, fileName), xml.ToString());
            await Out.WriteLineFormattedAsync("Data Type Definitions have been exported with file name '{0}'", fileName);
        }
    }
}
