using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Chauffeur.Deliverables
{
    [DeliverableName("user")]
    [DeliverableAlias("u")]
    public sealed class UserDeliverable : Deliverable, IProvideDirections
    {
        private readonly IUserService userService;

        public UserDeliverable(
            TextReader reader,
            TextWriter writer,
            IUserService userService
        ) : base(reader, writer)
        {
            this.userService = userService;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            if (!args.Any())
            {
                await Out.WriteLineAsync("No operation for the user was provided");
                return DeliverableResponse.Continue;
            }

            var operation = args[0];

            switch (operation)
            {
                case "change-password":
                    await ChangePassword(args.Skip(1).ToArray());
                    return DeliverableResponse.Continue;

                default:
                    await Out.WriteLineFormattedAsync("The user operation '{0}' is not supported", operation);
                    return DeliverableResponse.Continue;
            }
        }

        private async Task ChangePassword(string[] args)
        {
            if (args.Length != 2)
            {
                await Out.WriteLineAsync("The expected parameters for 'change-password' were not supplied.");
                await Out.WriteLineAsync("Format expected: change-password <User Id> <old password> <new password>");
                return;
            }

            var username = args[0];
            var user = userService.GetByUsername(username);

            if (user == null)
            {
                await Out.WriteLineFormattedAsync("User '{0}' does not exist in the system", username);
                return;
            }

            userService.SavePassword(user, args[1]);
            await Out.WriteLineFormattedAsync("User '{0}' has had their password updated", username);
        }

        public async Task Directions()
        {
            await Out.WriteLineAsync("A series of operations that can be run against an Umbraco User.");
            await Out.WriteLineAsync();

            await Out.WriteLineAsync("change-password <username> <new password>");
            await Out.WriteLineAsync("\tChanges the password for a given user. This will also hash it if hashing is turned on in the web.config");
        }
    }
}
