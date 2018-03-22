﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Legacy.Data.Forum;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
    public static class ForumCategoriesImporter
    {
		public static void Import(
			ApplicationDbContext context,
			NesVideosForumContext legacyForumContext)
		{
			var categories = legacyForumContext
				.Categories
				.Select(c => new ForumCategory
				{
					Id = c.Id,
					Title = c.Title,
					Ordinal = c.Order,
					Description = c.Description,
					CreateTimeStamp = DateTime.UtcNow,
					LastUpdateTimeStamp = DateTime.UtcNow,
					CreateUserName = "LegacyImport",
					LastUpdateUserName = "LegacyImport"
				})
				.ToList();

			var columns = new[]
			{
				nameof(ForumCategory.Id),
				nameof(ForumCategory.Title),
				nameof(ForumCategory.Ordinal),
				nameof(ForumCategory.Description),
				nameof(ForumCategory.CreateTimeStamp),
				nameof(ForumCategory.LastUpdateTimeStamp),
				nameof(ForumCategory.CreateUserName),
				nameof(ForumCategory.LastUpdateUserName)
			};

			categories.BulkInsert(context, columns, nameof(ApplicationDbContext.ForumCategories));
		}
    }
}