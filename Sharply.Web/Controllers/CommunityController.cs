using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using Sharply.Web.ViewModels;
using System.Security.Claims;

namespace Sharply.Web.Controllers
{
    [Authorize]
    public class CommunityController : Controller
    {
        private readonly IGroupService _groupService;
        private readonly ILeaderboardService _leaderboardService;

        public CommunityController(IGroupService groupService, ILeaderboardService leaderboardService)
        {
            _groupService = groupService;
            _leaderboardService = leaderboardService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Community";
            var group = await _groupService.GetGroupForUserAsync(CurrentUserId);

            var model = new CommunityViewModel
            {
                HasGroup = group is not null,
                ErrorMessage = TempData["CommunityError"] as string,
                ErrorSource = TempData["CommunityErrorSource"] as string
            };

            if (group is not null)
            {
                model.IsOwner = group.OwnerUserId == CurrentUserId;
                model.GroupName = group.Name;
                model.InviteCode = group.InviteCode;
                model.CreatedAt = group.CreatedAt;
                model.MemberCount = (await _groupService.GetMembersAsync(group.Id)).Count();

                var weekly = await _leaderboardService.GetWeeklyLeaderboardAsync(group.Id);
                var allTime = await _leaderboardService.GetAllTimeLeaderboardAsync(group.Id);
                var groupSkills = await _groupService.GetGroupSkillsAsync(group.Id);

                model.WeeklyLeaderboard = weekly.Select(ToRow).ToList();
                model.AllTimeLeaderboard = allTime.Select(ToRow).ToList();
                model.GroupSkills = groupSkills.Select(gs => new GroupSkillRowViewModel
                {
                    Id = gs.Id,
                    Name = gs.Name,
                    Level = gs.Level.ToString(),
                    Priority = gs.Priority.ToString()
                }).ToList();
            }

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string groupName)
        {
            if (!string.IsNullOrWhiteSpace(groupName))
            {
                try
                {
                    await _groupService.CreateGroupAsync(CurrentUserId, groupName);
                }
                catch (InvalidOperationException ex)
                {
                    TempData["CommunityError"] = ex.Message;
                    TempData["CommunityErrorSource"] = "create";
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(string inviteCode)
        {
            if (!string.IsNullOrWhiteSpace(inviteCode))
            {
                var joined = await _groupService.JoinGroupAsync(CurrentUserId, inviteCode);
                if (!joined)
                {
                    TempData["CommunityError"] = "That code is invalid, or you already belong to a group.";
                    TempData["CommunityErrorSource"] = "join";
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave()
        {
            await _groupService.LeaveGroupAsync(CurrentUserId);
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGroupSkill(string name, string level, string priority)
        {
            var group = await _groupService.GetGroupForUserAsync(CurrentUserId);

            if (group is not null && group.OwnerUserId == CurrentUserId && !string.IsNullOrWhiteSpace(name))
            {
                var parsedLevel = Enum.TryParse<Level>(level, out var l) ? l : Level.Intermediate;
                var parsedPriority = Enum.TryParse<SkillPriority>(priority, out var p) ? p : SkillPriority.Medium;
                await _groupService.AddGroupSkillAsync(group.Id, name, parsedLevel, parsedPriority);
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGroupSkill(int groupSkillId, string name, string level, string priority)
        {
            var group = await _groupService.GetGroupForUserAsync(CurrentUserId);

            if (group is not null && group.OwnerUserId == CurrentUserId && !string.IsNullOrWhiteSpace(name))
            {
                var parsedLevel = Enum.TryParse<Level>(level, out var l) ? l : Level.Intermediate;
                var parsedPriority = Enum.TryParse<SkillPriority>(priority, out var p) ? p : SkillPriority.Medium;
                await _groupService.UpdateGroupSkillAsync(group.Id, groupSkillId, name, parsedLevel, parsedPriority);
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroupSkill(int groupSkillId)
        {
            var group = await _groupService.GetGroupForUserAsync(CurrentUserId);

            if (group is not null && group.OwnerUserId == CurrentUserId)
            {
                await _groupService.DeleteGroupSkillAsync(group.Id, groupSkillId);
            }

            return RedirectToAction("Index");
        }

        private static LeaderboardRowViewModel ToRow(LeaderboardEntry entry) => new()
        {
            Name = entry.Name,
            Xp = entry.Xp,
            Streak = entry.Streak,
            MissionsCompleted = entry.MissionsCompleted
        };
    }
}
