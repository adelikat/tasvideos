﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.Extensions
{
	// ReSharper disable PossibleMultipleEnumeration
	public static class WikiHelper
	{
		public static bool UserCanEditWikiPage(string pageName, string userName, IEnumerable<PermissionTo> userPermissions)
		{
			if (userPermissions == null)
			{
				throw new ArgumentNullException($"{nameof(userPermissions)} can not be null");
			}

			if (string.IsNullOrWhiteSpace(pageName) || string.IsNullOrWhiteSpace(userName))
			{
				return false;
			}

			pageName = pageName.Trim('/');

			// Anyone who can edit anything (including the user's own homepage) should be allowed to edit his
			if (pageName == "SandBox")
			{
				return userPermissions.Contains(PermissionTo.EditGameResources)
					|| userPermissions.Contains(PermissionTo.EditHomePage)
					|| userPermissions.Contains(PermissionTo.EditWikiPages)
					|| userPermissions.Contains(PermissionTo.EditSystemPages);
			}

			if (pageName.StartsWith("GameResources/"))
			{
				return userPermissions.Contains(PermissionTo.EditGameResources);
			}

			if (pageName.StartsWith("System/"))
			{
				return userPermissions.Contains(PermissionTo.EditSystemPages);
			}

			if (pageName.StartsWith("Homepages/"))
			{
				// A home page is defiend as Homepages/[UserName]
				// If a user can exploit this fact to create an exploit
				// then we should first reconsider rules about allowed patterns of usernames and what defines a valid wiki page
				// before deciding to nuke this feature
				var homepage = pageName.Split("Homepages/")[1];
				if (string.Equals(homepage, userName, StringComparison.OrdinalIgnoreCase)
					&& userPermissions.Contains(PermissionTo.EditHomePage))
				{
					return true;
				}

				// Notice we fall back to EditWikiPages if it is not the user's homepage, regular editors should be able to edit homepages
			}

			return userPermissions.Contains(PermissionTo.EditWikiPages);
		}

		public static bool IsValidWikiPageName(string pageName)
		{
			// If the page is a homepage, then don't validate the username portion
			// However we want to validate any subpages off the user
			// Ex:
			// HomePages/My Bad UserName = valid
			// HomePages/My Bad UserName/My Bad Subpage = invalid
			string test = pageName;
			if (IsHomePage(pageName))
			{
				test = pageName.Replace("HomePages/", "");
				var slashIndex = test.IndexOf('/');
				if (slashIndex == -1)
				{
					return true; // Just HomePage/[username] so it is automatically valid
				}

				test = test.Substring(slashIndex, test.Length - slashIndex);
			}

			return !string.IsNullOrWhiteSpace(test)
				&& !test.StartsWith('/')
				&& !test.EndsWith('/')
				&& !test.EndsWith(".html")
				&& Regex.IsMatch(test, @"^\S*$")
				&& char.IsUpper(test[0])
				&& IsProperCased(test);
		}

		private static bool IsHomePage(string pageName)
		{
			return pageName.StartsWith("HomePages/");
		}

		// Does not check for null that should have already been done
		// Slashes must have already been trimmed or it will break
		private static bool IsProperCased(string pageName)
		{
			if (!char.IsUpper(pageName[0]))
			{
				return false;
			}

			var slashes = Util.AllIndexesOf(pageName, "/");
			foreach (var slash in slashes)
			{
				if (!char.IsUpper(pageName[slash + 1]))
				{
					return false;
				}
			}

			return true;
		}
	}
}
