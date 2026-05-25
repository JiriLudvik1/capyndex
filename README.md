# Capyndex - High-Performance Search Engine Core

Capyndex is a high-performance, lightweight search engine backend built with .NET 9. It implements an inverted index using Redis for extremely fast full-text search capabilities and PostgreSQL for persistent document storage.

## Core Technical Functionality

### 1. Inverted Index Architecture
The core of Capyndex is a custom-built inverted index stored in Redis. 
- **Indexing**: Documents are tokenized into terms. For each term, a Redis Hash is maintained (`index:{term}`) where the fields are Document IDs and the values are the term frequencies within that document.
- **Full-Text Search**: Searches are performed by tokenizing the query and performing an intersection of document sets from the corresponding Redis hashes.

### 2. Performance Optimizations
The engine is designed with a heavy focus on memory efficiency and throughput:
- **Low Allocation**: Extensive use of `Span<char>`, `stackalloc`, and pre-allocated buffers to minimize GC pressure during indexing.
- **Redis Batching**: Pipeline-based Redis commands to reduce network round-trips when processing multi-term queries.
- **Efficient Intersections**: Set intersections are performed on strings before converting to `Guid`, minimizing object allocations.
- **Pool Management**: Optimized PostgreSQL connection pooling and retry policies.

### 3. API Design (FastEndpoints)
The project utilizes **FastEndpoints**, a high-performance alternative to Minimal APIs and MVC, providing:
- Vertical Slice Architecture (Feature-based organization).
- Native source generator support for discovery and routing.
- Schema-first design with built-in Swagger/OpenAPI support.

### 4. Search Features
- **Full-Text Search**: Multi-term search with set intersection.
- **Term Frequency**: Retrieve exact frequency scores for terms across documents.
- **Exact DB Search**: Fallback or specialized search directly against the PostgreSQL database.
- **Document Management**: Endpoints for uploading, retrieving, and rebuilding the index.

## Technology Stack
- **Framework**: .NET 9 (ASP.NET Core)
- **API Framework**: FastEndpoints
- **Primary Storage**: PostgreSQL (via Entity Framework Core)
- **Indexing/Caching**: Redis (via StackExchange.Redis)
- **Authentication**: JWT Bearer
- **Documentation**: Swagger/OpenAPI

## Project Structure
- `Capyndex/Source/Features`: Vertical slices containing endpoint definitions and logic.
- `Capyndex/Source/Services`: Core logic for indexing (`RedisIndexService`), tokenization (`Tokenizer`), and search orchestration.
- `Capyndex/Source/Database`: EF Core context and migrations.

## Getting Started

### Prerequisites
- .NET 9 SDK
- Redis (running on `localhost:6379`)
- PostgreSQL

### Configuration
Update `appsettings.json` with your PostgreSQL connection string and JWT signing key:
```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Database=capyndex;Username=postgres;Password=..."
  },
  "Auth": {
    "JwtKey": "YourVeryLongSecretKeyHere"
  }
}
```

### Running the Project
```bash
cd Capyndex/Capyndex/Source
dotnet run
```
The Swagger UI will be available at the root URL for API exploration.
