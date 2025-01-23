using Microsoft.AspNetCore.Mvc;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("group-management")]
    [ApiController]
    public class GroupController : Controller
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }
    }
}
