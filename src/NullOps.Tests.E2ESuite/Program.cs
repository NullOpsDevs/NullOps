using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;
using Npgsql;
using NullOps.Tests.E2ESuite.Clients;
using NullOps.Tests.E2ESuite.Scenarios;
using Refit;
using Spectre.Console;

namespace NullOps.Tests.E2ESuite;

public static class Program
{
    private const string PostgresImage = "postgres";
    private const string PostgresTag = "18.0-alpine3.22";
    private const string TestDockerLabel = "nullops.e2e.component";
    private const string E2ENetworkName = "nullops.e2e.network";
    private const string DatabaseCredential = "root";
    private const string DatabasePort = "14650";
    private const string DatabaseName = "nullops";
    private const string LocallyBuiltNullOpsImageName = "e2ebuild-nullops:latest";
    private const string APIPort = "14651";
    private const string BaseUrl = $"http://localhost:{APIPort}";

    public static async Task<int> Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var dockerClient = new DockerClientConfiguration().CreateClient();

        if (!await CheckDockerVersionAsync(dockerClient))
            return 255;

        await CleanupAsync(dockerClient);
        await CreateNetworkAsync(dockerClient);
        await CheckAndPullImageAsync(dockerClient, PostgresImage, PostgresTag);
        var dbContainerName = await SetupDatabaseAsync(dockerClient);

        if (!await BuildAPIContainer(dockerClient))
            return 255;

        if (!await CheckDatabaseAsync())
            return 255;

        var apiContainerId = await SetupAPIAsync(dockerClient, dbContainerName);

        if (apiContainerId == null)
            return 255;

        if (!await RunTestsAsync(dockerClient, apiContainerId))
            return 255;

        await WriteLogsToFile(dockerClient, apiContainerId);
        await CleanupAsync(dockerClient);

