using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using umbraco;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Chauffeur.Deliverables
{
    [DeliverableName("user")]
    [DeliverableAlias("u")]
    public sealed class UserDeliverable : Deliverable, IProvideDirections
    {
        private readonly IUserService userService;
        private readonly BackOfficeUserManager<BackOfficeIdentityUser> userManager;

        public UserDeliverable(
            TextReader reader,
            TextWriter writer,
            IUserService userService,
            BackOfficeUserManager<BackOfficeIdentityUser> userManager
        ) : base(reader, writer)
        {
            this.userService = userService;
            this.userManager = userManager;
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

                case "change-name":
                    await ChangeName(args.Skip(1).ToArray());
                    return DeliverableResponse.Continue;

                case "change-loginname":
                    await ChangeLoginName(args.Skip(1).ToArray());
                    return DeliverableResponse.Continue;

                case "create-user":
                    return await CreateUser(args.Skip(1).ToArray());

                default:
                    await Out.WriteLineFormattedAsync("The user operation '{0}' is not supported", operation);
                    return DeliverableResponse.Continue;
            }
        }

        private async Task<DeliverableResponse> CreateUser(string[] args)
        {
            if (args.Length != 5)
            {
                await Out.WriteLineAsync("Please provide 5 arguments, name, username, email, password and groups. For more information see `help`");
                return DeliverableResponse.Continue;
            }

            var name = args[0];
            var username = args[1];
            var email = args[2];
            var password = args[3];
            var groupNames = args[4];

            var identity = BackOfficeIdentityUser.CreateNew(username, email, GlobalSettings.DefaultUILanguage);
            identity.Name = name;

            var result = await userManager.CreateAsync(identity);

            if (!result.Succeeded)
            {
                await Out.WriteLineAsync("Error saving the user:");
                foreach (var error in result.Errors)
                    await Out.WriteLineAsync($"\t{error}");

                return DeliverableResponse.FinishedWithError;
            }

            result = await userManager.AddPasswordAsync(identity.Id, password);
            if (!result.Succeeded)
            {
                await Out.WriteLineAsync("Error saving the user password:");
                foreach (var error in result.Errors)
                    await Out.WriteLineAsync($"\t{error}");

                return DeliverableResponse.FinishedWithError;
            }

            var user = userService.GetByEmail(email);
            var groups = userService.GetUserGroupsByAlias(groupNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            foreach (var group in groups)
            {
                var rg = new ReadOnlyUserGroup(group.Id, group.Name, group.Icon, group.StartContentId, group.StartMediaId, group.Alias, group.AllowedSections, group.Permissions);
                user.AddGroup(rg);
            }
            user.IsApproved = true;
            userService.Save(user);

            return DeliverableResponse.Continue;
        }

        private async Task ChangePassword(string[] args)
        {
            if (args.Length != 2)
            {
                await Out.WriteLineAsync("The expected parameters for 'change-password' were not supplied.");
                await Out.WriteLineAsync("Format expected: change-password <username> <new password>");
                return;
            }

            var username = args[0];
            var user = userService.GetByUsername(username);

            if (user == null)
            {
                await Out.WriteLineFormattedAsync("User '{0}' does not exist in the system", username);
                return;
            }

            try
            {
                userService.SavePassword(user, args[1]);
                await Out.WriteLineFormattedAsync("User '{0}' has had their password updated", username);
            }
            catch (NotSupportedException ex)
            {
                LogHelper.Error<UserDeliverable>("Wasn't able to update the user password from Chauffeur", ex);

                await Out.WriteLineAsync("Updating the user password is not supported.");
                await Out.WriteLineAsync("It's most likely because your UsersMembershipProvider has 'allowManuallyChangingPassword=\"false\"'.");
                await Out.WriteLineAsync("Currently Chauffeur can't update passwords for membership providers configured like this.");
            }
        }

        private async Task ChangeName(string[] args)
        {
            if (args.Length != 2)
            {
                await Out.WriteLineAsync("The expected parameters for 'change-name' were not supplied.");
                await Out.WriteLineAsync("Format expected: change-name <username> <new username>");
                return;
            }

            var username = args[0];
            var user = userService.GetByUsername(username);

            if (user == null)
            {
                await Out.WriteLineFormattedAsync("User '{0}' does not exist in the system", username);
                return;
            }
            user.Name = args[1];
            userService.Save(user);
            await Out.WriteLineFormattedAsync("User '{0}' has had their name updated", username);
        }

        private async Task ChangeLoginName(string[] args)
        {
            if (args.Length != 2)
            {
                await Out.WriteLineAsync("The expected parameters for 'change-loginname' were not supplied.");
                await Out.WriteLineAsync("Format expected: change-loginname <username> <new loginname>");
                return;
            }

            var username = args[0];
            var user = userService.GetByUsername(username);

            if (user == null)
            {
                await Out.WriteLineFormattedAsync("User '{0}' does not exist in the system", username);
                return;
            }
            user.Username = args[1];
            userService.Save(user);
            await Out.WriteLineFormattedAsync("User '{0}' has had their login name updated", username);
        }

        public async Task Directions()
        {
            await Out.WriteLineAsync("A series of operations that can be run against an Umbraco User.");
            await Out.WriteLineAsync();

            await Out.WriteLineAsync("change-password <username> <new password>");
            await Out.WriteLineAsync("\tChanges the password for a given user. This will also hash it if hashing is turned on in the web.config");
            await Out.WriteLineAsync("change-name <username> <new username>");
            await Out.WriteLineAsync("\tChanges the user name for a given user.");
            await Out.WriteLineAsync("change-loginname <username> <new loginname>");
            await Out.WriteLineAsync("\tChanges the login name for a given user.");
        }
    }
}
