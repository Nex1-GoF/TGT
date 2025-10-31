using CommunityToolkit.Mvvm.Messaging.Messages;
using GMap.NET;
using System.Collections.Generic;

namespace TGT.Messages
{
    /// <summary>
    /// 표적의 위치(또는 상태)가 갱신되었을 때 전송되는 메시지
    /// </summary>
    public class TargetBatchUpdateMessage : ValueChangedMessage<List<TargetUpdateData>>
    {
        public TargetBatchUpdateMessage(List<TargetUpdateData> value) : base(value) { }
    }

    /// <summary>
    /// TargetUpdateMessage에 담길 데이터 구조
    /// </summary>
    public class TargetUpdateData
    {
        /// <summary> 이전 위치 (직선 폴리곤 시작점) </summary>
        public PointLatLng From { get; }

        /// <summary> 현재 위치 (직선 폴리곤 끝점) </summary>
        public PointLatLng To { get; }

        /// <summary> 표적 ID </summary>
        public string TargetId { get; }

        /// <summary> 현재 고도 (없을 수 있음) </summary>
        public double? Altitude { get; }

        /// <summary> 선택적으로 전체 경로 (누적 데이터) </summary>
        public List<PointLatLng>? PathPoints { get; }

        public TargetUpdateData(string targetId, PointLatLng from, PointLatLng to, double? altitude = null, List<PointLatLng>? pathPoints = null)
        {
            TargetId = targetId;
            From = from;
            To = to;
            Altitude = altitude;
            PathPoints = pathPoints;
        }
    }
}
