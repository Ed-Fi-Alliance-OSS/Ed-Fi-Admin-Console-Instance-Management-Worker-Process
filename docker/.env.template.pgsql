# PGSql
DATABASE_ENGINE=PostgreSQL
PATHBASE=""
OVERRIDE_EXISTING_DATABASE=false
IGNORES_CERTIFICATE_ERRORS=true
EDFI_MASTER="host=host.docker.internal;port=5432;username=postgres;password=admin;database=postgres;pooling=false"
EDFI_ODS="host=host.docker.internal;port=5432;username=postgres;password=admin;database={0};pooling=false"
ACCESS_TOKEN_URL="https://host.docker.internal/auth/realms/edfi-admin-console/protocol/openid-connect/token"
ADMINCONSOLE_TENANTS_URL="https://host.docker.internal/adminapi/adminconsole/tenants"
ADMINCONSOLE_INSTANCES_URL="https://host.docker.internal/adminapi/adminconsole/instances?status={0}"
ADMINCONSOLE_COMPLETE_INSTANCES_URL="https://host.docker.internal/adminapi/adminconsole/instances/{0}/completed"
USER_NAME=
CLIENTID=adminconsole-worker-client
CLIENT_SECRET=7tpYh5eZtL0ct99cmfCXUY3q5o2KxUTU
PASSWORD=
GRANTTYPE=client_credentials
SCOPE=edfi_admin_api/full_access