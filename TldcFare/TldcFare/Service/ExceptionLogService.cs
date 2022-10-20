using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TldcFare.Dal;
using TldcFare.WebApi.IService;
using TldcFare.Dal.Repository;

namespace TldcFare.WebApi.Service
{
    public class ExceptionLogService : IExceptionLogService
    {
        IRepository<Logofexception> _exLogRepository { get; }

        public ExceptionLogService(IRepository<Logofexception> exLogRepository)
        {
            _exLogRepository = exLogRepository;
        }


        public bool ExLogIns(Logofexception entry)
        {
            try
            {
                entry.LogId = Guid.NewGuid().ToString();
                return _exLogRepository.Create(entry);
            }
            catch
            {
                return false;
            }
        }
    }
}
