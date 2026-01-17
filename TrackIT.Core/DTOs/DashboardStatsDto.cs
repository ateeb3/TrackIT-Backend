using System;
using System.Collections.Generic;
using System.Text;

namespace TrackIT.Core.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalAssets { get; set; }
        public int TotalAssetsTrend { get; set; } // e.g. +12%
        public int InUse { get; set; }
        public int UtilizationRate { get; set; } // %
        public int Available { get; set; }
        public int Maintenance { get; set; }
    }
}
