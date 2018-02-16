﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;

using FastMember;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.SeedData;
using TASVideos.Legacy.Data.Forum;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class UserImporter
	{
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext,
			NesVideosForumContext legacyForumContext)
		{
			// TODO:
			// forum user_active status?
			// import forum users that have no wiki, but check if they are forum banned
			// gender?
			// timezone

			var legacyUsers = legacySiteContext.Users
				.OrderBy(u => u.Id)
				.ToList();

			var legacyForumUsers = legacyForumContext.Users
				.Where(u => u.UserName != "Anonymous")
				.OrderBy(u => u.UserId)
				.ToList();

			var luserRoles = legacySiteContext.UserRoles.ToList();
			var lroles = legacySiteContext.Roles.ToList();

			var roles = context.Roles.ToList();

			// TODO: what to do about these??
			//var wikiNoForum = legacyUsers
			//	.Select(u => u.Name)
			//	.Except(legacyForumUsers.Select(u => u.UserName))
			//	.ToList();

			var users = new List<User>();
			var userRoles = new List<UserRole>();
			foreach (var legacyForumUser in legacyForumUsers)
			{
				var newUser = new User
				{
					Id = legacyForumUser.UserId,
					UserName = legacyForumUser.UserName,
					NormalizedUserName = legacyForumUser.UserName.ToUpper(),
					CreateTimeStamp = ImportHelpers.UnixTimeStampToDateTime(legacyForumUser.RegDate),
					LastUpdateTimeStamp = ImportHelpers.UnixTimeStampToDateTime(legacyForumUser.RegDate), // TODO
					LegacyPassword = legacyForumUser.Password,
					EmailConfirmed = legacyForumUser.EmailTime != null,
					Email = legacyForumUser.Email,
					NormalizedEmail = legacyForumUser.Email.ToUpper(),
					CreateUserName = "Automatic Migration",
					PasswordHash = ""
				};

				users.Add(newUser);

				var legacySiteUser = legacyUsers.SingleOrDefault(u => u.Name == legacyForumUser.UserName);

				if (legacySiteUser != null)
				{
					var legacyUserRoles =
						(from lr in luserRoles
						join r in lroles on lr.RoleId equals r.Id
						where lr.UserId == legacySiteUser.Id
						select r)
						.ToList();

					// not having user means they are effectively banned
					// limited = Limited User
					if (legacyUserRoles.Select(ur => ur.Name).Contains("user")
						&& !legacyUserRoles.Select(ur => ur.Name).Contains("admin")) // There's no point in adding these roles to admins, they have these perms anyway
					{
						userRoles.Add(new UserRole
						{
							RoleId = roles.Single(r => r.Name == SeedRoleNames.EditHomePage).Id,
							UserId = newUser.Id
						});

						if (!legacyUserRoles.Select(ur => ur.Name).Contains("limited"))
						{
							context.UserRoles.Add(new UserRole
							{
								RoleId = roles.Single(r => r.Name == SeedRoleNames.SubmitMovies).Id,
								UserId = newUser.Id
							});
						}
					}

					foreach (var userRole in legacyUserRoles
						.Where(r => r.Name != "user" && r.Name != "limited"))
					{
						var role = GetRoleFromLegacy(userRole.Name, roles);
						if (role != null)
						{
							context.UserRoles.Add(new UserRole
							{
								RoleId = role.Id,
								UserId = newUser.Id
							});
						}
					}
				}
				else
				{
					// TODO: check any kind of active/ban forum status if none, then give them homepage and submit rights
				}
			}

			// Some published authors that have no forum account
			// Note that by having no password nor legacy password they effectively can not log in without a database change
			// I think this is correct since these are not active users
			var portedPlayerNames = new[]
			{
				"Morimoto",
				"Tokushin",
				"Yy",
				"Mathieu P",
				"Linnom",
				"Mclaud2000",
				"Ryosuke",
				"JuanPablo",
				"qcommand",
				"Mana."
			};

			var portedPlayers = portedPlayerNames.Select(p => new User
			{
				UserName = p,
				NormalizedUserName = p.ToUpper(),
				Email = $"imported{p}@tasvideos.org",
				NormalizedEmail = $"imported{p}@tasvideos.org".ToUpper(),
				CreateTimeStamp = DateTime.UtcNow,
				LastUpdateTimeStamp = DateTime.UtcNow
			});

			var userCopyParams = new[]
			{
				nameof(User.Id),
				nameof(User.UserName),
				nameof(User.NormalizedUserName),
				nameof(User.CreateTimeStamp),
				nameof(User.LegacyPassword),
				nameof(User.EmailConfirmed),
				nameof(User.Email),
				nameof(User.NormalizedEmail),
				nameof(User.CreateUserName),
				nameof(User.PasswordHash),
				nameof(User.AccessFailedCount),
				nameof(User.LastUpdateTimeStamp),
				nameof(User.LockoutEnabled),
				nameof(User.PhoneNumberConfirmed),
				nameof(User.TwoFactorEnabled)
			};

			using (var userSqlCopy = new SqlBulkCopy(context.Database.GetDbConnection().ConnectionString, SqlBulkCopyOptions.KeepIdentity))
			{
				userSqlCopy.DestinationTableName = "[User]";
				userSqlCopy.BatchSize = 10000;
				
				foreach (var param in userCopyParams)
				{
					userSqlCopy.ColumnMappings.Add(param, param);
				}

				using (var reader = ObjectReader.Create(users, userCopyParams))
				{
					userSqlCopy.WriteToServer(reader);
				}
			}

			var userRoleCopyParmas = new[]
			{
				nameof(UserRole.UserId),
				nameof(UserRole.RoleId)
			};

			using (var userRoleSqlCopy = new SqlBulkCopy(context.Database.GetDbConnection().ConnectionString, SqlBulkCopyOptions.KeepIdentity))
			{
				userRoleSqlCopy.DestinationTableName = "[UserRoles]";
				userRoleSqlCopy.BatchSize = 10000;

				foreach (var param in userRoleCopyParmas)
				{
					userRoleSqlCopy.ColumnMappings.Add(param, param);
				}

				using (var reader = ObjectReader.Create(userRoles, userRoleCopyParmas))
				{
					userRoleSqlCopy.WriteToServer(reader);
				}
			}

			using (var playerSqlCopy = new SqlBulkCopy(context.Database.GetDbConnection().ConnectionString))
			{
				playerSqlCopy.DestinationTableName = "[User]";
				playerSqlCopy.BatchSize = 10000;

				var playerParams = userCopyParams.Where(p => p != nameof(User.Id)).ToArray();
				foreach (var param in playerParams)
				{
					playerSqlCopy.ColumnMappings.Add(param, param);
				}

				using (var reader = ObjectReader.Create(portedPlayers, playerParams))
				{
					playerSqlCopy.WriteToServer(reader);
				}
			}
		}

		private static Role GetRoleFromLegacy(string role, IEnumerable<Role> roles)
		{
			switch (role.ToLower())
			{
				default:
					return null;
				case "editor":
					return roles.Single(r => r.Name == SeedRoleNames.Editor);
				case "vestededitor":
					return roles.Single(r => r.Name == SeedRoleNames.VestedEditor);
				case "publisher":
					return roles.Single(r => r.Name == SeedRoleNames.Publisher);
				case "seniorpublisher":
					return roles.Single(r => r.Name == SeedRoleNames.SeniorPublisher);
				case "judge":
					return roles.Single(r => r.Name == SeedRoleNames.Judge);
				case "seniorjudge":
					return roles.Single(r => r.Name == SeedRoleNames.SeniorJudge);
				case "adminassistant":
					return roles.Single(r => r.Name == SeedRoleNames.AdminAssistant);
				case "admin":
					return roles.Single(r => r.Name == SeedRoleNames.Admin);
			}
		}
	}
}
