#!/bin/bash

# Create the solution and project structure

echo "Creating Life Sprint .NET solution..."

# Create solution

dotnet new sln -n LifeSprint

# Create Web API project

dotnet new webapi -n LifeSprint.Api -o src/LifeSprint.Api

# Create class library for data access

dotnet new classlib -n LifeSprint.Data -o src/LifeSprint.Data

# Create class library for models

dotnet new classlib -n LifeSprint.Models -o src/LifeSprint.Models

# Add projects to solution

dotnet sln add src/LifeSprint.Api/LifeSprint.Api.csproj
dotnet sln add src/LifeSprint.Data/LifeSprint.Data.csproj
dotnet sln add src/LifeSprint.Models/LifeSprint.Models.csproj

# Add project references

dotnet add src/LifeSprint.Api reference src/LifeSprint.Data
dotnet add src/LifeSprint.Api reference src/LifeSprint.Models
dotnet add src/LifeSprint.Data reference src/LifeSprint.Models

# Add required NuGet packages

cd src/LifeSprint.Api
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Cors
dotnet add package Swashbuckle.AspNetCore

cd ../LifeSprint.Data
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql

cd ../LifeSprint.Models

# No additional packages needed for models

cd ../../

echo "Project structure created successfully!"
echo "Next steps:"
echo "1. Start PostgreSQL: docker-compose up -d"
echo "2. Run the API: cd src/LifeSprint.Api && dotnet run"
echo "OR Run the API: dotnet run --project ./src/LifeSprint.Api"
