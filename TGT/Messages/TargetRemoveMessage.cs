using CommunityToolkit.Mvvm.Messaging.Messages;
using GMap.NET;
using System.Collections.Generic;

namespace TGT.Messages
{
    /// <summary>
    /// 표적의 위치(또는 상태)가 갱신되었을 때 전송되는 메시지
    /// </summary>
    public class TargetRemoveMessage : ValueChangedMessage<TargetRemoveData>
    {
        public TargetRemoveMessage(TargetRemoveData value) : base(value) { }
    }

    /// <summary>
    /// TargetUpdateMessage에 담길 데이터 구조
    /// </summary>
    public class TargetRemoveData
    {

        public string TargetId { get; }


        public TargetRemoveData(string targetId)
        {
            TargetId = targetId;
        }
    }
}
