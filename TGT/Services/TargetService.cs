using CommunityToolkit.Mvvm.Messaging;
using GMap.NET;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;
using TGT.Messages;
using TGT.Models;
using TGT.ViewModels;

namespace TGT.Services
{
    public class TgtInfoPakcet
    {
        public const int TGT_INFO_PACKET_SIZE = 37;

        public string SrcId { get; set; }              // 송신자 아이디 (4 chars)
        public string DesId { get; set; }              // 수신자 아이디 (4 chars)
        public UInt32 Seq { get; set; }                // 시퀀스 번호 (4 bytes)
        public byte MsgSize { get; set; }              // 메시지 본문 크기 (1 byte)
        public char DetectedId { get; set; }         // 탐지 아이디 (1 chars)
        public Int32 Latitude { get; set; }            // 위도 (4 bytes, ×1e7)
        public Int32 Longtitude { get; set; }           // 경도 (4 bytes, ×1e7)
        public Int16 Altitude { get; set; }            // 고도 (2 bytes)
        public Int16 Yaw { get; set; }                 // 요 (2 bytes)
        public UInt64 DetectedTime { get; set; }       // 탐지 시간 (8 bytes)
        public UInt16 Speed { get; set; }              // 속도 (2 bytes)
        public char DetectedType { get; set; }         // 탐지체 구분 (1 byte)

        public TgtInfoPakcet(string srcId, string desId, UInt32 seq, byte msgSize,
                                char detectedId, Int32 latitude, Int32 longtitude, Int16 altitude, Int16 yaw,
                                UInt64 detectedTime, UInt16 speed, char detectedType)
        {
            SrcId = srcId;
            DesId = desId;
            Seq = seq;
            MsgSize = msgSize;
            DetectedId = detectedId;
            Latitude = latitude;
            Longtitude = longtitude;
            Altitude = altitude;
            Yaw = yaw;
            DetectedTime = detectedTime;
            Speed = speed;
            DetectedType = detectedType;
        }

        public byte[] Serialize()
        {
            var buffer = new byte[TGT_INFO_PACKET_SIZE];
            var span = buffer.AsSpan();
            int offset = 0;

            Encoding.ASCII.GetBytes(SrcId.PadRight(4)).CopyTo(span.Slice(offset, 4)); offset += 4;
            Encoding.ASCII.GetBytes(DesId.PadRight(4)).CopyTo(span.Slice(offset, 4)); offset += 4;

            BitConverter.TryWriteBytes(span.Slice(offset, 4), Seq); offset += 4;
            span[offset++] = MsgSize;

            span[offset++] = (byte)DetectedId;

            BitConverter.TryWriteBytes(span.Slice(offset, 4), Latitude); offset += 4;
            BitConverter.TryWriteBytes(span.Slice(offset, 4), Longtitude); offset += 4;
            BitConverter.TryWriteBytes(span.Slice(offset, 2), Altitude); offset += 2;
            BitConverter.TryWriteBytes(span.Slice(offset, 2), Yaw); offset += 2;
            BitConverter.TryWriteBytes(span.Slice(offset, 8), DetectedTime); offset += 8;
            BitConverter.TryWriteBytes(span.Slice(offset, 2), Speed); offset += 2;

            span[offset++] = (byte)DetectedType;

            return buffer;
        }

        public TgtInfoPakcet Deserialize(byte[] data)
        {
            if (data.Length != TGT_INFO_PACKET_SIZE) throw new ArgumentException("Invalid packet size");

            var span = data.AsSpan();
            int offset = 0;

            SrcId = Encoding.ASCII.GetString(span.Slice(offset, 4)).Trim(); offset += 4;
            DesId = Encoding.ASCII.GetString(span.Slice(offset, 4)).Trim(); offset += 4;

            Seq = BitConverter.ToUInt32(span.Slice(offset, 4)); offset += 4;
            MsgSize = span[offset++];

            DetectedId = (char)span[offset++];

            Latitude = BitConverter.ToInt32(span.Slice(offset, 4)); offset += 4;
            Longtitude = BitConverter.ToInt32(span.Slice(offset, 4)); offset += 4;
            Altitude = BitConverter.ToInt16(span.Slice(offset, 2)); offset += 2;
            Yaw = BitConverter.ToInt16(span.Slice(offset, 2)); offset += 2;
            DetectedTime = BitConverter.ToUInt64(span.Slice(offset, 8)); offset += 8;
            Speed = BitConverter.ToUInt16(span.Slice(offset, 2)); offset += 2;

            DetectedType = (char)span[offset++];

            return this;

        }
    }

    public class TgtFinPacket
    {
        public const int TGT_FIN_PACKET_SIZE = 14;

        public string SrcId { get; set; }              // 송신자 아이디 (4 chars)
        public string DesId { get; set; }              // 수신자 아이디 (4 chars)
        public UInt32 Seq { get; set; }                // 시퀀스 번호 (4 bytes)
        public byte MsgSize { get; set; }              // 메시지 본문 크기 (1 byte)
        public char DetectedId { get; set; }         // 탐지 아이디 (1 chars)

        public TgtFinPacket(string srcId, string desId, UInt32 seq, byte msgSize, char detectedId)
        {
            SrcId = srcId;
            DesId = desId;
            Seq = seq;
            MsgSize = msgSize;
            DetectedId = detectedId;
        }
    }

        public class    TargetService
    {
        private static TargetService _instance;
        public static TargetService Instance => _instance ??= new TargetService();

        private readonly DispatcherTimer _timer;
        public ObservableCollection<Target> Targets { get; } = new();
        public Target? SelectedTarget;

