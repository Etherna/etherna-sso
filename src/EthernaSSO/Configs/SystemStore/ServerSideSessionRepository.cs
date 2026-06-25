// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
// 
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Conventions;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.MongODM.Core.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.SystemStore
{
    internal sealed class ServerSideSessionRepository : IServerSideSessionStore
    {
        // Fields.
        private readonly IMongoCollection<ServerSideSession> collection;

        // Constructor.
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
        public ServerSideSessionRepository(DbContextOptions options, string name)
        {
            ArgumentNullException.ThrowIfNull(options);

            // Register class map. (see: https://etherna.atlassian.net/browse/ESSO-140)
            BsonClassMap.RegisterClassMap<ServerSideSession>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(x => x.Key);
            });

            // Register discriminator convention. Default from MongODM doesn't work without DbContext.
            BsonSerializer.RegisterDiscriminatorConvention(typeof(ServerSideSession),
                StandardDiscriminatorConvention.Hierarchical);

            // Initialize MongoDB driver.
            var client = new MongoClient(options.ConnectionString);
            var database = client.GetDatabase(options.DbName);
            collection = database.GetCollection<ServerSideSession>(name);
        }

        // Methods.
        public Task CreateSessionAsync(ServerSideSession session, CancellationToken cancellationToken = default) =>
            collection.InsertOneAsync(session, cancellationToken: cancellationToken);

        public Task DeleteSessionAsync(string key, CancellationToken cancellationToken = default) =>
            collection.DeleteOneAsync(x => x.Key == key, cancellationToken: cancellationToken);

        public Task DeleteSessionsAsync(SessionFilter filter, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(filter);
            filter.Validate();

            return collection.DeleteManyAsync(BuildMongoFilterHelper(filter), cancellationToken);
        }

        public async Task<IReadOnlyCollection<ServerSideSession>> GetAndRemoveExpiredSessionsAsync(int count, CancellationToken cancellationToken = default)
        {
            var results = await collection.AsQueryable()
                .Where(x => x.Expires < DateTime.UtcNow)
                .OrderBy(x => x.Key)
                .Take(count)
                .ToListAsync(cancellationToken);

            if (results.Count == 0)
                return [];
            
            await collection.DeleteManyAsync(
                Builders<ServerSideSession>.Filter.In(x => x.Key, results.Select(x => x.Key)), cancellationToken);
            
            return results;
        }

        public async Task<ServerSideSession?> GetSessionAsync(string key, CancellationToken cancellationToken = default) =>
            await collection.AsQueryable()
                .SingleOrDefaultAsync(x => x.Key == key, cancellationToken: cancellationToken);

        public async Task<IReadOnlyCollection<ServerSideSession>> GetSessionsAsync(SessionFilter filter, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(filter);
            filter.Validate();

            var cursor = await collection.FindAsync(BuildMongoFilterHelper(filter), cancellationToken: cancellationToken);
            return await cursor.ToListAsync(cancellationToken: cancellationToken);
        }

        public Task<QueryResult<ServerSideSession>> QuerySessionsAsync(CancellationToken cancellationToken, SessionQuery? filter = null)
        {
            filter ??= new();

            // The keys of first and last items in the prior results stored as "x,y" in the filter.ResultsToken.
            var first = string.Empty;
            var last = string.Empty;

            if (filter.ResultsToken != null)
            {
                var parts = filter.ResultsToken.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    first = parts[0];
                    last = parts[1];
                }
            }

            var countRequested = filter.CountRequested;
            if (countRequested <= 0)
                countRequested = 25;

            var query = collection.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.DisplayName) ||
                !string.IsNullOrWhiteSpace(filter.SubjectId) ||
                !string.IsNullOrWhiteSpace(filter.SessionId))
            {
                query = query.Where(x =>
                    (filter.SubjectId == null || x.SubjectId.Contains(filter.SubjectId)) &&
                    (filter.SessionId == null || x.SessionId.Contains(filter.SessionId)) &&
                    (filter.DisplayName == null || (x.DisplayName != null && x.DisplayName.Contains(filter.DisplayName) == true))
                );
            }

            var totalCount = query.Count();
            var totalPages = (int)Math.Max(1, Math.Ceiling(totalCount / (countRequested * 1.0)));

            var currPage = 1;

            var hasNext = false;
            var hasPrev = false;
            ServerSideSession[] items;

            if (filter.RequestPriorResults)
            {
                // Sets query at the prior record from the last results, but in reverse order.
                items = query.OrderByDescending(x => x.Key)
                    .Where(x => string.Compare(x.Key, first, StringComparison.Ordinal) < 0)
                    // and we +1 to see if there's a prev page
                    .Take(countRequested + 1)
                    .ToArray();

                // put them back into ID order
                items = items.OrderBy(x => x.Key).ToArray();

                // if we have the one extra, we have a prev page
                hasPrev = items.Length > countRequested;

                if (hasPrev)
                {
                    // omit prev results entry
                    items = items.Skip(1).ToArray();
                }

                // how many are to the right of these results?
                if (items.Length > 0)
                {
                    var postCountId = items[^1].Key;
                    var postCount = query.Count(x => string.Compare(x.Key, postCountId, StringComparison.Ordinal) > 0);
                    hasNext = postCount > 0;
                    currPage = totalPages - (int)Math.Ceiling((1.0 * postCount) / countRequested);
                }

                if (currPage == 1 && hasNext && items.Length < countRequested)
                {
                    // this handles when we went back and are now at the beginning but items were deleted.
                    // we need to start over and re-query from the beginning.
                    filter.ResultsToken = null;
                    filter.RequestPriorResults = false;
                    return QuerySessionsAsync(cancellationToken, filter);
                }
            }
            else
            {
                items = query.OrderBy(x => x.Key)
                    // if last is "", then this will just start at beginning
                    .Where(x => string.Compare(x.Key, last, StringComparison.Ordinal) > 0)
                    // and we +1 to see if there's a next page
                    .Take(countRequested + 1)
                    .ToArray();

                // if we have the one extra, we have a next page
                hasNext = items.Length > countRequested;

                if (hasNext)
                {
                    // omit next results entry
                    items = items.SkipLast(1).ToArray();
                }

                // how many are to the left of these results?
                if (items.Length > 0)
                {
                    var priorCountId = items[0].Key;
                    var priorCount = query.Count(x => string.Compare(x.Key, priorCountId, StringComparison.Ordinal) < 0);
                    hasPrev = priorCount > 0;
                    currPage = 1 + (int)Math.Ceiling((1.0 * priorCount) / countRequested);
                }
            }

            // this handles prior entries being deleted since paging begun
            if (currPage <= 1)
            {
                currPage = 1;
                hasPrev = false;
            }

            string? resultsToken = null;
            if (items.Length > 0)
            {
                resultsToken = $"{items[0].Key},{items[^1].Key}";
            }
            else
            {
                hasPrev = false;
                hasNext = false;
                totalCount = 0;
                totalPages = 0;
                currPage = 0;
            }

            var result = new QueryResult<ServerSideSession>
            {
                ResultsToken = resultsToken!, //can be null
                HasNextResults = hasNext,
                HasPrevResults = hasPrev,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = currPage,
                Results = items
            };

            return Task.FromResult(result);
        }

        public Task UpdateSessionAsync(ServerSideSession session, CancellationToken cancellationToken = default) =>
            collection.ReplaceOneAsync(x => x.Key == session.Key, session, cancellationToken: cancellationToken);
        
        // Helpers.
        private static FilterDefinition<ServerSideSession> BuildMongoFilterHelper(SessionFilter sourceFilter)
        {
            var fieldFilters = new List<FilterDefinition<ServerSideSession>>();

            if (!string.IsNullOrWhiteSpace(sourceFilter.SessionId))
                fieldFilters.Add(Builders<ServerSideSession>.Filter.Eq(x => x.SessionId, sourceFilter.SessionId));

            if (!string.IsNullOrWhiteSpace(sourceFilter.SubjectId))
                fieldFilters.Add(Builders<ServerSideSession>.Filter.Eq(x => x.SubjectId, sourceFilter.SubjectId));

            return Builders<ServerSideSession>.Filter.And(fieldFilters);
        }
    }
}