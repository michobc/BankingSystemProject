# Project: Banking System API
## Objective
Build a full API using all the knowledge acquired during this module

## Introduction
Build a Baking System API while taking the following into consideration:

- The system is used across different branches
  - Each branch can see only their data
  
- An employee can create accounts for a customer
  - A customer can have a maximum of 5 accounts across all branches.
  - A customer can add a transaction to one of his accounts:
    - Transactions can be a withdrawal or a deposit
    - An employee can add a recurrent transaction and link it to a client account.

- The following user roles are available:
  - Admin
    - An admin has full access to all data.
      
  - Employee
    - An employee has write access to the branch where he’s employed.
    - An employee has only read access to other branches' data.
      
  - Customer
    - A Customer only has access to
    - Read his accounts (can’t create a new account).
    - Create/read transactions.
      
- Extra
Include event sourcing allowing the admin to roll back the transactions of a specific day. He should be able to roll
back all the database transactions or filter for specific transactions of a specific account.


# Solution

## keycloak configuration:
- added role and branchId in jwt token
  - branchId is an attribute user input at signup (he will register to a specific branch of the bank)
  - role is also assigned to the user at signup
![alt text](images/img1.png)

## dbContext and database:
dotnet ef dbcontext scaffold "Host=localhost;Database=bankingsystemdb;Username=postgres;Password=<pass>" Npgsql.EntityFrameworkCore.PostgreSQL -o Models -c BankingSystemContext 
#### I want to check the tenant ID only on application startup and not on every request:
steps:
 - Initializing Tenant Information at Startup: During application startup, we
retrieve the tenant information from the database and store it in a static or singleton service. 
This approach avoids querying the database on every request.

 - Using a Singleton Service: Created a singleton service that holds the tenant information. (ITenantService)
This service can be injected into your DbContext and used to set the schema for each request.

- BankingSystemContextFactory is implmented with a mock service only to apply some migrations.

# Michel Bou Chahine
## inmind.ai