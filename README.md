# Banking System Backend

This project is a banking backend built with **ASP.NET Core**, **Entity Framework Core**, **Oracle Database**, and a **microservices architecture**.

It supports core banking operations such as:

- Creating and managing customers and accounts
- Sending and receiving money
- Card-related operations
- Spending and charge limits
- Authorization records
- Refunds
- Point operations
- Saga-based compensation for distributed transactions

## Technologies

- .NET / ASP.NET Core
- Entity Framework Core
- Oracle Database
- Microservices
- REST APIs

## How to Run

### 1. Configure the database connection

Create an `appsettings.Development.json` file inside each microservice project.

Example:

```json
{
  "ConnectionStrings": {
    "OracleDb": "User Id=...;Password=...;Data Source=...;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 2. Apply database migrations

Run the EF Core migrations for each microservice that owns a database schema.

```dotnet ef database update \
  --project Microservices/Bank.AuthorizationService \
  --startup-project Microservices/Bank.AuthorizationService
```

### 3. Start all required microservices

### 4. Test the API

The request below can be used to test the API:

GET http://localhost:7000/api/account

Note: Requests can be sent using tools such as Postman or curl.


