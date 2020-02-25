using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIOTTrackerSimulator
{
    public class Location
    {
        public double lat { get; set; }
        public double lon { get; set; }

        public Location()
        {

        }
        public Location(double lat, double lon)
        {
            this.lat = lat;
            this.lon = lon;
        }
    }
}
