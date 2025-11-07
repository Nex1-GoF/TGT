using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TGT.Models;
using TGT.Services;

namespace TGT.Network
{
    public class ReceiveExit
    {
        private readonly TargetService _service;
        private readonly UdpClient _udp;
        private bool _running = true;
        private readonly int _listenPort;

        public ReceiveExit(int port = 50000)
        {
            _listenPort = port;
            _service = TargetService.Instance;
            _udp = new UdpClient(_listenPort);
        }

        public void Start()
        {
            _running = true;
            //Console.WriteLine($"[TargetReceiver] Listening (event-based) on port {_listenPort}");
            BeginReceiveLoop();
        }

        public void Stop()
        {
            _running = false;
            _udp.Close();
        }
        private Target Parse(byte[] data)
        {
            Target target = null;
            /*
                작성해주세요


            */
            return target;
        }

        private async void BeginReceiveLoop()
        {
            while (_running)
            {
                try
                {
                    // 데이터가 수신될 때까지 비동기 대기 (polling 아님)
                    UdpReceiveResult result = await _udp.ReceiveAsync();

                    if (!_running)
                        break;

                    byte[] data = result.Buffer;

                    // 변환 함수 호출
                    Target ReceiveTarget = Parse(data);
                    if (ReceiveTarget != null)
                    {
                        _service.RemoveTarget(ReceiveTarget);
                    }
                }
                catch (ObjectDisposedException)
                {
                    // 소켓이 닫힐 때 발생하는 정상 종료 예외
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TargetReceiver] Error: {ex.Message}");
                    await Task.Delay(50);
                }
            }
        }
    }
}
