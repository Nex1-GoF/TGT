using CommunityToolkit.Mvvm.Messaging;
using GMap.NET;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using TGT.Messages;
using TGT.Models;
using TGT.ViewModels;

namespace TGT.Services
{
    public class TgtInfoPakcet
    {
        public string SrcId { get; set; }              // 송신자 아이디 (4 chars)
        public string DesId { get; set; }              // 수신자 아이디 (4 chars)
        public UInt32 Seq { get; set; }                // 시퀀스 번호 (4 bytes)
        public byte MsgSize { get; set; }              // 메시지 본문 크기 (1 byte)
        public string DetectedId { get; set; }         // 탐지 아이디 (4 chars)
        public Int32 X { get; set; }            // 위도 (4 bytes, ×1e7)
        public Int32 Y { get; set; }           // 경도 (4 bytes, ×1e7)
        public Int32 Z { get; set; }            // 고도 (2 bytes)
        public Int16 Yaw { get; set; }                 // 요 (2 bytes)
        public UInt64 DetectedTime { get; set; }       // 탐지 시간 (8 bytes)
        public UInt16 Speed { get; set; }              // 속도 (2 bytes)
        public byte DetectedType { get; set; }         // 탐지체 구분 (1 byte)

        public TgtInfoPakcet(string srcId, string desId, UInt32 seq, byte msgSize,
                                string detectedId, Int32 x, Int32 y, Int32 z, Int16 yaw,
                                UInt64 detectedTime,UInt16 speed, byte detectedType)
        {
            SrcId = srcId;
            DesId = desId;
            Seq = seq;
            MsgSize = msgSize;
            DetectedId = detectedId;
            X = x;
            Y = y;
            Z = z;
            Yaw = yaw;
            DetectedTime = detectedTime;
            Speed = speed;
            DetectedType = detectedType;
        }

        public byte[] Serialize()
        {
            var buffer = new byte[42];
            var span = buffer.AsSpan();
            int offset = 0;

            Encoding.ASCII.GetBytes(SrcId.PadRight(4)).CopyTo(span.Slice(offset, 4)); offset += 4;
            Encoding.ASCII.GetBytes(DesId.PadRight(4)).CopyTo(span.Slice(offset, 4)); offset += 4;

            BitConverter.TryWriteBytes(span.Slice(offset, 4), Seq); offset += 4;
            span[offset++] = MsgSize;

            Encoding.ASCII.GetBytes(DetectedId.PadRight(4)).CopyTo(span.Slice(offset, 4)); offset += 4;
            BitConverter.TryWriteBytes(span.Slice(offset, 4), X); offset += 4;
            BitConverter.TryWriteBytes(span.Slice(offset, 4), Y); offset += 4;
            BitConverter.TryWriteBytes(span.Slice(offset, 4), Z); offset += 4;
            BitConverter.TryWriteBytes(span.Slice(offset, 2), Yaw); offset += 2;
            BitConverter.TryWriteBytes(span.Slice(offset, 8), DetectedTime); offset += 8;
            BitConverter.TryWriteBytes(span.Slice(offset, 2), Speed); offset += 2;

            span[offset++] = DetectedType;

            return buffer;
        }

