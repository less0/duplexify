## Summary

*duplexify* is a small dockerized application that merges PDF files that have been scanned frontside 
first and then backside. It watches on a configurable folder for PDFs to come in and as soon as 
there are two PDFs it assumes the the first one contains the frontsides and the second contains the
backsides (in descending order, e.g. pages 6, 4, 2) and merges them using the 
[Java port](https://gitlab.com/pdftk-java/pdftk) of [PDFtk Server](https://www.pdflabs.com/tools/pdftk-server/). 
(When run directly, PDFtk Server itself works as well.) Erroneous PDFs are moved to an error directory 
(to timestamped subfolders).

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
and read them after they have been merged. 

## Integration with portainer

Since portainer does not allow custom options when you create a CIFS volume and by default a CIFS volume is mounted as *root* without write access for the restricted user that runs the app, you can run *duplexify* on portainer using Stacks. Stacks allow you to create a Docker compose file *"on the fly"* and within that create CIFS volumes with a custom UID. 

**Example Stack:**

```
services:
  duplexify:
    image: less0/duplexify:latest
    container_name: duplexify_worker
    volumes:
      - in:/app/in 
      - out:/app/out
    restart: unless-stopped

volumes:
  in:
    driver: local
    driver_opts:
      type: "cifs"
      device: "<input network share>" # e.g. //192.168.0.123/duplexify/in
      o: "username=<input network share user>,password=<input network share pass>,vers=2.0,uid=1654,gid=1654"
  out:
    driver: local
    driver_opts:
      type: "cifs"
      device: "<output network share>" # e.g. //192.168.0.123/duplexify/out
      o: "username=<output network share user>,password=<output network share pass>,vers=2.0,uid=1654,gid=1654"
```

Just replace the parts in the angle brackets with the values suitable for your setup. `1654` is the UID and GID of the user and group `app` in the image.

## Docker variables

| Variable | |
|-|-|
| `DOTNET_WatchDirectory` | The directory the application watches at. Effectively defaults to `"/app/in"`. |
| `DOTNET_OutDirectory` | The directory the merged PDFs are written to. Effectively defaults to `"/app/out"` |
| `DOTNET_ErrorDirectory` | The directory, erroneous PDFs are written to. Effectively defaults to `"/app/error"` |

# Known limitations

- At the moment *duplexify* might struggle on files being added simultanously, this might result in unexpected results, make sure files are added one after another