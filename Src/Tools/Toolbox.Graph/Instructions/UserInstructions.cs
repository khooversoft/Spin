//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//internal static class UserInstructions
//{
//    public static Task<Option> Process(GiUser giUser, GraphTrxContext pContext)
//    {
//        pContext.Context.Location().LogDebug("Process giUser={giUser}, command={command}", giUser, giUser.Command);

//        var result = giUser.Command switch
//        {
//            UserCommand.Add => Add(giUser, pContext),
//            UserCommand.Update => Update(giUser, pContext),
//            UserCommand.Delete => Delete(giUser, pContext),
//            _ => throw new InvalidOperationException($"Invalid command type, command={giUser.Command}"),
//        };

//        pContext.AddQueryResult(result);

//        result.LogStatus(pContext.Context, "Completed processing of giUser={giUser}", [giUser]);
//        return result;
//    }

//    private static Option Add(GiUser giUser, GraphTrxContext pContext)
//    {
//        giUser.NotNull();
//        pContext.NotNull();

//        pContext.Context.LogDebug("Adding giUser={giUser}", giUser);

//        var pi = new PrincipalIdentity(
//            giUser.PrincipalId,
//            giUser.NameIdentifier.NotEmpty(),
//            giUser.UserName.NotEmpty(),
//            giUser.Email.NotEmpty(),
//            giUser.EmailConfirmed ?? false
//            );

//        pContext.GetMap().GrantControl.Principals.AddUser(pi, pContext);

//        pContext.Context.LogDebug("Added giUser={giUser}", giUser);
//        return StatusCode.OK;
//    }

//    private static Option Update(GiUser giUser, GraphTrxContext pContext)
//    {
//        giUser.NotNull();
//        pContext.NotNull();

//        pContext.Context.LogDebug("Updating giUser={giUser}", giUser);

//        var pi = new PrincipalIdentity(
//            giUser.PrincipalId,
//            giUser.NameIdentifier,
//            giUser.UserName,
//            giUser.Email,
//            giUser.EmailConfirmed ?? false
//            );

//        pContext.GetMap().GrantControl.Principals.UpdateUser(pi, pContext);

//        pContext.Context.LogDebug("Added giUser={giUser}", giUser);
//        return StatusCode.OK;
//    }
//}
