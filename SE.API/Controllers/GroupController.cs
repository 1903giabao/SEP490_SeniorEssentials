﻿using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using SE.Common.Request;
using SE.Common.Request.SE.Common.Request;
using SE.Service.Services;
using System.Threading.Tasks;

namespace SE.API.Controllers
{
    [Route("groups")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            var result = await _groupService.CreateGroup(request);
            return Ok(result);
        }        
        
        [HttpPut("add-member")]
        public async Task<IActionResult> AddMemberToGroup([FromBody] AddMemberToGroupRequest req)
        {
            var result = await _groupService.AddMemberToGroup(req);
            return Ok(result);
        }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetGroupsByAccountId(int accountId)
        {
            var result = await _groupService.GetGroupsByAccountId(accountId);
            return Ok(result);
        }

        [HttpDelete("{groupId}/members/{kickerId}/{accountId}")]
        public async Task<IActionResult> RemoveMemberFromGroup(int kickerId, int groupId, int accountId)
        {
            var result = await _groupService.RemoveMemberFromGroup(kickerId, groupId, accountId);
            return Ok(result);
        }

        [HttpDelete("{groupId}")]
        public async Task<IActionResult> RemoveGroup(int groupId)
        {
            var result = await _groupService.RemoveGroup(groupId);
            return Ok(result);
        }

        [HttpGet("{groupId}/members")]
        public async Task<IActionResult> GetMembersByGroupId(int groupId)
        {
            var result = await _groupService.GetMembersByGroupId(groupId);
            return Ok(result);
        }        
        
        [HttpGet("members-in-group/{userId}")]
        public async Task<IActionResult> GetAllGroupMembersByUserId(int userId)
        {
            var result = await _groupService.GetAllGroupMembersByUserId(userId);
            return Ok(result);
        }        
        
        [HttpGet("members-to-add/{groupChatId}")]
        public async Task<IActionResult> GetMembersNotInGroupChat(string groupChatId)
        {
            var result = await _groupService.GetMembersNotInGroupChat(groupChatId);
            return Ok(result);
        }
    }
}