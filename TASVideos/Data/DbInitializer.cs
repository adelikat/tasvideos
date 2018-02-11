﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TASVideos.Data.Entity;
using TASVideos.Data.SampleData;
using TASVideos.Data.SeedData;
using TASVideos.Extensions;
using TASVideos.WikiEngine;

namespace TASVideos.Data
{
	public static class DbInitializer
	{
		/// <summary>
		/// Creates the database and seeds it with necessary seed data
		/// Seed data is necessary data for a production release
		/// </summary>
		public static void Initialize(ApplicationDbContext context)
		{
			// For now, always delete then recreate the database
			// When the datbase is more mature we will move towards the Migrations process
			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();
		}

		public static void Migrate(ApplicationDbContext context)
		{
			// TODO
		}

		/// <summary>
		/// Adds data necessary for production, should be run before legacy migration processes
		/// </summary>
		public static void PreMigrateSeedData(ApplicationDbContext context)
		{
			context.Roles.Add(RoleSeedData.AdminRole);
			context.Roles.Add(RoleSeedData.SubmitMovies);
			context.Roles.Add(RoleSeedData.EditHomePage);
			context.SaveChanges();

			context.Roles.AddRange(RoleSeedData.Roles);
			context.GameSystems.AddRange(SystemSeedData.Systems);
			context.GameSystemFrameRates.AddRange(SystemSeedData.SystemFrameRates);
			context.Tiers.AddRange(TierSeedData.Tiers);
			context.SaveChanges();
		}

		public static void PostMigrateSeedData(ApplicationDbContext context)
		{
			context.Games.AddRange(GameSeedData.Games);

			foreach (var rom in GameSeedData.Roms)
			{
				rom.Game = context.Games.First(g => g.GoodName.StartsWith(rom.Name.Substring(0, 3))); // This is bad and not scalable
				context.Roms.Add(rom);
			}

			foreach (var wikiPage in WikiPageSeedData.NewRevisions)
			{
				var currentRevision = context.WikiPages
					.Where(wp => wp.PageName == wikiPage.PageName)
					.SingleOrDefault(wp => wp.Child == null);

				if (currentRevision != null)
				{
					wikiPage.Revision = currentRevision.Revision + 1;
					currentRevision.Child = wikiPage;
				}

				context.WikiPages.Add(wikiPage);
				var referrals = Util.GetAllWikiLinks(wikiPage.Markup);
				foreach (var referral in referrals)
				{
					context.WikiReferrals.Add(new WikiPageReferral
					{
						Referrer = wikiPage.PageName,
						Referral = referral.Link?.Split('|').FirstOrDefault(),
						Excerpt = referral.Excerpt
					});
				}
			}

			context.SaveChanges();
		}

		/// <summary>
		/// Adds optional sample data
		/// Unlike seed data, sample data is arbitrary data for testing purposes and would not be apart of a production release
		/// </summary>
		public static async Task GenerateDevSampleData(ApplicationDbContext context, UserManager<User> userManager)
		{
			foreach (var admin in UserSampleData.AdminUsers)
			{
				var result = await userManager.CreateAsync(admin, UserSampleData.SamplePassword);
				if (!result.Succeeded)
				{
					throw new Exception(string.Join(",", result.Errors.Select(e => e.ToString())));
				}

				var savedAdminUser = context.Users.Single(u => u.UserName == admin.UserName);
				savedAdminUser.EmailConfirmed = true;
				savedAdminUser.LockoutEnabled = false;

				context.UserRoles.Add(new UserRole { Role = RoleSeedData.AdminRole, User = savedAdminUser });

				// And one random role for testing multi-role
				context.UserRoles.Add(new UserRole { Role = RoleSeedData.Roles.AtRandom(), User = savedAdminUser });
			}

			foreach (var judge in UserSampleData.Judges)
			{
				var result = await userManager.CreateAsync(judge, UserSampleData.SamplePassword);
				if (!result.Succeeded)
				{
					throw new Exception(string.Join(",", result.Errors.Select(e => e.ToString())));
				}

				var savedUser = context.Users.Single(u => u.UserName == judge.UserName);
				savedUser.EmailConfirmed = true;
				savedUser.LockoutEnabled = false;

				context.UserRoles.Add(new UserRole { Role = RoleSeedData.Roles.Single(r => r.Name == "Judge"), User = savedUser });
				context.UserRoles.Add(new UserRole { Role = RoleSeedData.SubmitMovies, User = savedUser });
				context.UserRoles.Add(new UserRole { Role = RoleSeedData.EditHomePage, User = savedUser });
			}

			foreach (var user in UserSampleData.Users)
			{
				var result = await userManager.CreateAsync(user, UserSampleData.SamplePassword);
				if (!result.Succeeded)
				{
					throw new Exception(string.Join(",", result.Errors.Select(e => e.ToString())));
				}

				var savedUser = context.Users.Single(u => u.UserName == user.UserName);
				savedUser.EmailConfirmed = true;

				context.UserRoles.Add(new UserRole { Role = RoleSeedData.SubmitMovies, User = savedUser });
				context.UserRoles.Add(new UserRole { Role = RoleSeedData.EditHomePage, User = savedUser });
			}

			// Create lots of throw away users to test things like paging
			for (int i = 1; i <= 41; i++)
			{
				var dummyUser = new User
				{
					UserName = $"Dummy{i}",
					Email = $"Dummy{i}@example.com",
					EmailConfirmed = SampleGenerator.RandomBool(),
					LockoutEnd = SampleGenerator.RandomBool() && SampleGenerator.RandomBool()
						? DateTime.Now.AddMonths(1)
						: (DateTimeOffset?)null
				};

				await userManager.CreateAsync(dummyUser, UserSampleData.SamplePassword);

				context.UserRoles.Add(new UserRole { Role = RoleSeedData.SubmitMovies, User = dummyUser });
				context.UserRoles.Add(new UserRole { Role = RoleSeedData.EditHomePage, User = dummyUser });
			}

			await context.SaveChangesAsync();
		}
	}
}
