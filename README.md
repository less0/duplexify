## Summary

*duplexify* is a small dockerized application that merges PDF files that have been scanned frontside 
first and then backside. It watches on a configurable folder for PDFs to come in and as soon as 
there are two PDFs it assumes the the first one contains the frontsides and the second contains the
backsides (in descending order, e.g. pages 6, 4, 2) and merges them using 
[PDFtk Server](https://www.pdflabs.com/tools/pdftk-server/). Erroneous PDFs are moved to an error 
directory (to timestamped subfolders).

## License

*duplexify* is released under the [GNU General Public License v3.0](https://github.com/less0/duplexify/blob/main/LICENSE).

## Building with `dotnet`

The application can be built and run without Docker, with the `dotnet` command. The .net 8 SDK 
has to be installed, furthermore the `pdftk` command (PDFtk Server) has to be available from the 
command line. From `duplexify.Application` directory, the application can be built with 

```
dotnet build
```

Afterwards the application can be run from the `bin\Debug\net8.0` directory. By default the 
directories `in`, `out` and `error` are created as subdirectories of the `net8.0` directory. 
Custom directories can be set in `appsettings.json`

| Configuration key | Explanation |
|-|-|
| `WatchDirectory` | The directory the application watches at. |
| `OutDirectory` | The directory the merged PDFs are written to. |
| `ErrorDirectory` | The directory erroneous PDFs are written to. |

## Building with Docker

To build and run the application with Docker, Docker has to be installed. PDFtk Server will be 
downloaded to the container during the build process and is not required to be preinstalled. 
From `duplexify.Application` the image can be built with

```
docker build -t duplexify .
```

To add files to be merged, it comes handy to have mounted volumes you can write the PDFs to 
and read them after they have been merged. The exact configuration depends on your system and 
personal preferences, therefor I won't provide the information how to run the container here 
(and I assume you should know, how to run a Docker container).

## Docker variables

| Variable | |
|-|-|
| `DOTNET_WatchDirectory` | The directory the application watches at. Effectively defaults to `"/app/in"`. |
| `DOTNET_OutDirectory` | The directory the merged PDFs are written to. Effectively defaults to `"/app/out"` |
| `DOTNET_ErrorDirectory` | The directory, erroneous PDFs are written to. Effectively defaults to `"/app/error"` |

## Using the same input and output directory

If the input and output directory are the same, the merged file will be written to the input 
directory and be merged with the next file that comes in. The application does not takes this into
account at the moment, therefor, the configuration should not point to the same directory for
watching and merging. 