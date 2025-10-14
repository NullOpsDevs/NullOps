echo "Building containers...";
docker compose down --remove-orphans;
docker compose build;
echo "Starting containers...";
docker compose up -d pgsql;
docker compose up -d nullops;
echo "Executing tests...";
docker compose run --rm nullops-testsuite /app/NullOps.Tests.E2ESuite
docker compose down --remove-orphans;
