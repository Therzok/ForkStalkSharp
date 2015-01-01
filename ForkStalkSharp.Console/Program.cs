//
// Program.cs
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

namespace ForkStalkSharp.Console
{
	class MainClass
	{
		// Set these if you want to bypass per IP rate limit and use per account one.
		const string username = "";
		const string password = "";

		// Change this to query another repository.
		const string owner = "therzok";
		const string name = "ForkStalkSharp";

		// To demonstrate example usage.
		public static void Main ()
		{
			var stalk = new ForkStalk (20) {
				SinceUtc = DateTimeOffset.UtcNow.AddDays(-30),
				Username = username,
				Password = password,
			};

			var forks = stalk.GetForks (owner, name);

			foreach (var result in forks) {
				System.Console.WriteLine (result.ForkName);
				foreach (var branch in result.Branches)
					System.Console.WriteLine ("\t{0} - {1}", branch.CompareLink, branch.LastModified);
			}
		}
	}
}
