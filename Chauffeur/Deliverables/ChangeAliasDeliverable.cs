using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Services;

namespace Chauffeur.Deliverables
{
    [DeliverableName("change-alias")]
    [DeliverableAlias("ca")]
    public sealed class ChangeAliasDeliverable : Deliverable
    {
        private readonly IContentTypeService contentTypeService;

        public ChangeAliasDeliverable(
            TextWriter writer,
            TextReader reader,
            IContentTypeService contentTypeService
            )
            : base(reader, writer)
        {
            this.contentTypeService = contentTypeService;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            if (args.Length != 3)
            {
                await Out.WriteLineAsync("Invalid arguments, expected format of `change-alias <what> <old> <new>`");
                return DeliverableResponse.Continue;
            }

            var what = args[0].ToLower();

            var old = args[1];
            var @new = args[2];

            switch (what)
            {
                case "document-type":
                case "doc-type":
                case "dt":
                    var ct = contentTypeService.GetContentType(old);
                    ct.Alias = @new;
                    contentTypeService.Save(ct);
                    break;

                default:
                    await Out.WriteLineFormattedAsync("Presently we cannot change the alias of `{0}`. Run `help ca` to see what can be changed", what);
                    break;
            }

            return DeliverableResponse.Continue;
        }
    }
}
