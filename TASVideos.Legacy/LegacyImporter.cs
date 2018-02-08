﻿using System.Linq;
using TASVideos.Data;
using TASVideos.Legacy.Data.Forum;
using TASVideos.Legacy.Data.Site;
using TASVideos.Legacy.Imports;

namespace TASVideos.Legacy
{
    public static class LegacyImporter
    {
		public static void RunLegacyImport(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext,
			NesVideosForumContext legacyForumContext)
		{
			// For now assume any wiki pages means the importer has run
			if (context.WikiPages.Any())
			{
				return;
			}

			UserImporter.Import(context, legacySiteContext, legacyForumContext);
			//WikiImporter.Import(context, legacySiteContext);
		}
	}
}
