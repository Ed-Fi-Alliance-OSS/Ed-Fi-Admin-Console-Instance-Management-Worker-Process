# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

FROM mcr.microsoft.com/mssql/server@sha256:d7f2c670f0cd807b4dc466b8887bd2b39a4561f624c154896f5564ea38efd13a AS base
USER root
RUN apt-get update && apt-get install unzip -y dos2unix busybox openssl libxml2
LABEL maintainer="Ed-Fi Alliance, LLC and Contributors <techsupport@ed-fi.org>"

# Copy MsSql backup file
COPY ./Ods_Minimal_Template.bak /var/opt/mssql/data/
