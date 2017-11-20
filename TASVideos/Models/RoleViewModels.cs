using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents a Role entry for the purpose of display
	/// </summary>
	public class RoleDisplayViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public IEnumerable<string> Permissions { get; set; } = new List<string>();
	}

	/// <summary>
	/// Represents a Role entry for the purpose of editing
	/// </summary>
	public class RoleEditViewModel
	{
		public int? Id { get; set; }

		[Required]
		[StringLength(50)]
		[Display(Name = "Name")]
		public string Name { get; set; }

		[Required]
		[StringLength(200)]
		public string Description { get; set; }

		[AtLeastOne(ErrorMessage = "At least one permission is required.")]
		[Display(Name = "Selected Permissions")]
		public IEnumerable<int> SelectedPermissions { get; set; } = new List<int>();

		[Display(Name = "Available Permissions")]
		public IEnumerable<SelectListItem> AvailablePermissions { get; set; } = new List<SelectListItem>();

		[Display(Name = "Selected Assignable Permissions")]
		public IEnumerable<int> SelectedAssignablePermissions { get; set; } = new List<int>();

		// This is actually a dummy property that is never actually used to store data. It serves the purpose of passing metadata into the two column picker
		[Display(Name = "Available Assignable Permissions")]
		public IEnumerable<SelectListItem> AvailableAssignablePermissions { get; set; } = new List<SelectListItem>();
	}

	/// <summary>
	/// Represents a conscise view of Role for the User profile screen
	/// </summary>
	public class RoleBasicDisplay
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}
}