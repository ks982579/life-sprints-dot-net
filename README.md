# Life Sprints

> A DotNet - React web application

Life Sprint web application with Dot-Net backend and React frontend.

I will create a better README and Checklist as I go (hopefully) but just need to get ideas out.

I have ADHD and found a system that kind of helps me get things done.
I call it "life-sprints", based on the Agile-Scrum methodology.
I use the Obsidian markdown editor currently, which is cool.
However, I have an Annual backlog, monthly backlog, and weekly sprint.
Using check-boxes, when I complete a task, I need to check it off on three pages.
I want to create and deploy a web application that has these Check-List powers using C# and the .NET framework for the backend and React on the frontend.
These tools are chosen because they are popular, performant, and I use them at work - so familiarity and practice.
I have a digital Ocean account, so I'll docker-ize the application and deploy there I think at the end.

My first local user:
"fb5bebcb-9d01-4286-be78-e0f6d13f5bd4"

## Quick Use

```bash
docker-compose up -d
dotnet run --project back/src/LifeSprints.Api
```

You now have a `http://localhost:5089/swagger` endpoint!

```bash
docker exec -i lifesprint_postgres psql -U lifesprint_user -d lifesprint_db < database/init/01_schema.sql
docker exec -i lifesprint_postgres psql -U lifesprint_user -d lifesprint_db < database/init/02_stored_procedures.sql
```

Sometimes you might need to clean out the volumes... maybe make a spare?

```bash
docker-compose down -v # v for volume
docker exec lifesprint_postgres psql -U lifesprint_user -d lifesprint_db -c "SELECT routine_name FROM information_schema.routines WHERE routine_name LIKE 'sp_create%' OR routine_name LIKE 'sp_toggle%' OR routine_name LIKE 'sp_get%';"
# gives fancy row of relations.
docker exec lifesprint_postgres psql -U lifesprint_user -d lifesprint_db -c "\dt"
```

For TESTING!

```bash
dotnet new xunit -n LifeSprints.Tests -o tests/LifeSprints.Tests
cd ./back/tests/LifeSprints.Tests && dotnet add reference ../../src/LifeSprints.Api/LifeSprints.Api.csproj
dotnet add package Moq
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Testcontainers.PostgreSql
dotnet test tests/LifeSprints.Tests/LifeSprints.Tests.csproj --verbosity normal
```

✅ Test Setup Complete:

- xUnit test project with proper references
- Moq for mocking dependencies
- TestContainers.PostgreSQL for real database testing
- ASP.NET Core testing for integration tests

✅ Test Coverage:

1. StoredProcedureServiceTests - Unit tests with real PostgreSQL database
2. StoriesControllerIntegrationTests - End-to-end API testing

✅ Test Features:

- Real PostgreSQL containers for isolated testing
- Database schema/stored procedure setup per test
- CRUD operations testing
- Error handling validation
- Integration testing with HTTP client

```bash
cd back
dotnet test
```

## Kanban Board

Ironically using Kanban because I cannot dedicate consistent time each week.

### To Do

- [ ] React Frontend - Design / Implement / Test / Deploy
- [ ] integration tests

### In Progress

- [ ] Adding Unit Tests
- [ ] Analysing next Steps

### Done

- [x] System Design - Rough
- [x] Database - docker-ized
- [x] Initial SQL Stored procedures
- [x] Complete initial data classes and models
- [x] complete initial APIs
- [ ]
- [ ]
- [ ]
- [ ]
- [ ]
- [ ]
