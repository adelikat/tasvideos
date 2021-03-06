﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Pages.Roles.Models;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Roles
{
	[RequirePermission(PermissionTo.EditRoles)]
	public class AddEditModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;

		public AddEditModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher)
		{
			_db = db;
			_publisher = publisher;
		}

		[TempData]
		public string? Message { get; set; }

		[TempData]
		public string? MessageType { get; set; }

		[FromRoute]
		public int? Id { get; set; }

		[BindProperty]
		public RoleEditModel Role { get; set; } = new ();

		[Display(Name = "Available Permissions")]
		public IEnumerable<SelectListItem> AvailablePermissions => PermissionsSelectList;

		[Display(Name = "Available Assignable Permissions")]
		public IEnumerable<SelectListItem> AvailableAssignablePermissions { get; set; } = new List<SelectListItem>();

		private static IEnumerable<SelectListItem> PermissionsSelectList =>
			Enum.GetValues(typeof(PermissionTo))
				.Cast<PermissionTo>()
				.Select(p => new SelectListItem
				{
					Value = ((int)p).ToString(),
					Text = p.EnumDisplayName()
				})
				.ToList();

		public async Task OnGet()
		{
			if (Id.HasValue)
			{
				Role = await _db.Roles
					.Where(r => r.Id == Id.Value)
					.Select(r => new RoleEditModel
					{
						Name = r.Name,
						IsDefault = r.IsDefault,
						Description = r.Description,
						AutoAssignPostCount = r.AutoAssignPostCount,
						AutoAssignPublications = r.AutoAssignPublications,
						Links = r.RoleLinks
							.Select(rl => rl.Link)
							.ToList(),
						SelectedPermissions = r.RolePermission
							.Select(rp => (int)rp.PermissionId)
							.ToList(),
						SelectedAssignablePermissions = r.RolePermission
							.Where(rp => rp.CanAssign)
							.Select(rp => (int)rp.PermissionId)
							.ToList()
					})
					.SingleOrDefaultAsync();

				SetAvailableAssignablePermissions();
			}
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				SetAvailableAssignablePermissions();
				return Page();
			}

			Role.Links = Role.Links.Where(l => !string.IsNullOrWhiteSpace(l));
			if (!ModelState.IsValid)
			{
				AvailableAssignablePermissions = Role.SelectedPermissions
				.Select(sp => new SelectListItem
				{
					Text = ((PermissionTo)sp).ToString(),
					Value = sp.ToString()
				});
				return Page();
			}

			await AddUpdateRole(Role);

			try
			{
				MessageType = Styles.Success;
				Message = "Role successfully updated.";
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				MessageType = Styles.Danger;
				Message = $"Unable to update Role {Id}, the role may have already been updated, or the game no longer exists.";
			}

			return RedirectToPage("List");
		}

		public async Task<IActionResult> OnPostDelete()
		{
			if (!Id.HasValue)
			{
				return NotFound();
			}

			if (!User.Has(PermissionTo.DeleteRoles))
			{
				return AccessDenied();
			}

			try
			{
				MessageType = Styles.Success;
				Message = $"Role {Id}, deleted successfully.";
				_db.Roles.Attach(new Role { Id = Id.Value }).State = EntityState.Deleted;
				_publisher.SendUserManagement($"Role {Id} deleted by {User.Name()}", "", "Roles/List", User.Name());
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				MessageType = Styles.Danger;
				Message = $"Unable to delete Role {Id}, the role may have already been deleted or updated.";
			}

			return RedirectToPage("List");
		}

		public async Task<IActionResult> OnGetRolesThatCanBeAssignedBy(int[] ids)
		{
			var result = await _db.Roles
				.ThatCanBeAssignedBy(ids.Select(p => (PermissionTo)p))
				.Select(r => r.Name)
				.ToListAsync();

			return new JsonResult(result);
		}

		private void SetAvailableAssignablePermissions()
		{
			AvailableAssignablePermissions = Role.SelectedPermissions
				.Select(sp => new SelectListItem
				{
					Text = ((PermissionTo)sp).ToString(),
					Value = sp.ToString()
				});
		}

		private async Task AddUpdateRole(RoleEditModel model)
		{
			Role role;
			if (Id.HasValue)
			{
				role = await _db.Roles.SingleAsync(r => r.Id == Id);
				_db.RolePermission.RemoveRange(_db.RolePermission.Where(rp => rp.RoleId == Id));
				_db.RoleLinks.RemoveRange(_db.RoleLinks.Where(rp => rp.Role!.Id == Id));
				await _db.SaveChangesAsync();

				_publisher.SendUserManagement($"Role {model.Name} updated by {User.Name()}", "", $"Roles/Index?role={model.Name}", User.Name());
			}
			else
			{
				role = new Role();
				_db.Roles.Attach(role);
				_publisher.SendUserManagement($"New Role added: {model.Name} by {User.Name()}", "", $"Roles/Index?role={model.Name}", User.Name());
			}

			role.Name = model.Name;
			role.IsDefault = model.IsDefault;
			role.Description = model.Description;
			role.AutoAssignPostCount = model.AutoAssignPostCount;
			role.AutoAssignPublications = model.AutoAssignPublications;

			await _db.RolePermission.AddRangeAsync(model.SelectedPermissions
				.Select(p => new RolePermission
				{
					RoleId = role.Id,
					PermissionId = (PermissionTo)p,
					CanAssign = model.SelectedAssignablePermissions.Contains(p)
				}));

			await _db.RoleLinks.AddRangeAsync(model.Links.Select(rl => new RoleLink
			{
				Link = rl,
				Role = role
			}));
		}
	}
}
