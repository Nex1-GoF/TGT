using System;
using System.Diagnostics;
using System.Windows.Media;

namespace TGT
{
    public class FpsCounter
    {
        private Stopwatch _stopwatch = new();
        private int _frameCount = 0;
        private double _lastFps = 0.0;
        private double _accumulatedTime = 0.0;

        public double CurrentFps => _lastFps;

        public FpsCounter()
        {
            _stopwatch.Start();
            CompositionTarget.Rendering += OnRenderFrame;
        }

        private void OnRenderFrame(object? sender, EventArgs e)
        {
            _frameCount++;
            _accumulatedTime += _stopwatch.Elapsed.TotalSeconds;
            _stopwatch.Restart();

            if (_accumulatedTime >= 1.0) // 1초 단위로 FPS 갱신
            {
                _lastFps = _frameCount / _accumulatedTime;
                Debug.WriteLine($"[UI FPS] {Math.Round(_lastFps, 1)} fps");

                _accumulatedTime = 0.0;
                _frameCount = 0;
            }
        }

        public void Stop()
        {
            CompositionTarget.Rendering -= OnRenderFrame;
        }
    }
}
