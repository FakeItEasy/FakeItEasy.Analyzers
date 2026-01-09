# How to build

These instructions are *only* for building from the command line, which includes compilation, test execution and packaging. This is the simplest way to build.
It also replicates the build on the Continuous Integration (CI) build server and is the best indicator of whether a pull request will build.

You can also build the solution using Visual Studio 2019 or later, but this doesn't provide the same assurances as the command line build.

At the time of writing the full build (including testing on all target frameworks) can only run on Windows.

## Prerequisites

An up-to-date version of the .NET 10.0 SDK

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

The CI workflow uses [mkdocs](https://www.mkdocs.org/) to build the documentation.
We use [uv](https://docs.astral.sh/uv/) to manage mkdocs and other Python tools used for the documentation.
To build the documentation, **[install uv](https://docs.astral.sh/uv/getting-started/installation/)**.

After this, you can generate the docs by running

```
build.cmd docs
```

The documentation will be built in `artifacts/docs` and can be viewed directly in a web
browser or served using a number of tools such as [dotnet-serve](https://github.com/natemcmaster/dotnet-serve) or [http.server](https://docs.python.org/3/library/http.server.html).

### Updating the documentation-building packages

The versions of the packages used to build the documentation are frozen using uv as well.

If you wish to update the packages used, edit `pyproject.toml`, specifying the dev-dependencies, and run

```
uv lock
```

from the `docs` directory. After verifying the generated documentation, you can commit both
`pyproject.toml` and `uv.lock`.
