using AutoMapper;
using SE.Data.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Service.Services
{
    public interface INotificationService
    {

    }

    public class NotificationService : INotificationService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public NotificationService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
    }
}
