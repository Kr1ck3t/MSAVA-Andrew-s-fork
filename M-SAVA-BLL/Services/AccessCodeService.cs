using M_SAVA_BLL.Loggers;
using M_SAVA_BLL.Services.Interfaces;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using M_SAVA_Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace M_SAVA_BLL.Services
{
    public class AccessCodeService
    {
        private readonly IIdentifiableRepository<AccessCodeDB> _accessCodeRepository;
        private readonly IIdentifiableRepository<AccessGroupDB> _accessGroupRepository;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ServiceLogger _serviceLogger;
        private readonly AccessGroupService _accessGroupService;

        public AccessCodeService(
            IIdentifiableRepository<AccessCodeDB> accessCodeRepo,
            IIdentifiableRepository<AccessGroupDB> accessGroupRepo,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            ServiceLogger serviceLogger,
            AccessGroupService accessGroupService)
        {
            _accessCodeRepository = accessCodeRepo ?? throw new ArgumentNullException(nameof(accessCodeRepo));
            _accessGroupRepository = accessGroupRepo ?? throw new ArgumentNullException(nameof(accessGroupRepo));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
            _accessGroupService = accessGroupService ?? throw new ArgumentNullException(nameof(accessGroupService));
        }

        // Additional methods for AccessCodeService can be implemented here
        public async Task<Guid> CreateNewAccessCode(int maxUses, DateTime expiresAt, Guid accessgroupId, AccessGroupDB accessGroup)
        {
            UserDB user = _userService.GetSessionUserDB();
            AccessCodeDB AccessCode = new AccessCodeDB
            {
                Id = Guid.NewGuid(),
                OwnerId = user.Id,
                Owner = user,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                MaxUses = maxUses,
                AccessGroupId = accessgroupId,
                AccessGroup = accessGroup,
                UsageCount = 0
            };
            _accessCodeRepository.Insert(AccessCode);
            await _accessCodeRepository.CommitAsync();
            return AccessCode.Id;
        }

        public async Task JoinGroupWithAccessCode(Guid accessCodeId)
        {
            // ✅ Get current user
            UserDB user = _userService.GetSessionUserDB();

            // ✅ Find access code
            var accessCode = await _accessCodeRepository.GetByIdAsync(accessCodeId, ac => ac.AccessGroup);
            if (accessCode == null)
                throw new KeyNotFoundException("Access code not found.");

            // ✅ Validate
            if (accessCode.ExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("Access code has expired.");

            if (accessCode.MaxUses > 0 && accessCode.UsageCount >= accessCode.MaxUses)
                throw new InvalidOperationException("Access code usage limit reached.");

            // ✅ Add user to group if not already a member
            if (!user.AccessGroups.Any(ag => ag.Id == accessCode.AccessGroupId))
            {
                user.AccessGroups.Add(accessCode.AccessGroup);
            }

            // ✅ Increment usage
            accessCode.UsageCount++;

            // Save
            await _accessCodeRepository.CommitAsync();
        }

        public async Task AddSubgroupAsync(Guid accessCodeId)
        {
            // 1️⃣ Get access code
            AccessCodeDB accessCode = await _accessCodeRepository.GetByIdAsync(accessCodeId);
            if (accessCode == null)
                throw new KeyNotFoundException($"AccessCode with ID {accessCodeId} not found.");

            // 2️⃣ Get parent group
            AccessGroupDB parentGroup = await _accessGroupRepository.GetByIdAsync(accessCode.AccessGroupId);
            if (parentGroup == null)
                throw new KeyNotFoundException($"Parent AccessGroup with ID {accessCode.AccessGroupId} not found.");

            // 3️⃣ Ensure SubGroups collection is initialized
            if (parentGroup.SubGroups == null)
                parentGroup.SubGroups = new List<AccessGroupDB>();

            // 4️⃣ Add subgroup if not already added
            if (!parentGroup.SubGroups.Any(sg => sg.Id == accessCode.AccessGroup.Id))
            {
                parentGroup.SubGroups.Add(accessCode.AccessGroup);
            }

            // 5️⃣ Commit changes
            _accessGroupRepository.Update(parentGroup);
            await _accessGroupRepository.CommitAsync();
        }

        public async Task AddSubgroupWithAccessCode(Guid parentGroupId, Guid accessCodeId)
        {
            // ✅ Get current user
            UserDB user = _userService.GetSessionUserDB();

            // ✅ Find parent group
            var parentGroup = await _accessGroupRepository.GetByIdAsync(parentGroupId, pg => pg.SubGroups);
            if (parentGroup == null)
                throw new KeyNotFoundException("Parent group not found.");

            // ✅ Find access code
            var accessCode = await _accessCodeRepository.GetByIdAsync(accessCodeId, ac => ac.AccessGroup);
            if (accessCode == null)
                throw new KeyNotFoundException("Access code not found.");

            // ✅ Validate
            if (accessCode.ExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("Access code has expired.");

            if (accessCode.MaxUses > 0 && accessCode.UsageCount >= accessCode.MaxUses)
                throw new InvalidOperationException("Access code usage limit reached.");

            // ✅ Ensure subgroup isn’t already added
            if (!parentGroup.SubGroups.Any(sg => sg.Id == accessCode.AccessGroupId))
            {
                parentGroup.SubGroups.Add(accessCode.AccessGroup);
            }

            // ✅ Increment usage
            accessCode.UsageCount++;

            // Save
            await _accessGroupRepository.CommitAsync();
        }

        public async Task TransferOwnershipWithAccessCode(Guid accessGroupId, Guid accessCodeId)
        {
            // ✅ Get current user
            UserDB user = _userService.GetSessionUserDB();

            // ✅ Find access group
            var group = await _accessGroupRepository.GetByIdAsync(accessGroupId);
            if (group == null)
                throw new KeyNotFoundException("Access group not found.");

            // ✅ Find access code
            var accessCode = await _accessCodeRepository.GetByIdAsync(accessCodeId);
            if (accessCode == null)
                throw new KeyNotFoundException("Access code not found.");

            // ✅ Validate
            if (accessCode.ExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("Access code has expired.");

            if (accessCode.MaxUses > 0 && accessCode.UsageCount >= accessCode.MaxUses)
                throw new InvalidOperationException("Access code usage limit reached.");

            // ✅ Transfer ownership
            group.OwnerId = user.Id;
            group.Owner = user;

            // ✅ Increment usage
            accessCode.UsageCount++;

            // Save
            await _accessGroupRepository.CommitAsync();
        }


    }
}