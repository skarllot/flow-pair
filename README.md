# FlowPair

_FlowPair is a command-line interface tool that leverages CI&T Flow AI to enhance software development processes._

[![Build status](https://github.com/skarllot/flow-pair/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/skarllot/flow-pair/actions)
[![GitHub release](https://img.shields.io/github/v/release/skarllot/flow-pair)](https://github.com/skarllot/flow-pair/releases)
[![Code coverage](https://codecov.io/gh/skarllot/flow-pair/graph/badge.svg?token=XQ7SBGPS89)](https://codecov.io/gh/skarllot/flow-pair)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://raw.githubusercontent.com/skarllot/flow-pair/main/LICENSE)

<hr />

## About

FlowPair is a powerful command-line tool designed to enhance your software development workflow. By integrating CI&T's Flow AI, FlowPair provides intelligent assistance for various development tasks, including:

- Automated code reviews
- Unit test generation
- AI-powered feedback on code changes

With FlowPair, developers can:

- Improve code quality through AI-assisted reviews
- Save time on routine tasks like unit test creation
- Receive instant, actionable feedback on their work
- Streamline the development process with easy-to-use commands

Whether you're working on a small project or a large-scale application, FlowPair offers the tools you need to develop more efficiently and maintain high code standards.

## Installation

1. Download the latest version of FlowPair from the [releases page](https://github.com/skarllot/flow-pair/releases).

2. Extract the downloaded archive to a directory of your choice.

3. Add the FlowPair directory to your system's PATH.

4. Open a terminal and run the following command to verify the installation:

```bash
flowpair --version
```

## Configuration

To configure FlowPair, run:

```bash
flowpair configure
```

You will be prompted to provide the following information:

- Tenant
- Client ID
- Client Secret

These credentials are necessary for authenticating with the CI&T Flow AI service.

## Usage

### Code Review

To review your Git changes and receive feedback, use the `review` command:

```bash
flowpair review [path] [options]
```

Arguments:
- `[path]`: Optional. Path to the repository. If not specified, the current directory is used.

Options:
- `-c` or `--commit`: Optional. Specify a commit hash to review changes from that specific commit.

Examples:
1. Review changes in the current directory:
   ```bash
   flowpair review
   ```

2. Review changes in a specific repository:
   ```bash
   flowpair review /path/to/your/repo
   ```

3. Review changes from a specific commit:
   ```bash
   flowpair review -c abc123
   ```

This command will:
1. Detect Git changes in the specified repository (or current directory)
2. Send the changes to CI&T Flow AI for review
3. Generate an HTML file with the feedback
4. Automatically open the HTML report in your default web browser

### Creating Unit Tests

To create a unit test for a specific code file, use the `unittest create` command:

```bash
flowpair unittest create -f <file-path> [-e <example-file-path>]
```

Options:
- `-f` or `--file-path`: The file path of the code to test (Required)
- `-e` or `--example-file-path`: The example unit test file path (Optional)

This command will generate a unit test for the specified code file, optionally using an example unit test file as a reference.

### Updating Unit Tests

To update an existing unit test with code changes, use the `unittest update` command:

```bash
flowpair unittest update [options]
```

Options:
- `-s` or `--source-file`: The file path of the code to test (Required)
- `-t` or `--test-file`: The file path of the existing unit tests (Required)

This command will update the existing unit test file to reflect changes made in the source code file.

## Contributing

We welcome contributions to FlowPair! If you have suggestions for improvements or encounter any issues, please feel free to:

- Open an [issue](https://github.com/skarllot/flow-pair/issues)
- Submit a [pull request](https://github.com/skarllot/flow-pair/pulls)

Before contributing, please read our [contribution guidelines](CONTRIBUTING.md).

## Support

If you need help, have any questions, or want to report issues:

- Use our [GitHub Issues](https://github.com/skarllot/flow-pair/issues) page to report problems or seek assistance

## License

FlowPair is licensed under the [MIT License](./LICENSE).
