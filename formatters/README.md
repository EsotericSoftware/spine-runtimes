# Formatters
This folder contains formatter configuration files to be used with IDEs as well as the [spotless](https://github.com/diffplug/spotless/blob/main/plugin-gradle/README.md) formatter expressed in the Gradle project in this directory.

You will need the following on your `PATH`:

- JDK 10+
- clang-format 12.0.1 (i.e. `brew install clang-format`). Also set the environment variable `CLANGFORMAT` to the path of the `clang-format` executable.
- dotnet format (i.e. `dotnet tool install -g dotnet-format`, comes with dotnet 6 out of the box)
- tsfmt, (i.e. `npm install -g typescript-formatter`)

To run the formatter, invoke the `format.sh` script. This will shuffle around the Gradle config files, invoke spotless, then undo the config file shuffling. Invoking `./gradlew spotlessApply` from the `formatters/` directory will not work.