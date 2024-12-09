# Thinkify HSP 2.0 GUI Interface

A graphical user interface (GUI) developed by Converting Equipment International for managing Thinkify's HSP 2.0 system. This project provides an intuitive and user-friendly interface for interacting with RFID tags, allowing users to load tags from a file, adjust settings, generate tags from a file, adjust antenna settings, and perform basic commands to the HSP.

**Note:** Converting Equipment International is not connected to Thinkify in any way, shape, or form.

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## Installation

### Prerequisites

Ensure you have the following installed:

- [Visual Studio](https://visualstudio.microsoft.com/) with the .NET MAUI workload (preferred)
- [.NET SDK](https://dotnet.microsoft.com/download)

### Platform Support

- **Windows**: Fully supported.
- **Mac**: Support is possible with testing and some adjustments.

### Steps for Windows

1. Clone the repository:

    ```sh
    git clone https://github.com/yourusername/ThinkifyHSP2.0GUI.git
    cd ThinkifyHSP2.0GUI
    ```

2. Open the solution file (`ThinkifyHSP2.0GUI.sln`) in Visual Studio.

3. Restore the NuGet packages:

    ```sh
    dotnet restore
    ```

4. Build the project:

    ```sh
    dotnet build
    ```

5. Run the project:

    ```sh
    dotnet run
    ```

### Additional Installation Steps

If you are not using Visual Studio, you will need to:

1. **Create a Certificate**:
   Follow instructions to create a self-signed certificate.

2. **Enable Developer Mode**:
   - Open **Settings**.
   - Go to **Update & Security**.
   - Select **For developers**.
   - Enable **Developer Mode**.

## Usage

To use the Thinkify HSP 2.0 GUI Interface, run the following command:

```sh
dotnet run
