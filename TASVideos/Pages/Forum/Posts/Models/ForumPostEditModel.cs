﻿using System;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Posts.Models
{
	public class ForumPostEditModel
	{
		public int PosterId { get; set; }
		public string PosterName { get; set; }
		public DateTime CreateTimestamp { get; set; }

		public bool EnableBbCode { get; set; }
		public bool EnableHtml { get; set; }

		public int TopicId { get; set; }
		public string TopicTitle { get; set; }

		[StringLength(500)]
		public string Subject { get; set; }

		[Required]
		public string Text { get; set; }

		public string RenderedText { get; set; }

		public bool IsLastPost { get; set; }
	}
}