## Description

The playground for C# and Go languages. 

https://github.com/trydex/CodeRunner/assets/20781342/a5a954ed-9d0c-42c5-bc67-c9636f8cc4eb


## How to run locally

Clone the repository and run the command in the repo's directory:
> docker compose up

Wait for all of the containers to start then go to http://localhost:8080 in a browser.

## Used techs

 - C# / .NET 7
 - ASP.NET Core Minimal API and [modular approach](https://timdeschryver.dev/blog/maybe-its-time-to-rethink-our-project-structure-with-dot-net-6#an-api-with-controllers)
 - Quartz.NET
 - Redis
 - Kafka
 - MongoDB
 - Vue.js
 - Docker

Some of the technologies listed above are used as "tech for tech's sake" :)

# Project structure
The project is created using a microservice approach with the Database-per-Service Pattern

![Project structure](https://github.com/trydex/CodeRunner/assets/20781342/04d746fd-68d4-4e35-b970-3fd8ce0f63cb)
