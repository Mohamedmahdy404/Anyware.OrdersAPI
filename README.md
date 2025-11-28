# Anyware Software Backend Exam

## Overview
This repository contains a high-quality implementation of the technical exam for Backend Developer (.NET + Redis) at Anyware Software.

## Features
- Build a Simple Orders API (CRUD)
- Redis cache for GET by ID (5 min TTL)
- EF Core DB with migrations
- Clean architecture
- Dependency injection
- Async/await usage
- Logging & error handling

## Endpoints
- **POST /orders** – Create an order
- **GET /orders/{id}** – Fetch order, uses Redis (5 min TTL)
- **GET /orders** – List all orders
- **DELETE /orders/{id}** – Delete order (DB & Redis)

## Model
- `OrderId` (Guid)
- `CustomerName` (string)
- `Product` (string)
- `Amount` (decimal)
- `CreatedAt` (DateTime)

## Running locally
1. Make sure you have .NET 8 SDK, SQL Server, and Redis running
2. Update `appsettings.json` connection strings if needed
3. Run migrations:
    ```bash
    dotnet ef migrations add InitialCreate
    dotnet ef database update
    ```
4. Run API:
    ```bash
    dotnet run --project Anyware.OrdersAPI.API
    ```

## Conceptual Questions
See `ConceptualQuestions.md` for detailed answers to the required questions.
