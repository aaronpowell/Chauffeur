using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Security;

namespace Chauffeur.Deliverables
{
    [DeliverableName("user")]
    public sealed class UserDeliverable : Deliverable
    {
        private readonly UmbracoMembershipProviderBase membershipProvider;

        public UserDeliverable(
            TextReader reader,
            TextWriter writer,
            UmbracoMembershipProviderBase membershipProvider
        ) : base(reader, writer)
        {
            this.membershipProvider = membershipProvider;
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

            return await base.Run(command, args);
        }

        private async Task ChangePassword(string[] args)
        {
            if (args.Length != 3)
            {
                await Out.WriteLineAsync("The expected parameters for 'change-password' were not supplied.");
                await Out.WriteLineAsync("Format expected: change-password <User Id> <old password> <new password>");
                return;
            }

            int userId;
            if (!int.TryParse(args[0], out userId))
            {
                await Out.WriteLineAsync("The provided user id is not valid");
                return;
            }

            var user = membershipProvider.GetUser(userId, false);

            if (user == null)
            {
                await Out.WriteLineFormattedAsync("User '{0}' does not exist in the system", userId);
                return;
            }

            if (user.ChangePassword(args[1], args[2]))
            {
                await Out.WriteLineFormattedAsync("User '{0}' has had their password updated", userId);
            }
            else
            {
                await Out.WriteLineFormattedAsync("Unable to update the password for user '{0}'. Ensure the old password is correct.");
            }
        }
    }
}
