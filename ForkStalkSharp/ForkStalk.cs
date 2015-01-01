//
// ForkStalk.cs
//
// Author:
//       Marius Ungureanu <therzok@gmail.com>
//
// Copyright (c) 2015 Marius Ungureanu
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace ForkStalkSharp
{
	public class ForkStalk
	{
		/// <summary>
		/// Gets or sets the username to use for API authentication.
		/// </summary>
		/// <value>The username.</value>
		public string Username { get; set; }
		/// <summary>
		/// Gets or sets the password to use for API authentication.
		/// </summary>
		/// <value>The account password. The generated token for 2FA.</value>
		public string Password { get; set; }
		/// <summary>
		/// Gets or sets the time since the repository must have changed.
		/// </summary>
		/// <value>The since UTC.</value>
		public DateTimeOffset SinceUtc { get; set; }

		int forkCount;
		/// <summary>
		/// Initializes a new instance of the <see cref="ForkStalkSharp.ForkStalk"/> class.
		/// </summary>
		/// <param name="latestForkCount">Latest number of forks to display the result for.</param>
		public ForkStalk (int latestForkCount)
		{
			forkCount = latestForkCount;
		}
		/// <summary>
		/// Queries for the result.
		/// </summary>
		/// <returns>The result which contains the information on the fork commits.</returns>
		/// <param name="owner">The repository owner.</param>
		/// <param name="name">The repository name.</param>
		public IEnumerable<ForkStalkResult> GetForks (string owner, string name)
		{
			// Initialize Github query settings.
			var conn = new Connection (new ProductHeaderValue ("ForkSharp", "1.0"));
			if (!string.IsNullOrEmpty (Username) && !string.IsNullOrEmpty (Password))
				conn.Credentials = new Credentials (Username, Password);

			var client = new GitHubClient (conn);
			var api = new ApiConnection (conn);
			var cr = new CommitRequest { Since = SinceUtc };

			// Get the main repository's default branch.
			var defaultBranchTask = client.Repository.Get (owner, name);

			// Get the main repository's commits in the default branch.
			var commitsTask = client.Repository.Commits.GetAll (owner, name, cr);

			// Get the main repository's branches.
			var branchesTask = client.Repository.GetAllBranches (owner, name);

			// Get the main repository's forks.
			var forksTask = api.GetAll<Repository> (GetForkAPIUrl (owner, name));

			// Get the main repository's pull requests.
			var pullRequestsTask = client.PullRequest.GetForRepository (owner, name);

			Task.WaitAll (defaultBranchTask, commitsTask, branchesTask, forksTask, pullRequestsTask);

			// Start getting the data.
			var interestingForks = new Dictionary<string, ForkStalkResult> (forkCount);
			var defaultBranch = defaultBranchTask.Result.DefaultBranch;
			var branches = branchesTask.Result.ToDictionary (b => b.Name, b => b.Commit);
			var commits = new HashSet<string> (commitsTask.Result.Select(c => c.Sha));
			var forks = forksTask.Result.Where (f => f.PushedAt >= SinceUtc)
				.OrderByDescending (f => f.PushedAt)
				.Take (forkCount);
			var pullRequests = pullRequestsTask.Result.ToDictionary (p => string.Format("{0}/{1}",
				p.User.Login,
				p.Head.Ref));

			// Look for new commits.
			Parallel.ForEach (forks, fork => {
				var forkBranches = client.Repository.GetAllBranches (fork.Owner.Login, fork.Name).Result;
				Parallel.ForEach(forkBranches, branch => {
					// Check for any open pull request.
					if (pullRequests.ContainsKey (string.Format ("{0}/{1}", fork.Owner.Login, branch.Name)))
						return;

					var forkCommit = client.Repository.Commits.Get (fork.Owner.Login, fork.Name, branch.Name).Result;
					// Skip commits which happened before the requested time.
					if (forkCommit.Commit.Author.Date < SinceUtc)
						return;

					// Filter out branches which are not the default one.
					if (branch.Name != defaultBranch) {
						// Check for branches which exist in the main repository and have the same tip.
						if (branches.ContainsKey (branch.Name) && branches [branch.Name].Sha == branch.Commit.Sha)
							return;
					}

					// If the branch is merged into mainline, skip.
					if (commits.Contains (branch.Commit.Sha))
						return;

					// Set the branch as interesting.
					lock (interestingForks) {
						if (!interestingForks.ContainsKey (fork.FullName))
							interestingForks.Add (fork.FullName, new ForkStalkResult (fork.FullName));

						interestingForks [fork.FullName].BranchList.Add (new BranchResult {
							Name = branch.Name,
							LastModified = forkCommit.Commit.Author.Date
						});
					}
				});
			});

			return interestingForks.Values;
		}

		// Workaround missing API in Octokit.
		static Uri GetForkAPIUrl (string owner, string name)
		{
			return new Uri (string.Format (CultureInfo.InvariantCulture, "repos/{0}/{1}/forks", owner, name), UriKind.Relative);
		}
	}
}

