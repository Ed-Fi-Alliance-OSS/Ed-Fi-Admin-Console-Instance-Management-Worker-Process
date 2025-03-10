# Ed-Fi-Admin-Console-Instance-Management-Worker-Process

At this point the project doesn't have e2e tests, so this documentation will help developers learn how to setup an environment and how to run some manual tests.

## You will need:

### Admin API up and running using KeyCloak authentication. 

On Admin Api repo, you can go to `Docker\Compose\pgsql` and run a command like this one.

```powershell
docker compose -f .\MultiTenant\compose-build-idp-dev-multi-tenant.yml --env-file .env  up -d
```

Make sure the `UseSelfcontainedAuthorization` flag is set to false, so Admin Api uses KeyCloak for authentication. 

This will setup all the necessary containers on your Docker. 

#### KeyCloak Credentials

Use the KeyCloak UI to reset the password for `myuser`
To do, this go to `https://localhost/auth`, sign in with `admin`/`admin`. Change realm to myrealm, users, myuser, and on the Credentials tab, reset the password.
You will need this password on the appSettings of the Instance-Management-Worker-Process.
On this file, on the AdminApiSettings section, you will have something like 

```
"Username": "myuser",
"ClientId": "ac",
"Password": "The new password you just reset"
```

### Postgres up and running

`instance-management-svc.yml` includes Postgres.

In this Postgres installation you need a database with this name: Ods_Minimal_Template. In the real world this is an ods database, although you can use for now an empty one.
And on this database, run this script: `ALTER DATABASE your_database_name IS_TEMPLATE true;` to make it a template database.
Using this database template, Instance-Management-Worker-Process will create the ods instance database.

### Using the CreateOdsInstance.http

To understand what Instance-Management-Worker-Process does, you can review this (documentation)[https://github.com/Ed-Fi-Alliance-OSS/AdminAPI-2.x/tree/main/docs/design/adminconsole], but the big picture is this: It get the AdminConsole instances with status pending and for each one call the complete endpoint on Admin Api, so the key and secret are generated, the instance is copied over to the ods schema, etc.

To create the Admin Console instance, you can use the `CreateOdsInstance.http` file under `http` folder. It has a number of calls to: get a token on KeyCloak, create the pending instance, get pending instances and get completed instances. 