using System;
using System.Collections.Generic;
using TrackIT.Core.DTOs;

namespace TrackIT.API.DTOs.Dashboard
{
    public class DashboardDto
    {
        public DashboardStatsDto Stats { get; set; }
        public List<RecentActivityDto> RecentActivities { get; set; }
    }

   

    
}