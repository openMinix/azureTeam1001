using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramaApp1.ViewModels
{
    class MapTester
    {
        public static GeoCoordinate[] CreateRandomGeoCoordinates()
        {
            GeoCoordinate[] coords = new GeoCoordinate[3];
            double[] xCoords = new double[] {60.0,10.0,90.0};
            double[] yCoords = new double[] { 90, 45, 72 };

            for(int i = 0;i<3;i++)
            {
                coords[i] = new GeoCoordinate(xCoords[i], yCoords[i]);
            }
            return coords;
        } 
    }
}
