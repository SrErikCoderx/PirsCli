# PirsCli - Xbox 360 Package Extractor

`PirsCli` is a cross-platform command-line utility for listing and extracting contents from Xbox 360 PIRS, LIVE, and CON package files. This tool was developed by reverse-engineering an existing Windows GUI application (`wxPirs.exe`).

## Features

-   List contents of `.PIRS`, `.LIVE`, and `.CON` files.
-   Extract files from `.PIRS`, `.LIVE`, and `.CON` archives.

## Build from Source

To build this project, you need the .NET 8 SDK installed on your system.

1.  Clone this repository:
    ```bash
    git clone https://github.com/your-username/PirsCli.git
    cd PirsCli/src
    ```
2.  Build the project:
    ```bash
    dotnet build
    ```
    This will compile the application and place the executable (`PirsCli.dll`) in `bin/Debug/net8.0/` (or `bin/Release/net8.0/` if you build in Release mode).

## Usage

Navigate to the directory containing the compiled `PirsCli.dll` (e.g., `PirsCli/src/bin/Debug/net8.0/`).

### List Contents of a Package File

To list the files within a PIRS/LIVE/CON package:

```bash
dotnet PirsCli.dll -l <path_to_package_file>
# Example:
# dotnet PirsCli.dll -l C:\Games\DLC\MyGame.con
# dotnet PirsCli.dll -l /home/user/XboxContent/WorldPack.live
```

### Extract Contents of a Package File

To extract all files from a PIRS/LIVE/CON package to a specified output directory:

```bash
dotnet PirsCli.dll -x <path_to_package_file> <output_directory>
# Example:
# dotnet PirsCli.dll -x C:\Games\DLC\MyGame.con C:\ExtractedContent
# dotnet PirsCli.dll -x /home/user/XboxContent/WorldPack.live /tmp/extracted_data
```

## License

This project is licensed under the MIT License. See the [LICENSES.txt](LICENSES.txt) file for details.
Original application copyright information can also be found in `LICENSES.txt`.
