﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Data.Entity
{
    public class Submission : BaseEntity
    {
		public int Id { get; set; }
		public virtual WikiPage WikiContent { get; set; }
		public virtual User Submitter { get; set; }

		public virtual ICollection<SubmissionAuthor> SubmissionAuthors { get; set; } = new HashSet<SubmissionAuthor>();

		// TODO: game id
		// TODO: intended tier

		public virtual User Judge { get; set; }
		public virtual User Publisher { get; set; }

		//TODO: a history table of status changes, require queue maintainers to put in a message (this can replace the need for users to have to add it into the wiki content)
		public SubmissionStatus Status { get; set; } = SubmissionStatus.New;

		// TODO: we eventually should want to move these to the file server instead
		public byte[] MovieFile { get; set; }

		// Metadata parsed from movie file
		public virtual GameSystem System { get; set; }
		public int Frames { get; set; }
		public decimal FrameRate { get; set; }
		public int RerecordCount { get; set; }

		// Metadata, user entered

		// TODO: additional non-registered authors field

		[StringLength(20)]
		public string GameVersion { get; set; }

		[StringLength(100)]
		public string GameName { get; set; }

		[StringLength(50)]
		public string Branch { get; set; }

		[StringLength(100)]
		public string RomName { get; set; }
		
		[StringLength(50)]
		public string EmulatorVersion { get; set; }
	}

	public enum SubmissionStatus
	{
		New,
		Delayed,
		NeedsMoreInfo,
		JudgingUnderWay,
		Accepted,
		PublicationUnderway,
		Published,
		Rejected
	}
}
