#!/bin/bash
set -euo pipefail
dotnet run --project "./tools/FakeItEasy.Analyzers.Build/FakeItEasy.Analyzers.Build.csproj" -- $@

