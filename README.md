ForkStalkSharp
==============

Just some stalking project.

It takes a repository owner and name and returns information about updated forks.

Useful for tracking forks which have contributions and have not been upstreamed yet
(merged or had a pull request opened).

Notes:
======
 * It does a lot of queries. Currently, forks count isn't limited on the API side,
so if the repository has many forks, it will end up receiving data for all of them.
 * The queries are done on the non-authenticated rate limit or the user's own limit.
