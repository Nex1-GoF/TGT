using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGT.Services
{
    class MapService
    {
        private static MapService _instance;
        public static MapService Instance => _instance ??= new MapService();

        public double Distance {  get; set; }
        public PointLatLng Seoul { get; set; }
        public PointLatLng Center { get; set; }
        

        private MapService() {

            Distance = 250_000;
            Seoul = new PointLatLng(37.5665, 126.9780);
            Center = new PointLatLng(37.5665, 126.9780);


        }
    }
}
