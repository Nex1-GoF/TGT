using CommunityToolkit.Mvvm.Messaging.Messages;
using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGT.Messages
{
    public class MapClickMessage : ValueChangedMessage<MapClickData>
    {
        public MapClickMessage(MapClickData value) : base(value) { }
    }

    public class MapClickData // Changed from 'internal' to 'public' to fix CS0051
    {
        public PointLatLng LatLng { get; set; }
        public bool IsFirst { get; set; }

        public MapClickData(PointLatLng latLng, bool isFirst) // Added 'public' to the constructor to fix IDE0290
        {
            LatLng = latLng;
            IsFirst = isFirst;
        }
    }
}