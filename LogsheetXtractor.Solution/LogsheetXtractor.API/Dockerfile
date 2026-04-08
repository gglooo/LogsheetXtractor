FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN apt-get update && apt-get install -y \
    python3 \
    python3-pip \
    python3-venv \
    python3-dev \
    build-essential \
    libqpdf-dev \
    libjpeg-dev \
    zlib1g-dev \
    poppler-utils \
    libgl1 \
    libglib2.0-0 \
    libzbar0 \
    && rm -rf /var/lib/apt/lists/*

COPY ["LogsheetXtractor.Solution/LogsheetXtractor.API/requirements.txt", "/app/requirements.txt"]

RUN python3 -m venv /app/venv
ENV PATH="/app/venv/bin:$PATH"
ENV Python__InterpreterPath="/app/venv/bin/python"

RUN pip install --default-timeout=1000 --no-cache-dir -r /app/requirements.txt

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LogsheetXtractor.Solution/LogsheetXtractor.API/LogsheetXtractor.API.csproj", "LogsheetXtractor.Solution/LogsheetXtractor.API/"]
COPY ["LogsheetXtractor.Solution/LogsheetXtractor.Application/LogsheetXtractor.Application.csproj", "LogsheetXtractor.Solution/LogsheetXtractor.Application/"]
COPY ["LogsheetXtractor.Solution/LogsheetXtractor.Infrastructure/LogsheetXtractor.Infrastructure.csproj", "LogsheetXtractor.Solution/LogsheetXtractor.Infrastructure/"]
COPY ["LogsheetXtractor.Solution/LogsheetXtractor.Domain/LogsheetXtractor.Domain.csproj", "LogsheetXtractor.Solution/LogsheetXtractor.Domain/"]
RUN dotnet restore "LogsheetXtractor.Solution/LogsheetXtractor.API/LogsheetXtractor.API.csproj"
COPY . .
WORKDIR "/src/LogsheetXtractor.Solution/LogsheetXtractor.API"
RUN dotnet build "LogsheetXtractor.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LogsheetXtractor.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogsheetXtractor.API.dll"]
