using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerServiceDemo.Models
{
    public record IspInformation(string status, string country, string countryCode, string region, string regionName, string city, string zip, float lat, float lon, string timezone, string isp, string org, string _as, string query);
}

