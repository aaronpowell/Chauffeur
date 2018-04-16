using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using umbraco;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Chauffeur.Deliverables
{
    [DeliverableName("uc")]
    class UserCreateDeliverable : Deliverable
    {
        private readonly IUserService userService;
        private readonly BackOfficeUserManager<BackOfficeIdentityUser> userManager;

        public UserCreateDeliverable(
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
            var identity = BackOfficeIdentityUser.CreateNew("Chauffeur", "chuaffeur@aaron-powell.com", GlobalSettings.DefaultUILanguage);
            identity.Name = "Chauffeur";

            try
            {
                var result = await userManager.CreateAsync(identity);
            }
            catch (Exception)
            {

                throw;
            }

            return DeliverableResponse.Continue;
        }
    }
}
