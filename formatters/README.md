# Formatters
This folder contains formatter configuration files to be used with IDEs as well as the [spotless](https://github.com/diffplug/spotless/blob/main/plugin-gradle/README.md) formatter expressed in the Gradle project in this directory.

You will need the following in your `PATH`:

- JDK 10+
- clang-format 12 (i.e. `brew install clang-format`)

To run the formatter, invoke the `format.sh` script. This will shuffle around the Gradle config files, invoke spotless, then undo the config file shuffling. Invoking `./gradlew spotlessApply` from the `formatters/` directory will not work.