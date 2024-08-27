
# SXConverter

SXConverter is a simple tool to convert a CSV file to a Ultrafast System (UFS) file.

## Requirements

- [.NET Desktop runtime 8.0](https://dotnet.microsoft.com/ja-jp/download/dotnet/8.0)
- Windows 10 or later

## Usage

1. Run the SXConverter.exe.
1. Select a CSV file to convert (or drag and drop the file to the text box).
1. Click the "Load" button.
1. Select wavelength and time ranges and click the "Trim" button if necessary.
1. Edit the metadata if necessary.
1. Select a destination file (or drag and drop the file to the text box).
1. Click the "Save" button.

## Features

- Load a CSV file
- Load a UFS file
- Trim data
- Edit metadata
- Save a UFS file
- Save a CSV file

These features allow you to convert a CSV file to a UFS file and vice versa.
Technically, you can load a CSV file and save it as a CSV file, but it is not useful.

## UFS format

The UFS format is a binary format that contains the exactly same data as the CSV file saved from the Ultrafast Systems softwares.
The technical details of the UFS format is not public, but the analysis has been made for [csv2ufs](https://bitbucket.org/ptapping/csv2ufs/src/master/) project.
This project basically follows this analysis, with some improvements.

### Data sections

The UFS file contains the following sections with exact same order:

1. A version string (string, e.g., "Version2")
1. The first axis label (string, e.g., "Wavelength")
1. The first axis unit (string, e.g., "nm")
1. The number of data points in the first axis (Int32)
1. The list of data points in the first axis (Double[] with counts equal to the previous item)
1. The second axis label (string, e.g., "Time")
1. The second axis unit (string, e.g., "ps")
1. The number of data points in the second axis (Int32)
1. The list of data points in the second axis (Double[] with counts equal to the previous item)
1. The data section label\* (string, "DA")
1. The padding\* (Int32, 0)
1. The number of data points in the first axis (Int32, must be equal to above)
1. The number of data points in the second axis (Int32, must be equal to above)
1. A series of data points (Double[] with counts equal to the previous two items)
1. The metadata section (string, starts with "file info")

Items with \* are unknown and can be different values.

### Data encoding

#### Int32

The Int32 values are stored as 4 bytes in big-endian order.

#### Double

The Double values are stored as 8 bytes (assuming IEEE 754 double precision format) in big-endian order.

#### String

The string values are stored with its length (Int32) followed by the string itself.

The string values are encoded with system default encoding.
(The csv2ufs project assumes it is UTF-8, but it is not always true.)

The new line code can be either LF or CRLF.

## Library

A library to read and write UFS files is `SXConverter.Ufs.dll` and separated from the GUI application.
`SXConverter.Ufs.SpectraData` class provides the basic functionalities to read and write UFS files.
The wavelengths, times, and signals data are not public, but you can access them by inheriting the class.

## License

This project is licensed under the MIT License.
