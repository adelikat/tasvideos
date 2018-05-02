﻿using System;
using System.Data.SqlClient;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	public static class ForumPollImporter
	{
		public static void 
			Import(
			ApplicationDbContext context,
			NesVideosForumContext legacyForumContext)
		{
			/******** ForumPoll ********/
			var legVoteDescriptions = (from p in legacyForumContext.VoteDescription
				join t in legacyForumContext.Topics on p.TopicId equals t.Id // The join is to filter out orphan polls
				select p)
				.ToList();

			var forumPolls = legVoteDescriptions
				.Select(v => new ForumPoll
				{
					Id = v.Id,
					TopicId = v.TopicId,
					Question = ImportHelper.FixString(v.Text),
					CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(v.VoteStart),
					CreateUserName = "Unknown", // TODO: we could try to get the topic creator when not -1 if it is worth it
					LastUpdateUserName = "Unknown", // Ditto
					CloseDate = v.VoteLength == 0
						? (DateTime?)null
						: ImportHelper.UnixTimeStampToDateTime(v.VoteStart + v.VoteLength)
				});

			var pollColumns = new[]
			{
				nameof(ForumPoll.Id),
				nameof(ForumPoll.TopicId),
				nameof(ForumPoll.Question),
				nameof(ForumPoll.CloseDate),
				nameof(ForumPoll.CreateTimeStamp),
				nameof(ForumPoll.CreateUserName),
				nameof(ForumPoll.LastUpdateTimeStamp),
				nameof(ForumPoll.LastUpdateUserName)
			};

			forumPolls.BulkInsert(context, pollColumns, nameof(ApplicationDbContext.ForumPolls));

			/******** ForumPollOption ********/
			var legForumPollOptions =
				(from vr in legacyForumContext.VoteResult
				join v in legacyForumContext.VoteDescription on vr.Id equals v.Id // The joins are to filter out orphan options
				join t in legacyForumContext.Topics on v.TopicId equals t.Id
				select vr)
				.ToList();

			var forumPollOptions = legForumPollOptions
			.Select(r => new ForumPollOption
				{
					Text = r.VoteOptionText,
					PollId = r.Id,
					Ordinal = r.VoteOptionId,
					CreateTimeStamp = DateTime.UtcNow,
					LastUpdateTimeStamp = DateTime.UtcNow,
					CreateUserName = "Unknown", // TODO: could use the topic creator
					LastUpdateUserName = "Unknown"
				})
				.ToList();

			var pollOptionColumns = new[]
			{
				nameof(ForumPollOption.Text),
				nameof(ForumPollOption.Ordinal),
				nameof(ForumPollOption.PollId),
				nameof(ForumPollOption.CreateTimeStamp),
				nameof(ForumPollOption.CreateUserName),
				nameof(ForumPollOption.LastUpdateTimeStamp),
				nameof(ForumPollOption.LastUpdateUserName)
			};

			forumPollOptions.BulkInsert(context, pollOptionColumns, nameof(ApplicationDbContext.ForumPollOptions), SqlBulkCopyOptions.Default);

			/******** ForumPollOptionVote ********/
			var legForumVoters = legacyForumContext.Voter.ToList();
			var newForumOptions = context.ForumPollOptions.ToList();

			var forumPollOptionVotes =
				(from v in legForumVoters
				 join po in newForumOptions on new { PollId = v.Id, Ordinal = v.OptionId } equals new { po.PollId, po.Ordinal }
				 select new ForumPollOptionVote
				 {
					 PollOptionId = po.Id,
					 UserId = v.UserId,
					 IpAddress = v.IpAddress,
					 CreateTimestamp = DateTime.UtcNow, // Legacy system did not track this
				 })
				.ToList();

			// Insert Unknown User votes for discrepencies between de-normalized vote count and actual vote records
			var missingVotes =
				(from po in legForumPollOptions
				 join v in legForumVoters on new { po.Id, OptionId = po.VoteOptionId } equals new { v.Id, v.OptionId }
				 join newPo in newForumOptions on new { PollId = v.Id, Ordinal = po.VoteOptionId } equals new { newPo.PollId, newPo.Ordinal }
				 select new { po, v, newPo })
				.GroupBy(tkey => new { tkey.newPo.Id, tkey.po.ResultCount })
				.Select(g => new { g.Key.Id, g.Key.ResultCount, Actual = g.Count() })
				.Where(x => x.ResultCount > x.Actual)
				.ToList();

			foreach (var option in missingVotes)
			{
				var diff = option.ResultCount - option.Actual;

				for (int i = 0; i < diff; i++)
				{
					forumPollOptionVotes.Add(new ForumPollOptionVote
					{
						PollOptionId = option.Id,
						UserId = -1,
						IpAddress = null,
						CreateTimestamp = DateTime.UtcNow
					});
				}
			}

			var pollVoteColumns = new[]
			{
				nameof(ForumPollOptionVote.PollOptionId),
				nameof(ForumPollOptionVote.UserId),
				nameof(ForumPollOptionVote.CreateTimestamp),
				nameof(ForumPollOptionVote.IpAddress)
			};

			forumPollOptionVotes.BulkInsert(context, pollVoteColumns, nameof(ApplicationDbContext.ForumPollOptionVotes), SqlBulkCopyOptions.Default);
		}
	}
}