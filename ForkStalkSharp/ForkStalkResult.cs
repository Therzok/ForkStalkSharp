//
// ForkStalkResult.cs
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

using System.Collections.Generic;

namespace ForkStalkSharp
{
	public class ForkStalkResult
	{
		/// <summary>
		/// Gets the name of the fork.
		/// </summary>
		/// <value>The name in the format of "owner/name".</value>
		public string ForkName { get; private set; }

		/// <summary>
		/// Gets the fork branches which are not synced with the repository.
		/// </summary>
		/// <value>The branches.</value>
		public IReadOnlyList<string> Branches { get { return BranchList; } }

		internal List<string> BranchList { get; private set; }
		internal ForkStalkResult (string forkName)
		{
			ForkName = forkName;
			BranchList = new List<string> ();
		}
	}
}

