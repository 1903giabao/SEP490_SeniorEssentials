using AutoMapper;
using SE.Data.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Service.Services
{
    public interface IGroupService
    {

    }

    public class GroupService : IGroupService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GroupService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
    }
}