        public TgtInfoPakcet Deserialize(byte[] data)
        {
            if (data.Length != 42) throw new ArgumentException("Invalid packet size");

            var span = data.AsSpan();
            int offset = 0;

            SrcId = Encoding.ASCII.GetString(span.Slice(offset, 4)).Trim(); offset += 4;
            DesId = Encoding.ASCII.GetString(span.Slice(offset, 4)).Trim(); offset += 4;

            Seq = BitConverter.ToUInt32(span.Slice(offset, 4)); offset += 4;
            MsgSize = span[offset++];

            DetectedId = Encoding.ASCII.GetString(span.Slice(offset, 4)).Trim(); offset += 4;
            X = BitConverter.ToInt32(span.Slice(offset, 4)); offset += 4;
            Y = BitConverter.ToInt32(span.Slice(offset, 4)); offset += 4;
            Z = BitConverter.ToInt16(span.Slice(offset, 4)); offset += 4;
            Yaw = BitConverter.ToInt16(span.Slice(offset, 2)); offset += 2;
            DetectedTime = BitConverter.ToUInt64(span.Slice(offset, 8)); offset += 8;
            Speed = BitConverter.ToUInt16(span.Slice(offset, 2)); offset += 2;

            DetectedType = span[offset++];

            return this;
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
        private static int Hz = 100;
        private static string DestIp = "127.0.0.1";
        private static int DestPort = 7003;

        private Socket socket;
        private IPAddress ipAddress;
        private IPEndPoint ep;


        private TargetService()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipAddress = IPAddress.Parse(DestIp);
            ep = new IPEndPoint(ipAddress, DestPort);

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) }; // 100Hz
            _timer.Tick += UpdateTargets;
            _timer.Start();
        }

        //private void SendtoC2(Target target)
        //{
        //    /*
        //     (string srcId, string desId, UInt32 seq, byte msgSize,'
        //                        string detectedId, Int32 latitude, Int32 longitude,Int16 altitude,Int16 yaw,'
        //                        UInt64 detectedTime,UInt16 speed, byte detectedType)
        //     */

        //    TgtInfoPakcet packet = new TgtInfoPakcet("T001", "C001", 0, 40,
        //                                            $"E00{target.Id}", (Int32)(target.CurLoc.Lat * 1e7), (Int32)(target.CurLoc.Lon * 1e7), (Int32)target.Altitude,
        //                                            (Int16)target.Yaw,
        //                                            target.DetectTime.HasValue ? (UInt64)(target.DetectTime.Value - DateTime.UnixEpoch).TotalMilliseconds : 0UL,
        //                                            (UInt16)target.Speed, (byte)target.DetectedType);

        //    socket.SendTo(packet.Serialize(), ep);
        //    //juyeon.loc = t.CurLoc;
        //    //juyeon.speed = t.Speed;
        //    //juyeon.yaw = t.Yaw;
        //    //juyeon암호화!
        //    // realsend(juyeon);

        //}

        private async Task SendtoC2Async(Target target)
        {
            TgtInfoPakcet packet = new TgtInfoPakcet("T001", "C001", 0, 40,
                                                    $"E00{target.Id}", (Int32)(target.CurLoc.Lat * 1e7), (Int32)(target.CurLoc.Lon * 1e7), (Int32)target.Altitude,
                                                    (Int16)target.Yaw,
                                                    target.DetectTime.HasValue ? (UInt64)(target.DetectTime.Value - DateTime.UnixEpoch).TotalMilliseconds : 0UL,
                                                    (UInt16)target.Speed, (byte)target.DetectedType);
            var buffer = packet.Serialize();


            await socket.SendToAsync(new ArraySegment<byte>(buffer), SocketFlags.None, ep);
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

        /// <summary>
        /// 시뮬레이션 좌표(x,y) → 위도/경도 변환
        /// </summary>
        private (double lat, double lon) XYToLatLon(double x, double y, double lat0, double lon0)
        {
            const double R = 6_378_137.0;
            double lat0Rad = lat0 * Math.PI / 180.0;

            double newLat = lat0 + (y / R) * (180.0 / Math.PI);
            double newLon = lon0 + (x / (R * Math.Cos(lat0Rad))) * (180.0 / Math.PI);

            return (newLat, newLon);
        }
        private async void UpdateTargets(object? sender, EventArgs e)
        {
            const double dt = 0.01;             // 10ms
            const double R = 6_378_137.0;       // 지구 반경(m)
            var updatedTargets = new List<TargetUpdateData>();

            // 기준점 (서울)
            double lat0 = MapService.Instance.Center.Lat;
            double lon0 = MapService.Instance.Center.Lng;

            foreach (var t in Targets)
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

                double distance = Math.Sqrt(dx * dx + dy * dy);
                //Debug.WriteLine($"Target {t.Id} 이동거리 = {distance:F4} m");

                //--------------------------------------------------
                // ③ (x,y) → 위도/경도 복원
                //--------------------------------------------------
                (double newLat, double newLon) = XYToLatLon(x, y, lat0, lon0);

                //--------------------------------------------------
                // ④ Target 업데이트
                //--------------------------------------------------
                t.SimPosX = x;
                t.SimPosY = y;

                t.CurLoc = (newLat, newLon);
                t.PathHistory.Add(t.CurLoc);

                //--------------------------------------------------
                // ⑤ 탐지 여부 및 전송
                //--------------------------------------------------
                if (t.IsDetected)
                {
                    await SendtoC2Async(t);
                }
                else
                {
                    double dLatM = (newLat - lat0) * 111000.0;
                    double dLonM = (newLon - lon0) * 111000.0 * Math.Cos(lat0 * Math.PI / 180.0);
                    double dist = Math.Sqrt(dLatM * dLatM + dLonM * dLonM);

                    if (dist <= MapService.Instance.Distance)
                        t.IsDetected = true;
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
