namespace SoruxBot.SDK.Model.Message
{
    /// <summary>
    /// 事件类型，表示消息的来源状态。
    /// </summary>
    public enum MessageType
    {
        //私聊对话场景，含：临时对话、私聊对话、验证对话和群对话等等
        PrivateMessage = 0,
        //多人对话场景，含：群组对话和频道对话等等
        GroupMessage = 1,
        //通知场景，含：加好友通知、临时通知等等
        Notify = 2,
        //平台特定场景，含：QQ的戳一戳、微信的拍一拍等等
        Future = 3,
    }
}
