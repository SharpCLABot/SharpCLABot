// Copyright (c) 2014, Alexandre Mutel
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted 
// provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this list of conditions 
//    and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions
//    and the following disclaimer in the documentation and/or other materials provided with the 
//    distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER 
// IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
//  THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Octokit;
using SharpCLABot.Helpers;
using SharpCLABot.ViewModels;

namespace SharpCLABot.Controllers
{
    /// <summary>
    /// The controller used for the admin configuration.
    /// </summary>
    public class AdminController : CLABotControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        public AdminController()
        {
        }

        /// <summary>
        /// Main url for handling CLA signing
        /// </summary>
        /// <returns>Task{ActionResult}.</returns>
        [Authorize]
        public async Task<ActionResult> Index()
        {
            // Redirect to logon if the token is empty
            if (!CheckAuthentification())
            {
                return RedirectToAction("LogOn");
            }

            return View(await CreateViewModel());
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Index(AdminConfigViewModel configViewModel)
        {
            // Redirect to logon if the token is empty
            if (!CheckAuthentification())
            {
                return RedirectToAction("LogOn");
            }

            configViewModel.Config.IsValid = ModelState.IsValid;

            ModelState.Clear();

            await UpdateFromViewModel(configViewModel);

            UpdateContributors(configViewModel);

            // Redirect to Index to make sure the ModelState is updated
            return View("Index", configViewModel);
        }

        public async Task<ActionResult> LogOn()
        {
            if (CheckAuthentification())
            {
                return RedirectToAction("Index");
            }

            var logOnViewModel = new LogOnViewModel() { IsSetup =  !AdminConfig.Instance.IsTokenConfigured };
            return View(logOnViewModel);
        }

        [HttpPost]
        public async Task<ActionResult> LogOn(LogOnViewModel logOnViewModel)
        {
            if (CheckAuthentification())
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!GithubHelper.CheckTokenConfigured(logOnViewModel.Token))
                    {
                        // Performs a fullcheck of token access (check current user...etc.)
                        await GithubHelper.ConnectAndValidate(logOnViewModel.Token, true);

                        // If token is valid, we can store it
                        AdminConfig.Instance.GitHubAdminApplicationToken = logOnViewModel.Token;
                    }

                    // If we are here, the token is valid, otherwise we had an exception
                    FormsAuthentication.SetAuthCookie("admin", logOnViewModel.RememberMe);
                    
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Token", ex.Message);
                }
            }

            ModelState.AddModelError("", "Invalid github token");
            return View(logOnViewModel);
        }

        [Authorize]
        [HttpPost]
        public ActionResult CheckDb(string connectionString)
        {
            // Redirect to logon if the token is empty
            if (!CheckAuthentification())
            {
                return RedirectToAction("LogOn");
            }

            var task = new Task<DbConnectionStatus>(() =>
            {
                try
                {
                    CheckSqlConnection(connectionString);
                }
                catch (Exception ex)
                {
                    return new DbConnectionStatus(false, ex.Message);
                }
                return new DbConnectionStatus(true, "Connection is Ok");
            }, TaskCreationOptions.LongRunning);

            task.Start();
            return new JsonResult() {Data = task.Wait(3000) ? task.Result : new DbConnectionStatus(false, "Unable to connect to database")};
        }

        private void CheckSqlConnection(string connectionString)
        {
            connectionString += ";Connection Timeout=2";
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult GetContributors()
        {
            // Redirect to logon if the token is empty
            if (!CheckAuthentification())
            {
                return RedirectToAction("LogOn");
            }

            return new JsonResult() { Data = Db.Contributors.ToList() };
        }

        private bool CheckAuthentification()
        {
            if (Request.IsAuthenticated)
            {
                // If we are here, the token is valid, otherwise we had an exception
                if (!AdminConfig.Instance.IsTokenConfigured)
                {
                    FormsAuthentication.SignOut();
                    return false;
                }
                return true;
            }
            return false;
        }

        private async Task<AdminConfigViewModel> CreateViewModel()
        {
            var config = AdminConfig.Instance;
            var viewModel = new AdminConfigViewModel(config);

            // TODO implement a way to refresh repositories
            if (config.Hooks.Count == 0)
            {
                await UpdateWebHooks(viewModel);
                config.Save();
            }

            UpdateContributors(viewModel);

            return viewModel;
        }

        private void UpdateContributors(AdminConfigViewModel viewModel)
        {
            try
            {
                // Before accessing check that the sql connection is valid
                CheckSqlConnection(AdminConfig.Instance.ConnectionStringDb);
                viewModel.Contributors.Clear();
                var contributors = Db.Contributors.ToList();
                viewModel.Contributors.AddRange(contributors.Select(contributor => new ContributorViewModel(contributor)));
            }
            catch (Exception)
            {
            }
        }

        private async Task<bool> UpdateWebHooks(AdminConfigViewModel viewModel)
        {
            bool hooksChanged = false;
            try
            {
                var config = viewModel.Config;

                var github = await GithubHelper.ConnectAndValidate(config.GitHubAdminApplicationToken, false);

                var repoHooks = await GithubHelper.GetAllHooksForCallback(github, config.GitHubWebHookCallbackUrl);

                foreach (var repoHook in repoHooks)
                {
                    var repository = repoHook.Item1;
                    var hook = repoHook.Item2;

                    var repoHookViewModel = new GitHubRepositoryHook()
                    {
                        Owner = repository.Owner.Login,
                        Name = repository.Name,
                        Installed = hook != null
                    };

                    if (hook != null)
                    {
                        repoHookViewModel.Id = hook.Id;
                    }

                    if (!config.Hooks.Any(existingHook => existingHook.Owner == repoHookViewModel.Owner && existingHook.Name == repoHookViewModel.Name))
                    {
                        config.Hooks.Add(repoHookViewModel);
                        hooksChanged = true;
                    }
                }

                // Remove hooks no longer valid
                var registeredHooks = new List<GitHubRepositoryHook>(config.Hooks);
                foreach (var hook in registeredHooks)
                {
                    if (!repoHooks.Any(tuple => tuple.Item1.Owner.Login == hook.Owner && tuple.Item1.Name == hook.Name))
                    {
                        config.Hooks.Remove(hook);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", string.Format("Unable to get the list of repositories: {0}", ex));
            }
            return hooksChanged;
        }

        private async Task UpdateFromViewModel(AdminConfigViewModel viewModel)
        {
            var config = viewModel.Config;

            var isToUpdate = string.Compare(AdminConfig.Instance.GitHubWebHookCallbackUrl, config.GitHubWebHookCallbackUrl) != 0 ||
                string.Compare(AdminConfig.Instance.GitHubWebHookCallbackSecret, config.GitHubWebHookCallbackSecret) != 0;

            var newUrl = config.GitHubWebHookCallbackUrl;
            if (string.IsNullOrWhiteSpace(newUrl))
            {
                newUrl = LicensingController.GetDefaultWebHookCallback(Request.Url);
            }

            // Update repositories if required
            if (viewModel.RefreshRepositories)
            {
                if (await UpdateWebHooks(viewModel))
                {
                    viewModel.RefreshRepositories = false;
                }
            }

            // Check if we have any hooks to: Update, Create or Remove
            var hooksToUpdate = new List<GitHubRepositoryHook>();
            var hooksToCreate = new List<GitHubRepositoryHook>();
            var hooksToRemove = new List<GitHubRepositoryHook>();

            foreach(var hook in config.Hooks)
            {
                if (hook.Id != 0)
                {
                    if (hook.Installed)
                    {
                        if (isToUpdate)
                        {
                            hooksToUpdate.Add(hook);
                        }
                    }
                    else
                    {
                        hooksToRemove.Add(hook);
                    }
                }
                else if (hook.Installed)
                {
                    hooksToCreate.Add(hook);
                }
            }

            // Save the config to disk
            config.Save();

            // Process any hooks that we have to: Update, Create or Remove
            if (hooksToUpdate.Count > 0 || hooksToCreate.Count > 0 || hooksToRemove.Count > 0)
            {
                // We are waiting for hook ping requests
                config.BeginHookUpdate();

                var github = await GithubHelper.ConnectAndValidate(config.GitHubAdminApplicationToken, false);

                // Update existing hooks
                if (hooksToUpdate.Count > 0)
                {
                    var hookUpdate = new HookUpdate(true)
                    {
                        Events = new List<string>() {"pull_request"},
                        Config = new Dictionary<string, object>()
                        {
                            {"url", newUrl},
                            {"content_type", "json"}
                        }
                    };
                    if (config.GitHubWebHookCallbackSecret != null)
                    {
                        hookUpdate.Config["secret"] = config.GitHubWebHookCallbackSecret;
                    }

                    foreach (var hook in hooksToUpdate)
                    {
                        try
                        {
                            // Mark the hook as not validated before updating it (so we can get update in the mean time)
                            hook.Installed = false;
                            hook.Validated = false;
                            config.AddHookRequest();
                            await github.Repository.Hooks.Update(hook.Owner, hook.Name, hook.Id, hookUpdate);
                            hook.Installed = true;
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("",
                                string.Format("Error while updating hook [{0}]: {1}", hook, ex.Message));
                        }
                    }
                }

                // Remove existing hooks
                if (hooksToRemove.Count > 0)
                {
                    foreach (var hook in hooksToRemove)
                    {
                        try
                        {
                            await github.Repository.Hooks.Delete(hook.Owner, hook.Name, hook.Id);
                            hook.Id = 0;
                            hook.Installed = false;
                            hook.Validated = false;
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("",
                                string.Format("Error while deleting hook [{0}]: {1}", hook, ex.Message));
                        }
                    }
                }

                // Create new hooks
                if (hooksToCreate.Count > 0)
                {
                    foreach (var hook in hooksToCreate)
                    {
                        try
                        {
                            var newHook = NewHook.CreateWeb(newUrl, "json", "pull_request");
                            newHook.Config["secret"] = config.GitHubWebHookCallbackSecret;

                            hook.Installed = false;
                            hook.Validated = false;
                            config.AddHookRequest();
                            var githubHook = await github.Repository.Hooks.Create(hook.Owner, hook.Name, newHook);
                            hook.Id = githubHook.Id;
                            hook.Installed = true;
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("",
                                string.Format("Error while creating hook [{0}]: {1} (Maybe [{2}] is an invalid url?)", hook, ex.Message, newUrl));
                        }
                    }
                }

                // Wait for hook to be received (Cheap/unreliable way to wait for ping request)
                config.EndHookUpdate();
            }

            // Save to disk
            config.Save();
        }
    }
}
