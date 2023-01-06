# How to build

These instructions are *only* for building from the command line, which includes compilation, test execution and packaging. This is the simplest way to build.
It also replicates the build on the Continuous Integration (CI) build server and is the best indicator of whether a pull request will build.

You can also build the solution using Visual Studio 2019 or later, but this doesn't provide the same assurances as the command line build.

At the time of writing the full build (including testing on all target frameworks) can only run on Windows.

## Prerequisites

The build requires that a few pieces of software be installed on the host computer. We're somewhat aggressive about adoptiong new language features and the like, so rather than specifying exactly which versions are required, we'll tend toward "latest" or "at least" forms of guidance. If it seems you have an incompatible version of the software, prefer to upgrade rather than downgrade.

### On Windows

Ensure that the following are installed:

1. a recent version of Visual Studio 2019 (currently this means 16.3 or later) or the Build Tools for Visual Studio 2019

2. The .NET Core 2.1 runtime

3. The .NET Framework 4.6.1 or higher

4. The targeting packs for .NET Framework 4.0 and 4.5

5. A recent version of the .NET Core 3.0 SDK (currently this means 3.0.100 or later)

### On Linux

On non-Windows operating systems, tests are run on .NET Core 2.1 and 3.0, since the .NET Framework isn't supported on Linux.

Ensure the following are installed:

1. The .NET Core 2.1 runtime

2. A recent version of the .NET Core 3.0 SDK (currently this means 3.0.100 or later)

## Building

Using a command prompt, navigate to your clone root folder and execute:

- `build.cmd` on Windows
- `./build.sh` on Linux
- `./build.ps1` on Windows or Linux, if Powershell is installed

This executes the default build targets to produce .NET Standard 2.0 artifacts.

After the build has completed, the build artifacts will be located in `artifacts`.

## Extras

### Running specific build tasks

`build.cmd` wraps a [Bullseye](https://github.com/adamralph/bullseye) targets project, so you can use all the usual command line arguments that you would use with Bullseye, e.g.:

* View the full list of build targets:

    `build.cmd -T`

* Run a specific target:

    `build.cmd unit`

* Run multiple targets:

    `build.cmd unit pack`

* Run a target without running its dependencies (might fail if the dependencies
  haven't been previously built):

    `build.cmd -s pack`

* View the full list of options:

    `build.cmd -?`

(On Linux, just replace `build.cmd` with `./build.sh` or `./build.ps1`)

### Building the documentation

The CI workflow uses [mkdocs](https://www.mkdocs.org/) to build the documentation. To replicate this process,
install a recent [Python version](https://www.python.org/downloads/), and then install all the requirements
by running

```
python -m pip install --upgrade pip
python -m pip install --requirement requirements.txt
```

from the root of the repository. After this, you can generate the docs by running

```
python -m mkdocs build --clean --site-dir artifacts/docs --config-file mkdocs.yml --strict
```

The documentation will be built in `artifacts/docs` and can be viewed directly in a web
browser or served using a number of tools such as [dotnet-serve](https://github.com/natemcmaster/dotnet-serve) or [http.server](https://docs.python.org/3/library/http.server.html).

### Updating the documentation-building packages

The versions of the packages used to build the documentation are frozen using
[pip-compile](https://github.com/jazzband/pip-tools#example-usage-for-pip-compile) from the pip-tools project.

If you wish to update the packages used, install pip-tools:

```
python -m pip install --upgrade pip-tools
```

Then edit `requirements.in`, specifying the new requirements, and run

```
pip-compile requirements.in
```

from the root of the repository. After verifying the generated documentation, you can commit both
`requirements.*` files.
