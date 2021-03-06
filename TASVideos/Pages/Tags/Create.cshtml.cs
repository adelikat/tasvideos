﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.Pages.Tags
{
	[RequirePermission(PermissionTo.TagMaintenance)]
	public class CreateModel : BasePageModel
	{
		private readonly ITagService _tagService;

		public CreateModel(ITagService tagService)
		{
			_tagService = tagService;
		}

		[TempData]
		public string? Message { get; set; }

		[TempData]
		public string? MessageType { get; set; }

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public Tag Tag { get; set; } = new ();

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var result = await _tagService.Add(Tag.Code, Tag.DisplayName);
			switch (result)
			{
				default:
				case TagEditResult.Success:
					MessageType = Styles.Success;
					Message = "Tag successfully created.";
					return RedirectToPage("Index");
				case TagEditResult.DuplicateCode:
					ModelState.AddModelError($"{nameof(Tag)}.{nameof(Tag.Code)}", $"{nameof(Tag.Code)} {Tag.Code} already exists");
					MessageType = null;
					Message = null;
					return Page();
				case TagEditResult.Fail:
					MessageType = Styles.Danger;
					Message = "Unable to edit tag due to an unknown error";
					return Page();
			}
		}
	}
}