        // === 소켓 ===
        private static string DestIp = "127.0.0.1";
        private static int DestPort = 7003;

        private Socket txSocket;
        private IPAddress ipAddress;
        private IPEndPoint ep;

        private Socket rxSocket;


        private TargetService()
        {
            txSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipAddress = IPAddress.Parse(DestIp);
            ep = new IPEndPoint(ipAddress, DestPort);

            rxSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            rxSocket.Bind(new IPEndPoint(IPAddress.Any, 6004));
            Task.Run(StartReceivingAsync);

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) }; // 100Hz
            _timer.Tick += UpdateTargets;
            _timer.Start();
        }

        private async Task SendtoC2Async(Target target)
        {
            TgtInfoPakcet packet = new TgtInfoPakcet("T001", "C001", 0, TgtInfoPakcet.TGT_INFO_PACKET_SIZE,
                                                    target.Id, (Int32)(target.CurLoc.Lat * 1e7), (Int32)(target.CurLoc.Lon * 1e7),
                                                    (Int16)target.Altitude, (Int16)target.Yaw,
                                                    (UInt64)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                                    (UInt16)target.Speed, target.DetectedType);
            var buffer = packet.Serialize();
            await txSocket.SendToAsync(new ArraySegment<byte>(buffer), SocketFlags.None, ep);
        }

        private async Task StartReceivingAsync()
        {
            var buffer = new byte[TgtFinPacket.TGT_FIN_PACKET_SIZE];
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                var result = await rxSocket.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, remoteEP);

                // 격추 알려주는 로직
            }
        }

        private async void UpdateTargets(object? sender, EventArgs e)
        {
            const double EarthMetersPerDegree = 111_000.0; // 위도/경도 변환용 근사값
            const double deltaTime = 0.01; // 100ms (Timer 주기)


            var updatedTargets = new List<TargetUpdateData>(); // ✅ 한 번에 보낼 데이터 모음

            foreach (var t in Targets)
            {
                if (!t.IsMoving)
                    continue;

                // yaw는 degree*100 단위니까, 라디안으로 변환
                double yawRad = (t.Yaw / 100.0) * Math.PI / 180.0;

                // 진행 방향 벡터 계산 (yaw 기준)
                double dx = Math.Cos(yawRad);
                double dy = Math.Sin(yawRad);

                // 이동 거리 (m/s * Δt) → degree 단위로 환산
                double step = (t.Speed * deltaTime) / EarthMetersPerDegree;

                // 새로운 좌표 계산
                var newLat = t.CurLoc.Lat + dx * step;
                var newLon = t.CurLoc.Lon + dy * step;

                // 업데이트
                t.CurLoc = (newLat, newLon);

                // 탐지 여부 확인
                if (t.IsDetected)
                {
                    // TODO: 통신 보내기 (여기에 로직)
                    //SendtoC2(t);
                    await SendtoC2Async(t);
                }
                else
                {
                    // 거리체크
                    double dLat = (t.CurLoc.Lat - MapService.Instance.Center.Lat) * 111.0;
                    double dLon = (t.CurLoc.Lon - MapService.Instance.Center.Lng) * 88.8;
                    double distanceKm = Math.Sqrt(dLat * dLat + dLon * dLon);

                    bool withinRange = distanceKm <= (MapService.Instance.Distance / 1000);
                    if (withinRange)
                        t.IsDetected = true;
                }

                // (선택) 이동 경로 저장
                t.PathHistory.Add(t.CurLoc);

                // ✅ 리스트에 데이터 추가
                updatedTargets.Add(new TargetUpdateData(
                    targetId: t.Id.ToString(),
                    from: new PointLatLng(t.CurLoc.Lat, t.CurLoc.Lon),
                    to: new PointLatLng(newLat, newLon),
                    altitude: t.Altitude,
                    pathPoints: null // 필요 시 t.PathHistory 변환 가능
                ));
            }

            // ✅ 루프 바깥에서 단 한 번만 메시지 전송
            if (updatedTargets.Count > 0)
            {
                WeakReferenceMessenger.Default.Send(new TargetBatchUpdateMessage(updatedTargets));
            }
        }



        // 표적 추가 메서드
        public void AddTarget(Target target)
        {
            Targets.Add(target); // 모델 업데이트
            WeakReferenceMessenger.Default.Send(new TargetAddMessage(new TargetAddData(target))); // Map Viewmodel로 전달 
        }

        public void StartTarget(char id)
        {
            var target = FindTarget(id);
            if (target != null && !target.IsMoving)
            {
                target.IsMoving = true;
                target.PathHistory.Clear();
                target.PathHistory.Add(target.CurLoc);
            }
        }

        private Target? FindTarget(char id)
        {
            return Targets.FirstOrDefault(t => t.Id == id);
        }

        public void RemoveTarget(Target target)
        {
            Targets.Remove(target);
            WeakReferenceMessenger.Default.Send(new TargetRemoveMessage(new TargetRemoveData(target.Id.ToString())));
        }

        public void StartAll()
        {
            foreach (var t in Targets)
                t.IsMoving = true;
        }
        public void SelectTarget(Target selected)
        {
            if (selected.IsMoving == false) return;
            if (SelectedTarget != null && selected.Id == SelectedTarget.Id) 
            {
                SelectedTarget = null;
                WeakReferenceMessenger.Default.Send(new TargetSelectMessage(new TargetSelectData("NULL")));
                return;
            }
                
            SelectedTarget = selected;
            WeakReferenceMessenger.Default.Send(new TargetSelectMessage(new TargetSelectData(selected.Id.ToString())));
        }

    }
}
