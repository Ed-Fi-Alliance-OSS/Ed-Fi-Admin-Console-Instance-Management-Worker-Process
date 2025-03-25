# PGSql
DatabaseEngine=PostgreSQL
PathBase=""
OverrideExistingDatabase=false
IgnoresCertificateErrors=true
EdFi_Master="host=host.docker.internal;port=5432;username=postgres;password=admin;database=postgres;pooling=false"
EdFi_Ods="host=host.docker.internal;port=5432;username=postgres;password=admin;database={0};pooling=false"
AccessTokenUrl="https://host.docker.internal/auth/realms/edfi-admin-console/protocol/openid-connect/token"
AdminConsoleTenantsURL="https://host.docker.internal/adminapi/adminconsole/tenants"
AdminConsoleInstancesURL="https://host.docker.internal/adminapi/adminconsole/instances?status={0}"
AdminConsoleCompleteInstancesURL="https://host.docker.internal/adminapi/adminconsole/instances/{0}/completed"
Username=adminconsole-user
ClientId=admin-console
Password=SomePassword
DatabaseProvider=Npgsql