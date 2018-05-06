﻿using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class UserController : BaseController
	{
		private readonly UserTasks _userTasks;
		private readonly AwardTasks _awardTasks;

		public UserController(
			UserTasks userTasks,
			AwardTasks awardTasks)
			: base(userTasks)
		{
			_userTasks = userTasks;
			_awardTasks = awardTasks;
		}

		[RequirePermission(true, PermissionTo.ViewUsers, PermissionTo.EditUsers)]
		public IActionResult List(PagedModel getModel)
		{
			var model = _userTasks.GetPageOfUsers(getModel);
			return View(model);
		}

		[RequirePermission(true, PermissionTo.ViewUsers, PermissionTo.EditUsers)]
		public async Task<IActionResult> Index(int? id)
		{
			if (!id.HasValue)
			{
				return RedirectToAction(nameof(List));
			}

			var model = await _userTasks.GetUserDetails(id.Value);
			return View(model);
		}

		[RequirePermission(PermissionTo.EditUsers)]
		public async Task<IActionResult> Edit(int id)
		{
			if (id > 0)
			{
				var userName = await _userTasks.GetUserNameById(id);

				var model = await _userTasks.GetUserForEdit(userName, User.GetUserId());
				return View(model);
			}

			return RedirectToAction(nameof(List));
		}

		[RequirePermission(PermissionTo.EditUsers)]
		public async Task<IActionResult> EditByName(string userName)
		{
			var model = await _userTasks.GetUserForEdit(userName, User.GetUserId());
			return View(nameof(Edit), model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		[RequirePermission(PermissionTo.EditUsers)]
		public async Task<IActionResult> Edit(UserEditViewModel model)
		{
			if (!ModelState.IsValid)
			{	
				model.AvailableRoles = await _userTasks.GetAllRolesUserCanAssign(User.GetUserId(), model.SelectedRoles);
				return View(model);
			}

			await _userTasks.EditUser(model);
			return RedirectToAction(nameof(List));
		}

		[RequirePermission(PermissionTo.EditUsers)]
		public async Task<IActionResult> Unlock(int id, string returnUrl)
		{
			await _userTasks.UnlockUser(id);
			return RedirectToLocal(returnUrl);
		}

		[RequirePermission(PermissionTo.EditUsersUserName)]
		public async Task<IActionResult> VerifyUserNameIsUnique(string userName)
		{
			var exists = await _userTasks.CheckUserNameExists(userName);
			return Json(exists);
		}

		[RequirePermission(true, PermissionTo.ViewUsers, PermissionTo.EditUsers)]
		public async Task<IActionResult> SearchUserName(string partial)
		{
			if (!string.IsNullOrWhiteSpace(partial) && partial.Length > 1)
			{
				var matches = await _userTasks.GetUsersByPartial(partial);
				return Json(matches);
			}

			return Json(new List<string>());
		}

		[AllowAnonymous]
		public async Task<IActionResult> Profile(int id)
		{
			var model = await _userTasks.GetUserProfile(id);
			if (model == null)
			{
				return NotFound();
			}

			if (!string.IsNullOrWhiteSpace(model.Signature))
			{
				model.Signature = RenderPost(model.Signature, true, false);
			}

			model.Awards = await _awardTasks.GetAllAwardsForUser(id);

			return View(model);
		}
	}
}
