# MoviesDB
## Needed tools:
- VisualStudio (development done in VS 2022)
- MongoDB instance

## Environment variables
All variables are defined in appsettings.json:
```
"MongoDBSettings": {
    "ConnectionString": "<connection string to your MongoDB instance>",
    "DatabaseName": "<database name>",
    "CollectionName": "<movie collection name>"
  },
  "PathToJSONData": "<pathToYourDataJson>\\<yourJsonFile>.json"
}
```
Modify these to match your environment.

## How to start the API

Debug > Start Debugging  
Shortcut: `F5`

OR

Debug > Start Without Debugging  
Shortcut: `Ctrl`+`F5`

OR

In cmd, PowerShell or other terminal:

```
cd <your git folder>\MoviesAPI
dotnet run
```

By default the server will run under
```
https://localhost:7108
http://localhost:5108
```

## CORS
The server allows all HTTP methods with all HTTP headers from http://localhost:3000 and Swagger

## Swagger
Swagger instance is automatically started with the server

## Branching
Only one branch used (main).

## Authorization
No authorization method implemented

