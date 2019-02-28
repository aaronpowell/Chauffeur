namespace Chauffeur.Deliverables

open Chauffeur
open FSharp.Control.Tasks.V2
open Umbraco.Core.Services
open Umbraco.Core.Configuration
open Umbraco.Web.Security
open Umbraco.Core.Models.Identity
open Umbraco.Core.Logging
open Umbraco.Core.Models.Membership
open System.Threading.Tasks
open System

module UserPasswordChanger =
    let resetPasswordWithToken generateToken (resetPassword : (int * string * string) -> Task<Microsoft.AspNet.Identity.IdentityResult>) userId password =
        task {
            let! token = generateToken userId
            let! result = resetPassword (userId, token, password)

            return result
        }

    let password write (getUser : string -> IUser) (resetPassword : int -> string -> Task<Microsoft.AspNet.Identity.IdentityResult>) args =
        task {
            match args with
            | username :: password :: _ ->
                let user = getUser username
                match user with
                | null ->
                    do! (sprintf "User '%s' does not exist in the system" username) |> write
                | _ ->
                    let! result = resetPassword user.Id password
                    match result.Succeeded with
                    | true ->
                        do! (sprintf "User '%s' has had their password changed" username) |> write
                    | false ->
                        do! write "There were errors changing the password:"
                        for err in result.Errors do
                            do! (sprintf "\t%s" err) |> write
            | _ ->
                do! write("The expected parameters for 'change-password' were not supplied.");
                do! write("Format expected: change-password <username> <new password>");

            return ignore()
        }

module UserDetailsChanger =
    let name writer (getUser : string -> IUser) saveUser args =
        task {
            match args with
            | username :: name :: _ ->
                let user = getUser username
                match user with
                | null ->
                    do! (sprintf "User '%s' does not exist in the system" username) |> writer
                | _ ->
                    user.Name <- name
                    saveUser user
                    do! (sprintf "User '%s' has had their name updated" username) |> writer

            | _ ->
                do! writer "The expected parameters for 'change-name' were not supplied."
                do! writer "Format expected: change-name <username> <new username>"
        }

    let loginName writer (getUser : string -> IUser) saveUser  args =
        task {
            match args with
            | username :: newUsername :: _ ->
                let user = getUser username
                match user with
                | null ->
                    do! (sprintf "User '%s' does not exist in the system" username) |> writer
                | _ ->
                    user.Username <- newUsername
                    saveUser user
                    do! (sprintf "User '%s' has had their name updated" username) |> writer

            | _ ->
                do! writer "The expected parameters for 'change-loginname' were not supplied."
                do! writer "Format expected: change-loginname <username> <new username>"
        }

module UserCreator =
    let createUser (writer : string -> Task) uiLanguage (create : BackOfficeIdentityUser -> Task<Microsoft.AspNet.Identity.IdentityResult>) name username email =
        task {
            let identity = BackOfficeIdentityUser.CreateNew(username, email, uiLanguage)
            identity.Name <- name
            
            let! result = create identity

            match result.Succeeded with
            | false ->
                do! writer "Error saving the user:"
                for err in result.Errors do
                    do! writer(sprintf "\t%s" err)
                return None
            | true ->
                return Some identity
        }

    let addPassword (writer : string -> Task) (addPassword : int * string -> Task<Microsoft.AspNet.Identity.IdentityResult>) password (identity : BackOfficeIdentityUser option) =
        task {
            match identity with
            | None -> return None
                | Some identity-> 

                let! result = addPassword(identity.Id, password)
                match result.Succeeded with
                | false ->
                    do! writer "Error saving the user:"
                    for err in result.Errors do
                        do! writer(sprintf "\t%s" err)
                    return None
                | true ->
                    return Some identity
        }

    let addGroups (getUser : string -> IUser) (groupsByAlias : string array -> IUserGroup seq) saveUser groups (identity : BackOfficeIdentityUser option) =
        match identity with
        | None -> None
        | Some identity-> 
            let user = getUser identity.Email
            let userGroups = groupsByAlias groups
            for g in userGroups do
                let rg = ReadOnlyUserGroup(g.Id, g.Name, g.Icon, g.StartContentId, g.StartMediaId, g.Alias, g.AllowedSections, g.Permissions)
                user.AddGroup rg

            user.IsApproved <- true
            saveUser user
            Some user

    let create writer (createUser : string -> string-> string -> Task<BackOfficeIdentityUser option>) addPassword addGroups args =
        task {
            match args with
            | name :: username :: email :: password :: groupNames :: _ ->
                let! identity = createUser name username email
                let! _ = addPassword password identity
                let user = addGroups (groupNames.Split([| ',' |], StringSplitOptions.RemoveEmptyEntries)) identity

                match user with
                | Some _ ->
                    do! (sprintf "The user '%s' has been created" username) |> writer
                | None ->
                    ignore()

            | _ ->
                do! writer "Please provide 5 arguments, name, username, email, password and groups. For more information see `help`"
        }

[<DeliverableName("user")>]
[<DeliverableAlias("u")>]
type UserDeliverable(
                    reader,
                    writer,
                    userService : IUserService,
                    userManager : BackOfficeUserManager<BackOfficeIdentityUser>,
                    umbracoSettings : IGlobalSettings) =
    inherit Deliverable(reader, writer)

    let changePassword = UserPasswordChanger.password writer.WriteLineAsync
    let changeName = UserDetailsChanger.name writer.WriteLineAsync
    let changeLoginName = UserDetailsChanger.loginName writer.WriteLineAsync

    override __.Run _ args =
        task {
            match args |> Array.toList with
            | [] ->
                do! writer.WriteLineAsync "No operation for the user was provided"
            | head :: tail when head = "change-password" ->
                let passwordResetter = 
                    UserPasswordChanger.resetPasswordWithToken (userManager.GeneratePasswordResetTokenAsync) (userManager.ResetPasswordAsync)

                do! changePassword (userService.GetByUsername) passwordResetter tail
            | head :: tail when head = "change-name" ->
                do! changeName (userService.GetByUsername) (userService.Save) tail
            | head :: tail when head = "change-loginname" ->
                do! changeLoginName (userService.GetByUsername) (userService.Save) tail
            | head :: tail when head = "create-user" ->
                let createUser = UserCreator.createUser (writer.WriteLineAsync) umbracoSettings.DefaultUILanguage (userManager.CreateAsync)
                let addPassword = UserCreator.addPassword (writer.WriteLineAsync) (userManager.AddPasswordAsync)
                let addGroups = UserCreator.addGroups (userService.GetByEmail) (fun g -> userService.GetUserGroupsByAlias g) (userService.Save)

                do! UserCreator.create (writer.WriteLineAsync) createUser addPassword addGroups tail
            | _ ->
                do! writer.WriteLineAsync(sprintf "The user operation '%s' is not supported" args.[0])

            return DeliverableResponse.Continue
        }