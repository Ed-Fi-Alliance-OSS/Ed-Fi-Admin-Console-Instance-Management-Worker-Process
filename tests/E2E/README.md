# E2E Tests for Instance Management Worker Process

This folder contains scripts to test the Instance Management Worker Process in an end-to-end scenario using Docker containers.

## How it Works

The E2E tests verify that the Instance Management Worker Process correctly:
1. Processes instance creation requests, creating new ODS databases
2. Updates instance statuses from "Pending" to "Complete"
3. Processes instance deletion requests
4. Updates instance statuses from "Pending_Delete" to "Deleted" or removes them

## Prerequisites

- Docker and Docker Compose
- PowerShell 7.0 or later

## Steps to Run

1. Create your `.env` file using the `.env.example` as a reference:
   ```
   ADMIN_API_URL=http://localhost:8003
   ADMIN_USERNAME=test@ed-fi.org
   ADMIN_PASSWORD=Password123!
   CONNECTION_STRING=Host=localhost;Port=5401;Username=postgres;Password=postgres;Database=EdFi_Admin;
   ```

2. Execute the `e2eTest.ps1` script, which will:
   - Start the Docker environment (using the `../docker/start.ps1` script)
   - Wait for Admin API to be ready
   - Create a test instance with "Pending" status
   - Run the worker process to create the instance
   - Verify the instance was created successfully
   - Test instance deletion (unless `-SkipDeleteTest` is specified)
   - Stop and clean up the Docker environment (unless `-SkipTearDown` is specified)

## Command-Line Options

You can run the script with the following options:

- `-SkipTearDown`: Skip tearing down the Docker environment after the test (useful for debugging)
- `-SkipDeleteTest`: Skip testing the instance deletion functionality

```powershell
./e2eTest.ps1
./e2eTest.ps1 -SkipTearDown
./e2eTest.ps1 -SkipDeleteTest
./e2eTest.ps1 -SkipTearDown -SkipDeleteTest
```

## Files in this Directory

- `e2eTest.ps1` - The main test script
- `e2eTest-helpers.psm1` - PowerShell module with helper functions
- `.env.example` - Example environment variables file
- `README.md` - This documentation file

## Troubleshooting

If the tests fail:

1. Check if the Docker containers are running:
   ```powershell
   docker ps
   ```

2. Check the Admin API logs:
   ```powershell
   docker logs ed-fi-adminapi
   ```

3. Verify the environment variables in `.env` are correct

4. Try running with `-SkipTearDown` to keep the environment running for manual inspection
