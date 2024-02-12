#!/bin/bash

dotnet test --collect:"XPlat Code Coverage" 

# required: dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"TestResults/{guid}/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