        return 0;
    }

    private static async Task<bool> CheckDockerVersionAsync(DockerClient client)
    {
        AnsiConsole.MarkupLine("[underline]Checking Docker version...[/]");

        string? detectedVersion = null;

        try
        {
            var version = await client.System.GetVersionAsync();
            detectedVersion = version.Version;
        }
        catch
        {
            /* ignored */
        }

        if (detectedVersion == null)
        {
            AnsiConsole.MarkupLine("[red underline]Unable to connect to Docker[/]");
            return false;
        }

        AnsiConsole.MarkupLine($"[green underline]Connected to Docker, version: [bold]{detectedVersion}[/][/]");
        return true;
    }

    private static async Task CleanupAsync(DockerClient client)
    {
        AnsiConsole.MarkupLine("[underline]Cleaning up containers...[/]");

        var containers = await client.Containers.ListContainersAsync(new ContainersListParameters
        {
            All = true
        });

        var containersFromPreviousRuns = containers
            .Where(container => container.Labels.ContainsKey(TestDockerLabel))
            .ToArray();

        if (containersFromPreviousRuns.Length == 0)
            AnsiConsole.MarkupLine("[green]No containers from previous runs were found.[/]");
        else
            foreach (var container in containersFromPreviousRuns)
            {
                await client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters
                {
                    Force = true,
                    RemoveVolumes = true
                });

                AnsiConsole.MarkupLine($"[green]Container '{container.Names[0]}' was removed.[/]");
            }

        var networks = await client.Networks.ListNetworksAsync(new NetworksListParameters());

        var networksFromPreviousRuns = networks
            .Where(network => network.Labels.ContainsKey(TestDockerLabel))
            .ToArray();

        if (networksFromPreviousRuns.Length == 0)
        {
            AnsiConsole.MarkupLine("[green]No networks from previous runs were found.[/]");
            return;
        }

        foreach (var network in networksFromPreviousRuns)
        {
            await client.Networks.DeleteNetworkAsync(network.ID);

            AnsiConsole.MarkupLine($"[green]Network '{network.Name}' was removed.[/]");
        }
    }

    private static async Task CreateNetworkAsync(DockerClient client)
    {
        AnsiConsole.MarkupLine("[underline]Creating network...[/]");

        await client.Networks.CreateNetworkAsync(new NetworksCreateParameters
        {
            Name = E2ENetworkName,
            Labels = new Dictionary<string, string>
            {
                [TestDockerLabel] = "true"
            },
            Driver = "bridge",
            CheckDuplicate = true
        });

        AnsiConsole.MarkupLine("[green underline]Network was created![/]");
    }

    private static async Task CheckAndPullImageAsync(DockerClient client, string image, string tag)
    {
        AnsiConsole.MarkupLine($"[underline]Checking and pulling image '[bold]{image}:{tag}[/]'...[/]");

        var images = await client.Images.ListImagesAsync(new ImagesListParameters
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>
            {
                ["reference"] = new Dictionary<string, bool> { [$"{image}:{tag}"] = true },
                ["dangling"] = new Dictionary<string, bool> { ["false"] = true }
            }
        });

        var imageFound = images.Any();

        if (!imageFound)
        {
            AnsiConsole.MarkupLine($"[yellow]Image '[bold]{image}:{tag}[/]' was not found, pulling...[/]");

            await AnsiConsole.Progress()
                .AutoRefresh(false)
                .AutoClear(true)
                .StartAsync(async context =>
                {
                    var tasks = new ConcurrentDictionary<string, ProgressTask>();

                    await client.Images.CreateImageAsync(
                        new ImagesCreateParameters { FromImage = image, Tag = tag },
                        null,
                        new Progress<JSONMessage>(m =>
                        {
                            if (m.Progress == null)
                                return;

                            var task = tasks.GetOrAdd(m.ID, _ => context.AddTask($"[bold]{m.ID}[/]"));

                            task.Description = $"{m.ID} ({m.Status})";
                            task.Value = m.Progress.Current;
                            task.MaxValue = m.Progress.Total;
                            context.Refresh();
                        }));
                });

            AnsiConsole.MarkupLine($"[green]Image '[bold]{image}:{tag}[/]' was pulled[/]");
        }
    }

    private static async Task<string> SetupDatabaseAsync(DockerClient client)
    {
        AnsiConsole.MarkupLine("[underline]Setting up database in the background...[/]");

        var dbContainerName = $"nops-e2e-postgres-{Guid.NewGuid():N}";

        var container = await client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = $"{PostgresImage}:{PostgresTag}",
            Name = dbContainerName,
            Labels = new Dictionary<string, string>
            {
                [TestDockerLabel] = "true"
            },
            Env =
            [
                $"POSTGRES_USER={DatabaseCredential}",
                $"POSTGRES_PASSWORD={DatabaseCredential}",
                $"POSTGRES_DB={DatabaseName}"
            ],
            HostConfig = new HostConfig
            {
                AutoRemove = true,
                NetworkMode = E2ENetworkName,
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    ["5432/tcp"] = new List<PortBinding>
                    {
                        new()
                        {
                            HostIP = "127.0.0.1",
                            HostPort = DatabasePort
                        }
                    }
                }
            }
        });

        await client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
        return dbContainerName;
    }

    private static async Task<bool> BuildAPIContainer(DockerClient client)
    {
        AnsiConsole.MarkupLine("[underline]Building API container...[/]");

        var solutionRoot = Directory.GetCurrentDirectory();

        while (!File.Exists(Path.Combine(solutionRoot, "NullOps.sln")))
            solutionRoot = Path.GetFullPath(Path.Combine(solutionRoot, ".."));

        AnsiConsole.MarkupLine($"[underline]Found solution root at: '[bold]{solutionRoot}[/]'[/]");

        var stream = new MemoryStream();
        var logs = new StringBuilder();

        AnsiConsole.Status()
            .Start("Collecting context", _ =>
            {
                var allFiles = Directory.GetFiles(solutionRoot, "*.*", SearchOption.AllDirectories);

                var withoutBinObjFolders = allFiles
                    .Where(path => !path.Contains("bin") && !path.Contains("obj"))
                    .ToArray();

                TarHelper.CreateTarFromPaths(solutionRoot, withoutBinObjFolders, stream);
            });

        stream.Seek(0, SeekOrigin.Begin);

        AnsiConsole.MarkupLine("[underline]Building NullOps API...[/]");

        JSONError? caughtError = null;

        await client.Images.BuildImageFromDockerfileAsync(new ImageBuildParameters
        {
            Dockerfile = "NullOps.Dockerfile",
            Tags = [LocallyBuiltNullOpsImageName],
            Remove = true,
            CPUShares = 2048,
            CPUQuota = (Environment.ProcessorCount - 2) * 100000,
            CPUPeriod = 100000,
            Memory = 12L * 1024 * 1024 * 1024,
            MemorySwap = -1
        }, stream, null, null, new Progress<JSONMessage>(m =>
        {
            if (m.Stream != null)
                AnsiConsole.Write(m.Stream);

            if (m.Error != null)
                caughtError = m.Error;
        }));

        if (caughtError == null)
        {
            AnsiConsole.MarkupLine("[green underline]API container was built![/]");
            return true;
        }

        AnsiConsole.Write(logs.ToString());
        AnsiConsole.MarkupLine($"[red underline]Failed to build API container: '{caughtError.Message}'[/]");

        return false;
    }

    private static async Task<bool> CheckDatabaseAsync()
    {
        const string connectionString =
            $"Host=localhost;Port={DatabasePort};Username={DatabaseCredential};Password={DatabaseCredential};Database=postgres";
        var pgsqlConnection = new NpgsqlConnection(connectionString);

        await AnsiConsole.Status()
            .StartAsync("Connecting to PgSQL DB..", async _ => { await pgsqlConnection.OpenAsync(); });

        AnsiConsole.MarkupLine("[green underline]Connected to PgSQL DB![/]");
        var cmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbName", pgsqlConnection);
        cmd.Parameters.AddWithValue("dbName", DatabaseName);

        var exists = await AnsiConsole.Status()
            .StartAsync("Checking if database exists..", async _ => await cmd.ExecuteScalarAsync() != null);

        if (!exists)
        {
            AnsiConsole.MarkupLine(
                "[red underline]Database does not exist. Already had too much time to setup. Configuration problem?[/]");
            return false;
        }

        AnsiConsole.MarkupLine("[green underline]Database exists![/]");

        cmd.Dispose();
        await pgsqlConnection.CloseAsync();
        await pgsqlConnection.DisposeAsync();

        return true;
    }

    private static async Task<string?> SetupAPIAsync(DockerClient client, string databaseContainerName)
    {
        var containerName = $"nops-e2e-api-{Guid.NewGuid():N}";

        AnsiConsole.MarkupLine("[underline]Setting up API...[/]");

        var container = await client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = LocallyBuiltNullOpsImageName,
            Name = containerName,
            Labels = new Dictionary<string, string>
            {
                [TestDockerLabel] = "true"
            },
            Env =
            [
                "ASPNETCORE_ENVIRONMENT=Development",
                "NOPS_API_PORT=7000",
                $"NOPS_DATABASE_CONNECTION_STRING=Host={databaseContainerName};Database={DatabaseName};Username={DatabaseCredential};Password={DatabaseCredential};"
            ],
            HostConfig = new HostConfig
            {
                AutoRemove = false,
                NetworkMode = E2ENetworkName,
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    ["7000/tcp"] = new List<PortBinding>
                    {
                        new()
                        {
                            HostIP = "0.0.0.0",
                            HostPort = APIPort
                        }
                    }
                }
            }
        });

        var started = await client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());

        if (!started)
        {
            AnsiConsole.MarkupLine("[red underline]Failed to start API container.[/]");
            return null;
        }

        AnsiConsole.MarkupLine("[green underline]API started. Waiting for a healthcheck...[/]");

        var httpClient = new HttpClient();
        var cancellationTokenSrc = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        while (!cancellationTokenSrc.IsCancellationRequested)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://localhost:{APIPort}/api/v1/health/ping")
            };

            try
            {
                var result = await httpClient.SendAsync(request, cancellationTokenSrc.Token);

                if (result.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine("[green underline]API is healthy![/]");
                    return container.ID;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[yellow underline]API is not healthy yet. Exception:[/]");
                AnsiConsole.WriteException(ex);
                await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
            }
        }

        AnsiConsole.MarkupLine(
            "[red underline]API is not healthy after 30 seconds. Something went wrong. Check logs.[/]");
        await WriteLogsToFile(client, container.ID);

        AnsiConsole.MarkupLine("[yellow underline]Running diagnostics...[/]");

        var inspect = await client.Containers.InspectContainerAsync(container.ID, CancellationToken.None);
        AnsiConsole.MarkupLine($"[yellow]API Container Status: {inspect.State.Status}[/]");
        AnsiConsole.MarkupLine($"[yellow]API Container Running: {inspect.State.Running}[/]");
        AnsiConsole.MarkupLine($"[yellow]API Container Exit Code: {inspect.State.ExitCode}[/]");
        if (!string.IsNullOrEmpty(inspect.State.Error))
            AnsiConsole.MarkupLine($"[red]API Container Error: {inspect.State.Error}[/]");

        // 2. List all running containers
        var runningContainers = await client.Containers.ListContainersAsync(new ContainersListParameters
        {
            All = false
        }, CancellationToken.None);

        AnsiConsole.MarkupLine($"[yellow]Running containers ({runningContainers.Count}):[/]");
        
        foreach (var c in runningContainers)
        {
            var ports = string.Join(", ", c.Ports.Select(p => $"{p.PrivatePort}->{p.PublicPort}"));
            AnsiConsole.MarkupLine($"  - {c.Names[0]}: {c.Image} {c.State} Ports: {ports}");
        }

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ss",
                    Arguments = "-ltn",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(CancellationToken.None);
            await process.WaitForExitAsync(CancellationToken.None);

            var relevantLines = output.Split('\n')
                .Where(line => line.Contains(APIPort) || line.Contains(DatabasePort))
                .ToArray();

            AnsiConsole.MarkupLine($"[yellow]Listening ports (filtered for {APIPort} and {DatabasePort}):[/]");
            foreach (var line in relevantLines)
                AnsiConsole.MarkupLine($"  {line}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to run ss command: {ex.Message}[/]");
        }

        return null;
    }

    private static async Task WriteLogsToFile(DockerClient client, string apiContainerId)
    {
        AnsiConsole.MarkupLine("[underline]Collecting logs from the container...[/]");

        var containerLogs =
            await client.Containers.GetContainerLogsAsync(apiContainerId, false, new ContainerLogsParameters
            {
                Follow = false,
                ShowStderr = true,
                ShowStdout = true,
                Timestamps = false
            });

        var logFileName = $"logs-{DateTime.Now.Ticks}.log";
        var logFile = File.Create(logFileName);
        var logFileWriter = new StreamWriter(logFile, Encoding.UTF8);

        var (stdout, stderr) = await containerLogs.ReadOutputToEndAsync(CancellationToken.None);

        await logFileWriter.WriteAsync(stdout);
        await logFileWriter.WriteAsync(stderr);
        await logFileWriter.FlushAsync();
        await logFile.FlushAsync();

        await logFileWriter.DisposeAsync();
        await logFile.DisposeAsync();

        var fullLogPath = Path.GetFullPath(logFileName);

        AnsiConsole.MarkupLine($"API logs were written to '{new Uri(fullLogPath, UriKind.Absolute)}'");
    }

    private static async Task<bool> RunTestsAsync(DockerClient dockerClient, string apiContainerId)
    {
        var globalContext = new GlobalTestContext
        {
            BaseUrl = BaseUrl,
            AuthClient = RestService.For<IAuthClient>(BaseUrl),
            TestSuiteClient = RestService.For<ITestSuiteClient>(BaseUrl)
        };

        var scenarios = new Scenario<GlobalTestContext>[]
        {
            new SetupDatabaseScenario(),
            new AuthenticationScenario(),
            new SetupClients(),
            new RegistrationScenario(),
            new UsersScenario()
        };

        foreach (var scenario in scenarios)
        {
            var result = await scenario.RunScenario(globalContext);

            if (!result)
            {
                AnsiConsole.MarkupLine($"[red]Scenario failed: '[underline]{scenario.Name}[/]'[/]");
                await WriteLogsToFile(dockerClient, apiContainerId);

                return false;
            }
        }

        AnsiConsole.MarkupLine("\n[green underline]All tests passed![/]");
        return true;
    }
}