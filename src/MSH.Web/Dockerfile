# Stage 1: Build .NET app and Matter SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG CACHE_BUSTER
WORKDIR /src

# Install Matter SDK dependencies
# RUN apt-get update && \
#     apt-get install -y git cmake ninja-build gcc g++ python3-venv libssl-dev libdbus-1-dev \
#     libglib2.0-dev libavahi-client-dev libboost-dev && \
#     rm -rf /var/lib/apt/lists/*

# # Clone Matter SDK
# RUN git clone --depth 1 https://github.com/project-chip/connectedhomeip.git /matter-sdk

# # Build Matter SDK (korrigiert)
# WORKDIR /matter-sdk
# SHELL ["/bin/bash", "-c"]
# RUN source scripts/bootstrap.sh && \
#     scripts/build/build_examples.py --target linux-arm64 build

# Copy .NET projects
COPY ["src/MSH.Web/MSH.Web.csproj", "src/MSH.Web/"]
COPY ["src/MSH.Infrastructure/MSH.Infrastructure.csproj", "src/MSH.Infrastructure/"]
COPY ["src/MSH.Core/MSH.Core.csproj", "src/MSH.Core/"]
RUN dotnet restore "src/MSH.Web/MSH.Web.csproj"

# Copy everything else and publish
COPY . .
RUN dotnet publish "src/MSH.Web/MSH.Web.csproj" -c Release -o /app/publish

# Stage 2: Final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install runtime dependencies (Matter + .NET)
RUN apt-get update && \
    apt-get install -y libgdiplus libssl-dev libavahi-client3 libdbus-1-3 && \
    rm -rf /var/lib/apt/lists/*

# Copy .NET artifacts
COPY --from=build /app/publish .

# # Copy Matter SDK artifacts (adjust paths as needed)
# COPY --from=build /matter-sdk /matter-sdk

# Set environment variables for Matter
# ENV MATTER_ROOT=/matter-sdk
# ENV PATH="${PATH}:/matter-sdk/examples/chip-tool/out"

# Entrypoint with migration support
ENTRYPOINT ["dotnet", "MSH.Web.dll", "--migrate"]