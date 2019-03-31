﻿namespace TASVideos
{
	public static class EnvironmentVariables
	{
		public const string StartupStrategy = "StartupStrategy";
	}

	public static class LinkConstants
	{
		public const string SubmissionWikiPage = "InternalSystem/SubmissionContent/S";
		public const string PublicationWikiPage = "InternalSystem/PublicationContent/M";
		public const string GameWikiPage = "InternalSystem/GameContent/G";
	}

	public static class Durations
	{
		public const int OneMinuteInSeconds = 60;
		public const int FiveMinutesInSeconds = 60 * 5;
		public const int OneDayInSeconds = 60 * 60 * 24;
		public const int OneWeekInSeconds = 60 * 60 * 24 * 7;
	}

	public static class CacheKeys
	{
		public const string WikiCache = "WikiCache";
		public const string AwardsCache = "AwardsCache";
		public const string UnreadMessageCount = "UnreadMessageCountCache-";
		public const string MovieTokens = "MovieTokenData";
		public const string MovieRatingKey = "OverallRatingForMovieViewModel-";
	}

	// These perform site functions, maybe they should be in the database?
	public static class SiteGlobalConstants
	{
		public const int VestedPostCount = 3; // Minimum number of posts to become an experienced forum user
		public const int MinimumHoursBeforeJudgment = 72; // Minimum number of hours before a judge can set a submission to accepted/rejected

		public const string TASVideoAgent = "TASVideoAgent";
		public const int TASVideoAgentId = 505;

		public const string NewPublicationPostSubject = "Movie published";
		public const string NewPublicationPost = @"<hr/><b>This movie has been published.</b><br/>The posts before this message apply to the submission, and posts after this message apply to the published movie.<hr/>See the <a href=""/{PublicationId}M"">publication</a><br/><hr/>";

		public const string NewSubmissionPost = @"This topic is for the purpose of discussing ";
		public const string PollQuestion = @"Vote: Did you like watching this movie? (Vote after watching!)<br/><br/><span style=""color:#C03"">Note: Because of abuse that has happened, lurkers can't vote anymore.</span>";
		public const string PollOptionYes = "Yes";
		public const string PollOptionNo = "No";
		public const string PollOptionsMeh = "Meh";
	}

	public static class ForumConstants
	{
		public const int PostsPerPage = 25;
		public const int TopicsPerForum = 50;

		public const int WorkBenchForumId = 7;
	}

	public static class PostGroups
	{
		public const string Forum = "Forum";
		public const string Wiki = "Wiki";
		public const string Submission = "Submission";
		public const string UserManagement = "UserManagement";
	}

	// TODO: this is bootstrap specific, maybe it should go in the MVC project
	// TODO: a better name
	public static class Styles
	{
		public const string Info = "info";
		public const string Success = "success";
		public const string Warning = "warning";
		public const string Danger = "danger";
	}

	public static class CustomClaimTypes
	{
		public const string Permission = "P";
	}

	public static class AvatarRequirements
	{
		public const int Width = 100;
		public const int Height = 100;
	}
}