using M_SAVA_BLL.Loggers;
using M_SAVA_BLL.Services.Interfaces;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.AspNetCore.Http;
using M_SAVA_Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services
{
    public class AccessCodeService
    {
        private readonly IIdentifiableRepository<AccessCodeDB> _accessCodeRepo;
        private readonly IIdentifiableRepository<AccessGroupDB> _accessGroupRepo;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ServiceLogger _logger;

        private readonly AccessGroupService _accessGroupService;

        public AccessCodeService(
            IIdentifiableRepository<AccessCodeDB> accessCodeRepo,
            IIdentifiableRepository<AccessGroupDB> accessGroupRepo,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            AccessGroupService accessGroupService,
            ServiceLogger logger)
        {
            _accessCodeRepo = accessCodeRepo;
            _accessGroupRepo = accessGroupRepo;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _accessGroupService = accessGroupService;
            _logger = logger;
        }

        public async Task<Guid> CreateAccessCodeAsync(Guid groupId, DateTime expiresAt, int maxUses)
        {
            var userId = _userService.GetSessionUserId();
            var user = await _userService.GetUserByIdAsync(userId);
            var group = await _accessGroupRepo.GetByIdAsync(groupId);

            if (user == null) throw new KeyNotFoundException("User not found.");
            if (group == null) throw new KeyNotFoundException("Group not found.");
            if (group.OwnerId != user.Id && !user.IsAdmin) 
                throw new UnauthorizedAccessException("Cannot create codes for this group.");

            var userDto = await _userService.GetUserByIdAsync(userId);

            var code = new AccessCodeDB
            {
                Id = Guid.NewGuid(),
                OwnerId = userDto.Id,  
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                MaxUses = maxUses,
                AccessGroupId = group.Id,
                AccessGroup = group
            };

            _accessCodeRepo.Insert(code);
            await _accessCodeRepo.CommitAsync();

            _logger.LogInformation($"User {userDto.Username} created access code {code.Id} for group {group.Name}");


            return code.Id;
        }

        public async Task RedeemAccessCodeAsync(Guid codeId)
        {
            var code = await _accessCodeRepo.GetByIdAsync(codeId);
            if (code == null)
                throw new KeyNotFoundException("Access code not found.");

            if (code.ExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("Access code expired.");

            if (code.MaxUses <= 0)
                throw new InvalidOperationException("Access code already used maximum times.");

            var userDb = _userService.GetSessionUserDB();
            if (userDb == null)
                throw new KeyNotFoundException("Session user not found.");

            await _accessGroupService.AddAccessGroupToUserAsync(code.AccessGroupId, userDb.Id);

            code.MaxUses--;
            _accessCodeRepo.Update(code);
            await _accessCodeRepo.CommitAsync();

            _logger.LogInformation($"User {userDb.Username} redeemed code {code.Id} and joined group {code.AccessGroupId}.");
        }

        public async Task TransferAccessGroupOwnershipAsync(Guid groupId, Guid newOwnerId)
        {
            var group = await _accessGroupRepo.GetByIdAsync(groupId);
            if (group == null) throw new KeyNotFoundException("Access group not found.");

            var newOwner = await _userService.GetUserByIdAsync(newOwnerId);
            if (newOwner == null) throw new KeyNotFoundException("New owner not found.");

            if (!IsSessionUserAdminOrOwnerOfGroup(group))
                throw new UnauthorizedAccessException("Only the current owner or admin can transfer ownership.");

            group.OwnerId = newOwner.Id;
            

            _accessGroupRepo.Update(group);
            await _accessGroupRepo.CommitAsync();
        }

        private bool IsSessionUserAdminOrOwnerOfGroup(AccessGroupDB group)
        {
            var sessionUserId = _userService.GetSessionUserId();
            var sessionUserIsAdmin = _userService.IsSessionUserAdmin();
            return sessionUserIsAdmin || group.OwnerId == sessionUserId;
        }
    }
}
