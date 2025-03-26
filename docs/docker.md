# Docker Support for Ed-Fi-Admin-Console-Instance-Management-Worker-Process

1. Having an Admin Api up and running is a requirement for `Instance-Management-Worker-Process`.
   One option for setting it up quickly is by using docker as well, following these [instructions](https://github.com/Ed-Fi-Alliance-OSS/AdminAPI-2.x/blob/main/docs/docker.md). Take into account that Admin Api needs to be running with KeyCloak authentication
   enabled.

2. Copy and customize the `.env.template.mssql` or `.env.template.pgsql` file.
   The project has a PostgreSQL version (docker/pgsql) and a MSSQL version (docker/mssql)
   to run the containers. Importantly, be sure to change the encryption key.

   PostgreSQL

   ```shell
   cd docker
   cp .env.template.pgsql .env
   code .env
   ```

   MSSQL

   ```shell
   cd docker
   cp .env.template.mssql .env
   code .env
   ```

3. When you are running `Instance-Management-Worker-Process` with mssql it is required to provide
   an Ods database backup with `.bak` format. Copy and paste that file in the docker/mssql folder
   and name it `Ods_Minimal_Template.bak`.

4. On the other hand, for Postgres setting up the template database is different.
   There must be a database in your Postgres Database Server with name `Ods_Minimal_Template`.
   It must be a template database. To make it a database template run the followint command:

    ```sql
    ALTER DATABASE "Ods_Minimal_Template" IS_TEMPLATE true;
    ```

5. Start containers. Go to the mssql folder or pgsql folder.

   ```shell
   docker compose -f instance-management-svc.yml --env-file ./../.env up -d
   ```
