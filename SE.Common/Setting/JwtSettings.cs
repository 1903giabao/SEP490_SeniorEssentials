using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Setting
{
    public class JwtSettings
    {
        public string Key { get; set; } = null!;

        public JwtSettings()
        {
            Key = Environment.GetEnvironmentVariable("JwtSettings");
        }
    }

    
}
