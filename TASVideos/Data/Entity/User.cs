﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity
{
	[Table(nameof(User))]
	public class User : IdentityUser<int>, ITrackable
	{
		public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();

		public DateTime CreateTimeStamp { get; set; } = DateTime.UtcNow;
		public string CreateUserName { get; set; }

		public DateTime LastUpdateTimeStamp { get; set; } = DateTime.UtcNow;
		public string LastUpdateUserName { get; set; }

		public DateTime? LastLoggedInTimeStamp { get; set; }
	}
}
