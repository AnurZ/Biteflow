using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Shared.Options
{
    public sealed class ActivationLinkOptions
    {
        public string BaseUrl { get; set; } = "https://localhost:4200"; //For local dev; change to biteflow.com TODO

        public string Route { get; set; } = "/activate";

        public TimeSpan Lifetime { get; set; } = TimeSpan.FromHours(24);

        public string TokenSecret { get; set; } = string.Empty;
    }
}
