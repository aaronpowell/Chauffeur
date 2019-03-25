namespace Chauffeur.Components

open Microsoft.AspNet.Identity.Owin
open Umbraco.Core.Composing
open Umbraco.Core.Services
open Umbraco.Core.Security
open Umbraco.Core.Configuration
open Umbraco.Web.Security
open Umbraco.Core.Models.Identity
open Microsoft.Owin.Security.DataProtection

// This module is a hack because `Umbraco.Core` as a namespace imports a type that has an extension method
// of `IRegister.Register<T>`, which causes the `fun`-based registration to register the delegate, not the
// result of the delegate. But we need it open to use `GetInstance<T>`! So we shove that in another type
// and don't break the rest of things
module internal UserManagerCreater =
    open Umbraco.Core
    type ChauffeurDataProtectionProvider() =
        interface IDataProtectionProvider with
            member __.Create(_) =
                { new IDataProtector with
                    member __.Protect(userData) = userData
                    member __.Unprotect(protectedData) = protectedData }

    let maker (f : IFactory) =
        let options = new IdentityFactoryOptions<BackOfficeUserManager>()
        options.Provider <- new IdentityFactoryProvider<BackOfficeUserManager>()
        options.DataProtectionProvider <- new ChauffeurDataProtectionProvider()

        let userManager = BackOfficeUserManager.Create
                            (options,
                                f.GetInstance<IUserService>(),
                                f.GetInstance<IMemberTypeService>(),
                                f.GetInstance<IEntityService>(),
                                f.GetInstance<IExternalLoginService>(),
                                MembershipProviderExtensions.GetUsersMembershipProvider().AsUmbracoMembershipProvider(),
                                f.GetInstance<Configs>().Settings().Content,
                                f.GetInstance<IGlobalSettings>())

        userManager :> BackOfficeUserManager<BackOfficeIdentityUser>

[<RuntimeLevelAttribute(MinLevel = Umbraco.Core.RuntimeLevel.Install)>]
type UserManagerComponent() =
    interface IComposer with
        member __.Compose(composition) =
            composition.Register(fun f -> UserManagerCreater.maker f)