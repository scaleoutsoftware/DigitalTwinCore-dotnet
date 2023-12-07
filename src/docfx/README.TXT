Install the docfx tool:

    dotnet tool update -g docfx

From a command prompt in this directory, build the site (output will land in the top-level *docs* folder, whose contents will be served by GitHub Pages):

    docfx


Run the site behind a development web server:

    docfx --serve --port=8081
