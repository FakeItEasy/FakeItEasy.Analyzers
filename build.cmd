@pushd %~dp0
@dotnet run --project ".\tools\FakeItEasy.Analyzers.Build\FakeItEasy.Analyzers.Build.csproj" -- %*
@popd
