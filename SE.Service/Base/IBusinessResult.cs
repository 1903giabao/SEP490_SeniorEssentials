using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Service.Base
{
    public interface IBusinessResult
    {
        int Status { get; set; }
        string? Message { get; set; }
        object? Data { get; set; }
    }
}
