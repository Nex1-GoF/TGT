using CommunityToolkit.Mvvm.Messaging.Messages;
using GMap.NET;
using System.Collections.Generic;
using TGT.Models;

namespace TGT.Messages
{
    /// <summary>
    /// 표적의 위치(또는 상태)가 갱신되었을 때 전송되는 메시지
    /// </summary>
    public class TargetAddMessage : ValueChangedMessage<TargetAddData>
    {
        public TargetAddMessage(TargetAddData value) : base(value) { }

    }

    /// <summary>
    /// TargetUpdateMessage에 담길 데이터 구조
    /// </summary>
    public class TargetAddData
    {

        public Target Target { get; }


        public TargetAddData(Target target)
        {
            Target = target;
        }
    }
}
