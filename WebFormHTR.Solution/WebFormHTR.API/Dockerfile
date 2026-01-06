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

COPY ["formHTR", "/app/formHTR"]
WORKDIR /app/formHTR

RUN python3 -m venv /app/venv
ENV PATH="/app/venv/bin:$PATH"
ENV Python__InterpreterPath="/app/venv/bin/python"
ENV Python__ScriptsFolder="/app/formHTR"

RUN pip install --default-timeout=1000 --no-cache-dir -r requirements.txt

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebFormHTR.Solution/WebFormHTR.API/WebFormHTR.API.csproj", "WebFormHTR.Solution/WebFormHTR.API/"]
COPY ["WebFormHTR.Solution/WebFormHTR.Application/WebFormHTR.Application.csproj", "WebFormHTR.Solution/WebFormHTR.Application/"]
COPY ["WebFormHTR.Solution/WebFormHTR.Infrastructure/WebFormHTR.Infrastructure.csproj", "WebFormHTR.Solution/WebFormHTR.Infrastructure/"]
COPY ["WebFormHTR.Solution/WebFormHTR.Domain/WebFormHTR.Domain.csproj", "WebFormHTR.Solution/WebFormHTR.Domain/"]
RUN dotnet restore "WebFormHTR.Solution/WebFormHTR.API/WebFormHTR.API.csproj"
COPY . .
WORKDIR "/src/WebFormHTR.Solution/WebFormHTR.API"
RUN dotnet build "WebFormHTR.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebFormHTR.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebFormHTR.API.dll"]
