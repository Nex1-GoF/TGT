using CommunityToolkit.Mvvm.Messaging;
using GMap.NET;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TGT.Messages;
using TGT.Models;
using TGT.ViewModels;
using System.Windows;
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
        public UInt16 Yaw { get; set; }                 // 요 (2 bytes)
        public UInt64 DetectedTime { get; set; }       // 탐지 시간 (8 bytes)
        public UInt16 Speed { get; set; }              // 속도 (2 bytes)
        public char DetectedType { get; set; }         // 탐지체 구분 (1 byte)

        public TgtInfoPakcet(string srcId, string desId, UInt32 seq, byte msgSize,
                                char detectedId, Int32 latitude, Int32 longtitude, Int16 altitude, UInt16 yaw,
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
            Yaw = BitConverter.ToUInt16(span.Slice(offset, 2)); offset += 2;
            DetectedTime = BitConverter.ToUInt64(span.Slice(offset, 8)); offset += 8;
            Speed = BitConverter.ToUInt16(span.Slice(offset, 2)); offset += 2;

            DetectedType = (char)span[offset++];

            return this;

        }
    }

    public class TgtFinPacket
    {
        public const int TGT_FIN_PACKET_SIZE = 15;

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

        public TgtFinPacket()
        {
        }

        public byte[] Serialize()
        {
            var buffer = new byte[TGT_FIN_PACKET_SIZE];
            var span = buffer.AsSpan();
            int offset = 0;

            // 4 bytes
            Encoding.ASCII.GetBytes(SrcId.PadRight(4))
                .CopyTo(span.Slice(offset, 4));
            offset += 4;

            // 4 bytes
            Encoding.ASCII.GetBytes(DesId.PadRight(4))
                .CopyTo(span.Slice(offset, 4));
            offset += 4;

            // 4 bytes
            BitConverter.TryWriteBytes(span.Slice(offset, 4), Seq);
            offset += 4;

            // 1 byte
            span[offset++] = MsgSize;

            // 1 byte
            span[offset++] = (byte)DetectedId;

            return buffer;
        }

        public TgtFinPacket Deserialize(byte[] data)
        {
            //if (data.Length != TGT_FIN_PACKET_SIZE)
            //    throw new ArgumentException("Invalid packet size");

            var span = data.AsSpan();
            int offset = 0;

            SrcId = Encoding.ASCII.GetString(span.Slice(offset, 4)).Trim();
            offset += 4;

            DesId = Encoding.ASCII.GetString(span.Slice(offset, 4)).Trim();
            offset += 4;

            Seq = BitConverter.ToUInt32(span.Slice(offset, 4));
            offset += 4;

            MsgSize = span[offset++];

            DetectedId = (char)span[offset++];

            return this;
        }
    }

    public class TargetService
    {
        private static TargetService _instance;
        public static TargetService Instance => _instance ??= new TargetService();

        private Thread _updateThread;
        private bool _running = true;

        // 기존 그대로
        public ObservableCollection<Target> Targets { get; } = new();
        //public Target? SelectedTarget;

        // === 소켓 ===
        private static string DestIp = "192.168.1.100"; //"127.0.0.1";
        private static int DestPort = 7003;

        private Socket txSocket;
        private IPAddress ipAddress;
        private IPEndPoint ep;

        private Socket rxSocket;
        public Target? SelectedTarget { get; set; } = null;
        private TargetService()
        {
            txSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipAddress = IPAddress.Parse(DestIp);
            ep = new IPEndPoint(ipAddress, DestPort);

            rxSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            rxSocket.Bind(new IPEndPoint(IPAddress.Any, 6004));
            Task.Run(StartReceivingAsync);

            // ✅ 정확히 100Hz 고정 루프 시작
            _updateThread = new Thread(UpdateLoop)
            {
                IsBackground = true
            };
            _updateThread.Start();
        }

        // ✅ 기존 DispatcherTimer → Fixed 100Hz 업데이트로 변경
        private void UpdateLoop()
        {
            const double dt = 0.1;   // 100Hz
            Stopwatch sw = Stopwatch.StartNew();
            double accumulated = 0;

            while (_running)
            {
                double elapsed = sw.Elapsed.TotalSeconds;
                sw.Restart();

                accumulated += elapsed;

                // 누적된 시간이 dt 이상이면 여러 프레임 처리
                while (accumulated >= dt)
                {
                    Debug.WriteLine($"Target {accumulated}");
                    UpdateTargets(dt);   // ✅ 기존 UpdateTargets 로직 유지
                    accumulated -= dt;
                }

                Thread.Sleep(0); // CPU 점유율 방지
            }
        }

        private void UpdateTargets(double dt)
        {
            const double R = 6_378_137.0; // 지구 반경(m)
            var updatedTargets = new List<TargetUpdateData>();

            // 기준점 (서울)
            double lat0 = MapService.Instance.Center.Lat;
            double lon0 = MapService.Instance.Center.Lng;

            foreach (var t in Targets.ToList())
            {
                if (!t.IsMoving)
                    continue;

                double curLat = t.CurLoc.Lat;
                double curLon = t.CurLoc.Lon;

                //--------------------------------------------------
                // ① 위도/경도 → 평면좌표(x, y) 변환
                //--------------------------------------------------
                (double x, double y) = LatLonToXY(curLat, curLon, lat0, lon0);

                //--------------------------------------------------
                // ② 이동 적용 (m 단위)
                //--------------------------------------------------
                double headingRad = (t.Yaw / 100.0) * Math.PI / 180.0;

                double dx = Math.Sin(headingRad) * t.Speed * dt;   // 동
                double dy = Math.Cos(headingRad) * t.Speed * dt;   // 북

                x += dx;
                y += dy;

                //Debug.WriteLine($"Target {t.Id} x = {x} y = {y}");

                //--------------------------------------------------
                // ③ (x,y) → 위도/경도 복원
                //--------------------------------------------------
                (double newLat, double newLon) = XYToLatLon(x, y, lat0, lon0);

                //--------------------------------------------------
                // ④ Target 업데이트
                //--------------------------------------------------
                t.SimPosX = x;
                t.SimPosY = y;
                Debug.WriteLine($"Target {t.Id} x = {x} y = {y}");    
                t.CurLoc = (newLat, newLon);
                t.PathHistory.Add(t.CurLoc);

                //--------------------------------------------------
                // ⑤ 탐지 여부 및 전송
                //--------------------------------------------------
                if (t.IsDetected)
                {
                    SendtoC2Async(t);
                }
                double dLatM = (newLat - lat0) * 111000.0;
                double dLonM = (newLon - lon0) * 111000.0 * Math.Cos(lat0 * Math.PI / 180.0);
                double dist = Math.Sqrt(dLatM * dLatM + dLonM * dLonM);
                if (dist <= MapService.Instance.Distance)
                    t.IsDetected = true;
                else
                {
                    t.IsDetected = false;
                }
       

                //--------------------------------------------------
                // ⑥ 메시지 패킹
                //--------------------------------------------------
                updatedTargets.Add(new TargetUpdateData(
                    targetId: t.Id.ToString(),
                    from: new PointLatLng(curLat, curLon),
                    to: new PointLatLng(newLat, newLon),
                    altitude: t.Altitude,
                    pathPoints: null
                ));
            }

            //--------------------------------------------------
            // ⑦ 일괄 브로드캐스트
            //--------------------------------------------------
            if (updatedTargets.Count > 0)
                WeakReferenceMessenger.Default.Send(new TargetBatchUpdateMessage(updatedTargets));
        }

        private async Task SendtoC2Async(Target target)
        {
            TgtInfoPakcet packet = new TgtInfoPakcet("T001", "C001", 0, TgtInfoPakcet.TGT_INFO_PACKET_SIZE,
                                                    target.Id, (Int32)(target.CurLoc.Lat * 1e7), (Int32)(target.CurLoc.Lon * 1e7),
                                                    (Int16)target.Altitude, (UInt16)target.Yaw,
                                                    (UInt64)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                                    (UInt16)target.Speed, target.DetectedType);
            var buffer = packet.Serialize();
            await txSocket.SendToAsync(new ArraySegment<byte>(buffer), SocketFlags.None, ep);
        }

        private async Task StartReceivingAsync()
        {
            Debug.WriteLine($"소켓 열림");

            var buffer = new byte[TgtFinPacket.TGT_FIN_PACKET_SIZE];
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {

                try
                {
                    var result = await rxSocket.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, remoteEP);
                }
                catch (SocketException ex)
                {
                    Debug.WriteLine($"[SocketException] Code={ex.SocketErrorCode}, Msg={ex.Message}");
                    throw;
                }

                
                TgtFinPacket tgtFin = new TgtFinPacket();
                tgtFin.Deserialize(buffer);

                Debug.WriteLine($"Received TGT_FIN for Target ID: {tgtFin.DetectedId}");
                var target = Targets.FirstOrDefault(t => t.Id==tgtFin.DetectedId);
                if (target==null) return;
                RemoveTarget(target);

                //Target ReceiveTarget = //받은 패킷까서 해당 id 타겟
                //RemoveTarget(ReceiveTarget);
                // 격추 알려주는 로직
            }
        }

        private (double x, double y) LatLonToXY(double lat, double lon, double lat0, double lon0)
        {
            const double R = 6_378_137.0; // 지구 반경
            double lat0Rad = lat0 * Math.PI / 180.0;

            double dLat = (lat - lat0) * Math.PI / 180.0;
            double dLon = (lon - lon0) * Math.PI / 180.0;

            double x = dLon * R * Math.Cos(lat0Rad);  // 동쪽
            double y = dLat * R;                      // 북쪽

            return (x, y);
        }

        private (double lat, double lon) XYToLatLon(double x, double y, double lat0, double lon0)
        {
            const double R = 6_378_137.0;
            double lat0Rad = lat0 * Math.PI / 180.0;

            double newLat = lat0 + (y / R) * (180.0 / Math.PI);
            double newLon = lon0 + (x / (R * Math.Cos(lat0Rad))) * (180.0 / Math.PI);

            return (newLat, newLon);
        }


        // ===================== 기존 메서드 유지 =====================

        public void AddTarget(Target target)
        {
            Targets.Add(target);
            WeakReferenceMessenger.Default.Send(new TargetAddMessage(new TargetAddData(target)));

            var logVM = Application.Current.MainWindow.Resources["LogVM"] as LogViewModel;
            if (logVM != null)
                logVM.IsAnyTarget = Targets.Count > 0;
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
        public class TargetListChangedMessage { }

        public void RemoveTargetWithid(char id)
        {
            Target target = FindTarget(id);
            if (target != null) return;
            Targets.Remove(target);
            WeakReferenceMessenger.Default.Send(new TargetRemoveMessage(new TargetRemoveData(target.Id.ToString())));
            WeakReferenceMessenger.Default.Send(new TargetListChangedMessage());
            var logVM = Application.Current.MainWindow.Resources["LogVM"] as LogViewModel;
            if (logVM != null)
                logVM.IsAnyTarget = Targets.Count > 0;
        }

        public void RemoveTarget(Target target)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Targets.Remove(target);
                    WeakReferenceMessenger.Default.Send(new TargetRemoveMessage(new TargetRemoveData(target.Id.ToString())));

                    WeakReferenceMessenger.Default.Send(new TargetListChangedMessage());
                });
                var logVM = Application.Current.MainWindow.Resources["LogVM"] as LogViewModel;
                if (logVM != null)
                    logVM.IsAnyTarget = Targets.Count > 0;
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private Target? FindTarget(char id)
        {
            return Targets.FirstOrDefault(t => t.Id == id);
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

            // Todo: 여기서 시나리오 모드라면, 작동 안되게끔

            SelectedTarget = selected;
            WeakReferenceMessenger.Default.Send(new TargetSelectMessage(new TargetSelectData(selected.Id.ToString())));
        }

        // ✅ 서비스 종료
        public void Stop()
        {
            _running = false;
        }
    }
}