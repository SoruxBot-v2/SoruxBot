using SoruxBot.SDK.Model.Message;

namespace SoruxBot.SDK.QQ;

public static class QqExternHelper
{
    public static string GetMemberUin(this MessageContext context)
    {
        return context.UnderProperty["MemberUin"];
    }
    
    public static string GetKickedMemberUin(this MessageContext context)
    {
        return context.UnderProperty["MemberUin"];
    }
}