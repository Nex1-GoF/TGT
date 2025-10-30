using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TGT.Messages
{
    /// <summary>
    /// 표적의 위치(또는 상태)가 갱신되었을 때 전송되는 메시지
    /// </summary>
    public class TargetUpdateMessage : ValueChangedMessage<TargetUpdateData>
    {
        public TargetUpdateMessage(TargetUpdateData value) : base(value) { }
    }

    /// <summary>
    /// TargetUpdateMessage에 담길 데이터 구조
    /// </summary>
    public class TargetUpdateData
    {
        public string TargetId { get; } 
        public double Latitude { get; }
        public double Longitude { get; }
        public double? Altitude { get; }

        public TargetUpdateData(string targetId, double latitude, double longitude, double? altitude = null)
        {
            TargetId = targetId;
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
        }
    }
}
