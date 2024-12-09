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
    git clone https://github.com/CadenRickerCEI/HSP_2.0_Client_GUI
    cd HSP_2.0_Client_GUI
    ```

2. Open the solution file (`HSPGUI.sln`) in Visual Studio.

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
```
## License
MIT License

Copyright (c) 2024 Converting Equipment International

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
