# Conceptual Questions

1. **Redis vs SQL – key differences**
   - Redis is an in-memory key-value store, mainly used for caching, session storage, and fast data access. It’s extremely fast because data lives in memory, but it’s not ideal for complex queries or long-term persistence.
SQL databases like SQL Server or PostgreSQL are relational and disk-based, supporting complex queries, joins, and transactions. They ensure strong consistency and are best for structured, persistent data.
So, I’d use Redis for speed and temporary data, and SQL for data integrity and relational queries.

2. **When not to use caching?**
   - I’d avoid caching when data changes very frequently or when stale data could cause issues like real-time financial transactions. Also, if the dataset is small or queries are already fast, caching might add unnecessary complexity.
     
3. **What if Redis is down?**
   - With Redis down, the API gracefully falls back to fetching from the database. 
   - In my implementation, all cache failures are logged as warnings (not errors) and don't break the API.
   - This ensures high availability only performance degrades, not functionality.
   - The system continues to function normally, just without caching benefits.
     
4. **Optimistic vs pessimistic locking?**
   - Optimistic locking assumes conflicts are rare. It lets multiple users read the same data, and when updating, it checks if the data has changed. If it has, the update fails, and the user must retry.
   - Pessimistic locking assumes conflicts are likely. It locks the record as soon as someone reads or starts editing it, preventing others from changing it until the lock is released.
In .NET, optimistic locking can be implemented using row version columns in the Entity Framework, while pessimistic uses database-level locks.

5. **Ways to scale a .NET backend?**
   - Add more CPU/RAM to the server.
   - Run multiple instances behind a load balancer.
   - Caching: Use Redis or MemoryCache to reduce DB load.
   - Async and background processing: Offload heavy tasks to queues
