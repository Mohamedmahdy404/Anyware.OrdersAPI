# Conceptual Questions

1. **Redis vs SQL – key differences**
   - Redis is an in-memory key-value store, extremely fast for caching and real-time scenarios. SQL databases are persistent, offer relational data, transactions, queries, and durability.
2. **When not to use caching?**
   - Don't cache rapidly changing data, highly sensitive info, or content where consistency is essential at all times. Example: financial transactions, user authentication states.
3. **What if Redis is down?**
   - With Redis down, the API should gracefully fallback to fetching from the database. This way, only caching benefits are lost; data remains accessible. Log Redis exceptions and monitor recovery.
4. **Optimistic vs pessimistic locking?**
   - *Optimistic locking* assumes no conflict, checks version/timestamp on save; fails if changed. *Pessimistic locking* locks records for a transaction, preventing others from updating simultaneously.
5. **Ways to scale a .NET backend?**
   - Horizontal scaling with more API instances
   - API Gateway with load balancing
   - Distributed caching
   - Database sharding/replication
   - Asynchronous messaging (queues)
   - Microservices architecture
