using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TldcFare.Dal;

namespace TldcFare.WebApi.IService
{
    public interface IExceptionLogService
    {
        bool ExLogIns(Logofexception entry);
    }
}
