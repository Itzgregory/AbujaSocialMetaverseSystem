.PHONY: build run clean test restore

# Default target
all: build

# Build the entire solution
build:
	dotnet build AbujaSocialMetaverse.slnx

# Run the API project without rebuilding
run:
	dotnet run --project src/AbujaSocialMetaverse.API/AbujaSocialMetaverse.API.csproj --no-build

# Clean the solution
clean:
	dotnet clean AbujaSocialMetaverse.slnx

# Run all tests in the solution
test:
	dotnet test AbujaSocialMetaverse.slnx

# Restore NuGet packages
restore:
	dotnet restore AbujaSocialMetaverse.slnx
