﻿using Fritz.StreamTools.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Octokit;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using LazyCache;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Fritz.StreamTools.Controllers
{
  public class GitHubController : Controller
	{
		public GitHubController(
			IAppCache cache,
			GitHubRepository repository,
			ILogger<GitHubController> logger,
			IOptions<GitHubConfiguration> githubConfiguration)
		{
			this.Cache = cache;
			this.Logger = logger;
			_gitHubRepository = repository;
			_gitHubConfiguration = githubConfiguration.Value;
		}

    public IAppCache Cache { get; }
    public ILogger<GitHubController> Logger { get; }

		private readonly GitHubRepository _gitHubRepository;

		private readonly GitHubConfiguration _gitHubConfiguration;

		public async Task<IActionResult> ContributorsInformation()
		{
			var outModel = await _gitHubRepository.GetRecentContributors(_gitHubConfiguration.RepositoryCsv);

			ViewBag.Configuration = _gitHubConfiguration;

			return View($"contributor_{_gitHubConfiguration.DisplayMode}", outModel.ToArray());

		}

		public IActionResult Configuration()
		{
			return View(_gitHubConfiguration);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Configuration(GitHubConfiguration configuration)
		{
			if (ModelState.IsValid)
			{
					_gitHubConfiguration.RepositoryName = configuration.RepositoryName;
					_gitHubConfiguration.RepositoryOwner = configuration.RepositoryOwner;
			}

			return View(configuration);
		}

		[AcceptVerbs("Get", "Post")]
		public async Task<IActionResult> VerifyUser(string repositoryOwner)
		{
			try
			{
				var user
					= await _gitHubRepository.Client.User.Get(repositoryOwner);
			}
			catch (NotFoundException)
			{
				return Json("User not found.");
			}
			catch (Exception)
			{
				return Json("Ops... something went wrong.");
			}

			return Json(true);
		}

		[AcceptVerbs("Get", "Post")]
		public async Task<IActionResult> VerifyRepository(string repositoryName, string repositoryOwner)
		{
			try
			{
				var repository =
					await _gitHubRepository.Client.Repository.Get(repositoryOwner, repositoryName);
			}
			catch (NotFoundException)
			{
				return Json("Repository not found.");
			}
			catch (Exception)
			{
				return Json("Ops... something went wrong.");
			}

			return Json(true);
		}
  }
}
