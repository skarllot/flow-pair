# FlowPair

_FlowPair provides automated code review and feedback using CI&T Flow AI through a command-line interface._

[![GitHub Release](https://img.shields.io/github/v/release/skarllot/flow-pair)](https://github.com/skarllot/flow-pair/releases)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://raw.githubusercontent.com/skarllot/flow-pair/main/LICENSE)

<hr />

## About

FlowPair is a CLI tool that leverages CI&T's Flow AI to provide automated code reviews. It detects Git staged (or unstaged) changes and generates insightful feedback, enhancing your development process with AI-powered assistance.

## Features

- Automated code review for Git changes
- AI-powered feedback generation
- HTML report output for easy review
- Simple configuration and usage

## Installation

1. Download the latest version of FlowPair from the [releases page](https://github.com/skarllot/flow-pair/releases).

2. Extract the downloaded archive to a directory of your choice.

3. Add the FlowPair directory to your system's PATH.

4. Open a terminal and run the following command to verify the installation:
```
flowpair --version
```

## Configuration

To configure FlowPair, run:
```
flowpair configure
```

You will be prompted to provide the following information:

- Tenant
- Client ID
- Client Secret

These credentials are necessary for authenticating with the CI&T Flow AI service.

## Usage

To review your Git changes and receive feedback, simply run:
```
flowpair review
```

This command will:
1. Detect Git staged (or unstaged) changes in your current repository
2. Send the changes to CI&T Flow AI for review
3. Generate an HTML file with the feedback

The HTML report will be saved in your current directory for easy access and review.

## Documentation

For more detailed information on how to use FlowPair and its features, please refer to our [GitHub wiki](https://github.com/skarllot/flow-pair/wiki).

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
