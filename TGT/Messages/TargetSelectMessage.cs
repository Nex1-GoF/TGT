using CommunityToolkit.Mvvm.Messaging.Messages;
using GMap.NET;
using System.Collections.Generic;

namespace TGT.Messages
{
    /// <summary>
    /// 표적의 위치(또는 상태)가 갱신되었을 때 전송되는 메시지
    /// </summary>
    public class TargetSelectMessage : ValueChangedMessage<TargetSelectData>
    {
        public TargetSelectMessage(TargetSelectData value) : base(value) { }
    }

    /// <summary>
    /// TargetUpdateMessage에 담길 데이터 구조
    /// </summary>
    public class TargetSelectData
    {

        public string TargetId { get; }


        public TargetSelectData(string targetId)
        {
            TargetId = targetId;
        }
    }
}
