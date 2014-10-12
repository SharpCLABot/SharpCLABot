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
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using Octokit.Internal;

namespace SharpCLABot.Helpers
{
    /// <summary>
    /// Helpers used to connect/interact with github.
    /// </summary>
    internal static class GithubHelper
    {
        public static GitHubClient Connect(string token)
        {
            return new GitHubClient(new ProductHeaderValue("SharpCLABot"),
                new InMemoryCredentialStore(new Credentials(token)));
        }

        public static async Task<IssueComment> PostCommentToContributor(this GitHubClient gitHubClient, Contributor contributor, string comment)
        {
            if (contributor == null) throw new ArgumentNullException("contributor");
            if (comment == null) throw new ArgumentNullException("comment");
            if (contributor.IsFromPullRequest)
            {
                return await gitHubClient.Issue.Comment.Create(contributor.PullRequestRepositoryOwner,
                    contributor.PullRequestRepositoryName, contributor.PullRequestNumber,
                    comment);
            }
            return null;
        }

        /// <summary>
        /// Gets all hooks for callback.
        /// </summary>
        /// <param name="github">The github.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>Task&lt;List&lt;Tuple&lt;Repository, Hook&gt;&gt;&gt;.</returns>
        /// <exception cref="ArgumentNullException">github</exception>
        public static async Task<List<Tuple<Repository, Hook>>> GetAllHooksForCallback(GitHubClient github, string callback)
        {
            if (github == null) throw new ArgumentNullException("github");


            var allRepositories = await GetAllRepositories(github);
            var registeredHooks = new List<Tuple<Repository, Hook>>();

            foreach (var repo in allRepositories)
            {
                try
                {
                    var hooks = await github.Repository.Hooks.GetAll(repo.Owner.Login, repo.Name);
                    Hook registeredHook = null;
                    foreach (var hook in hooks)
                    {
                        object callbackUrl;
                        if (hook.Config.TryGetValue("url", out callbackUrl) && callbackUrl != null &&
                            string.CompareOrdinal(callbackUrl.ToString(), callback) == 0)
                        {
                            registeredHook = hook;
                            break;
                        }
                    }

                    registeredHooks.Add(new Tuple<Repository, Hook>(repo, registeredHook));
                }
                catch (Exception)
                {
                }
            }

            return registeredHooks;
        }

        public static async Task<List<Repository>> GetAllRepositories(GitHubClient github)
        {
            if (github == null) throw new ArgumentNullException("github");

            var allRepositories = new List<Repository>();

            // Retrieve all repo for current user
            var orgas = await github.Organization.GetAllForCurrent();
            foreach (var org in orgas)
            {
                var orgaRepos = await github.Repository.GetAllForOrg(org.Login);
                foreach (var repo in orgaRepos)
                {
                    if (allRepositories.All(item => item.Id != repo.Id))
                    {
                        allRepositories.Add(repo);
                    }
                }
            }

            var repositoies = await github.Repository.GetAllForCurrent();
            foreach (var repo in repositoies)
            {
                if (allRepositories.All(item => item.Id != repo.Id))
                {
                    allRepositories.Add(repo);
                }
            }

            return allRepositories;
        }

        public static bool CheckTokenConfigured(string token)
        {
            var config = AdminConfig.Instance;
            if (!string.IsNullOrWhiteSpace(config.GitHubAdminApplicationToken))
            {
                var isValidToken = config.GitHubAdminApplicationToken == token;
                if (!isValidToken)
                {
                    throw new InvalidOperationException(
                        "Token is not matching currently configured token. You can't access to the configuration");
                }
                return true;
            }
            return false;
        }

        public static async Task<GitHubClient> ConnectAndValidate(string token, bool forceCheck)
        {
            var githubTemp = Connect(token);
            if (!forceCheck)
            {
                return githubTemp;
            }

            try
            {
                // Check that we can access user
                var user = await githubTemp.User.Current();
                try
                {
                    // Check that we have access to the repositories, notifications and read:org access
                    var repositories = await githubTemp.Repository.GetAllForCurrent();
                    var notifications = await githubTemp.Notification.GetAllForCurrent();
                    var orgas = await githubTemp.Organization.GetAllForCurrent();

                    // Save the token to the settings files
                    if (string.IsNullOrWhiteSpace(AdminConfig.Instance.GitHubAdminApplicationToken))
                    {
                        // Save the setting for the first time
                        AdminConfig.Instance.GitHubAdminApplicationToken = token;
                        AdminConfig.Instance.Save();
                    }

                    return githubTemp;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Token has not enough privileges. Requiring scope access to: [repo, notifications, read:org, user]", ex);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid Token. Unable to connect", ex);
            }
        }
    }
}
