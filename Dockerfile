# STEP 1: Build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file and all referenced projects
COPY TTT2.sln ./
COPY TTT2 ./TTT2
COPY Shared ./Shared

WORKDIR /src/TTT2
RUN dotnet publish -c Release -o /app/publish

# STEP 2: Run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TTT2.dll"]