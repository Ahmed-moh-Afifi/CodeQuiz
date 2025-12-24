using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Resources
{
    public static class Config
    {
        public static string Server { get; set; } = "http://129.151.234.105";
        public static string API { get; set; } = $"{Server}/api";
        public static string HUB { get; set; } = $"{Server}/hubs";
    }
}
