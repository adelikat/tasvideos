﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity
{
	/// <summary>
	/// The table representation of a <seealso cref="PermissionTo"/>
	/// This table is a convenience table to aid in documentation of these permissions
	/// </summary>
	public class Permission
	{
		[Required]
		public PermissionTo Id { get; set; }

		[Required]
		[StringLength(50)]
		public string Name { get; set; }

		[StringLength(200)]
		public string Description { get; set; }

		[StringLength(20)]
		public string Group { get; set; }

		public virtual ICollection<RolePermission> RolePermission { get; set; }
	}

	/// <summary>
	/// Represents the most granular level permissions possible in the site. All site code is based on these permissions
	/// The <see cref="Permission" /> table documents these permissions but this is informational and do not affect the site behavior />
	/// The <seealso cref="Role" /> table represents a group of permissions that can be assigned to a <seealso cref="User"/>
	/// </summary>
	public enum PermissionTo
	{
		EditWikiPages = 1,
		EditGameResources = 2,
		EditSystemPages = 3,
		EditRoles = 4,
		DeleteRoles = 5,
		ViewUsers = 6,
		EditUsers = 7,
		EditPermissionDetails = 8,
		EditUsersUserName = 9,
		FullAssignRoles = 10,
		PartialAssignRoles = 11
	}

	/// <summary>
	/// These are the grouping mechanism to <seealso cref="Permission" />
	/// The intent is for this to be a convenience for finding and understanding permissions and to improve the UI experience when assigning them
	/// </summary>
	public static class PermissionGroups
	{
		public const string Editing = "Editing";
		public const string UserAdministration = "UserAdministration";
	}
}
