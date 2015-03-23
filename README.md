# Meridium EPiServer Migration

A library which aims to help the [EPiServer](http://www.episerver.com/) developer to migrate content from an old EPiServer site (5+) to a new EPiServer 7.5+ site.

## What is it?

This project is an effort to reuse some common patterns that emerge when a new site is developed and content from the old site must be migrated to the new. 

The main feature is an API which is used to specify mappings of page types and their properties from the old site to the content types of the new site. 

Please read the following conventions we use to decide if this library will be usable in your scenario.

### Conventions

**Import**

- `PageSaved`, `PageChanged`, `PageChangedBy` and `PageCreatedBy` are set to their original values on publish
- Currently all pages' master language is hard coded to `sv` (swedish). This is a remain from an old custom migration an will be removed

**Migrate**

## How to install

Currently there is no pre-built artifact you can fetch from NuGet or download. There is however a simple post build script which zips the necessary files for you. 

So you need to clone this repository and build it locally. 

### Build

After you have cloned the repository you'll probably want to [update the NuGet references to EPiServer](http://world.episerver.com/documentation/items/installation-instructions/installing-episerver-updates/) from [EPi's NuGet feed](http://nuget.episerver.com/feed/packages.svc/) to the specific version your project is using to avoid any version conflicts. 

When you successfully build the project, a [post-build event](https://msdn.microsoft.com/en-us/library/ke5z92ks.aspx) is triggered which executes the **build.ps1** PowerShell file. This script creates a **build** folder in the project and a zip archive - **migration.zip** - in that folder containing the files you need.    

```
migration.zip
|-- Migration
|   `-- Migrate.aspx                      # Migration Web UI 
|-- lib
    |-- HtmlAgilityPack.dll               # Dependency
    |-- HtmlAgilityPack.pdb
    |-- Meridium.EPiServer.Migration.dll  # Where all the good stuff lives
    `-- Meridium.EPiServer.Migration.pdb  # Debug stuff
```

If you get a PowerShell error during the post-build event you'll probably need to [set the PowerShell execution policy](http://stackoverflow.com/questions/6500320/post-build-event-execute-powershell). Note that Visual Studio is a 32-bit application and that [PowerShell 32/64 bit have different settings.](http://forloveofsoftware.blogspot.se/2011/07/powershell-32-and-64-bit-have-different.html)

## Using it

When you have the zip-file, we recommend that you create a new project in the solution of your new site which will receive the result of the migration, e.g. `Your.New.Site.Migration`.

Extract the zip here and reference the **Meridium.EPiServer.Migration** assembly and the project where your content types live.

What you need to do now is to define the mappings from page types in the old site to the new shining content types of your new site. 

### Define mappings

The following commented code example will explain the main building blocks.

```c#
// The migration library will scan the assemblies of the bin folder
// looking for classes marked with the MigrationInitializer attribute
// and a public static void Initialize method which it will happily call.
[MigrationInitializer]
public static class MigrationDefinitions {
  public static void Initialize() {
    // Register your mappings using the static Register method of the 
    // MapperRegistry class. This method accepts a variable parameter list
    // if IPageMapping instances
    MapperRegistry.Register(

      // Hopefully you will not need to implement the IPageMapper interface
      // yourself, but use the PageMapper fluent API instead. The starting
      // point of the API is the Define method which accepts a name for the
      // mapping
      PageMapper.Define("Mapping name")

        // The Map method defines a mapping for a page type. The new strongly 
        // typed content type is specified as the type argument and the name 
        // of the old page type is given as the first argument. 
        .Map<NewsListPage>("[Old] NewsArchive",

          // The second argument to Map is an Action<SourcePage, TNew> callback
          // in which you specify how properties are mapped. The SourcePage
          // class contains the properties of the page being migrated and has
          // a couple of helpful methods you can use to get the values. 
          // This example uses the GetValueWithFallback method which falls 
          // back to the next property in the list when a previous value does
          // not exist or is null
          (s, d) => d.Heading = s.GetValueWithFallback<string>("Header", "PageName"))

        .Map<NewsPage>("[Old] NewsItem",
          (s, d) => {
            d.Heading = s.GetValue<string>("Header");
            // Sometimes you'll need to clean up content from the old pages
            // This is best done via extension methods. A few are defined in
            // the library which may or may not be of use to you
            d.Introduction = s.GetValue<string>("Ingress").CleanupForIntroduction();
            d.MainBody = s.GetValue<XhtmlString>("Bread").CleanupForMainBody();
        })

        // You may leave the Action argument if no properties are mapped
        .Map<ContainerPage>("[Old] Folder")

        // The Default method defines the mapping for all pages where a mapping
        // cannot be found.
        .Default<ArticlePage>(
          (s, d) => {
            d.Heading = s.GetValueWithFallback<string>("MainHeader", "Header", "PageName");
            d.Introduction = s.GetValue<XhtmlString>("Ingress").CleanupForIntroduction();
            d.MainBody = s.GetValue<XhtmlString>("MainBody").CleanupForMainBody();
        }));
  }
}
```

The `MapperRegistry.Register` method accepts a list of `IPageMapper`s. This can be useful if you want to separate the mappings, e.g. when you are importing content from more than one site or the original site had subsections with their own page types.

## Execute migration

When you have built and tested your mappings it is time to execute the migration. These are the steps.

### 1. Export content from old site(s)

Use EPiServer's export tool under _Admin/Tools_ in the admin interface. Export the content you need in one or more packages.

Copy the export packages to the server where the migration will take place.

### 2. "Install" your migration dll to the target site

Move your migration dll that contain the page mappings together with the **Meridium.EPiServer.Migration** and the **HtmlAgilityPack** dlls to the bin folder of the target site. Copy the **Migrate.aspx** file to a suitable location.

### 3. Open the Migrate.aspx page in a browser

The **Migrate.aspx** page consists of three steps: **Import**, **Migrate** and **Clean up**.

#### 3.1 Import

The **Import** step performs an import of the pages which were exported from the other site in **step 1**. The input needed from you is the page id of the page under which the content will be imported and the absolute path on the ser ver of the package to import. When you're done, click **Import** to start the import. 

When the import is done the log output is displayed at the bottom of the page.

#### 3.2 Migrate

The next step is to perform the actual migration of content from old page types to the new types. Specify the `id` of the root page of the imported pages in **step 3.1** and select the page mapping you want to use from the drop down list. Click the **Migrate** button to start the migration.

Log output is displayed at the bottom of the page when the migration is done.

#### 3.3 Clean up

The last step is to delete the imported page types, since they are probably no longer relevant to keep around. Click the **Delete page types** button to delete the imported page types. Leave the **Logging only** checkbox checked to only log the page types which would be removed.

The heuristic used when deciding which page types to remove is 

- the page type has no associated model (`ModelType` property is `null`)
- the page type name does not start with `"sys"`

### 4. Clean up

When you are done you can

- Delete the **Migrate.aspx** page
- Delete the dll:s you copied in **step 2**
- Delete the import packages.

## Additional resources

- How to plan an EPiServer 7 upgrade or content migration: [part 1](http://www.epinova.no/blog/arild-henrichsen/dates/2013/11/how-to-plan-an-episerver-7-upgrade-or-content-migration--part-1/), [part 2](http://www.epinova.no/blog/arild-henrichsen/dates/2014/3/how-to-plan-an-episerver-7-upgrade-or-content-migration---part-2/) (Arild Henrichsen)
- [Complex import/export with EPiServer](https://andersnordby.wordpress.com/2012/06/04/complex-importexport-with-episerver/) (Anders G. Nordby) 
- [Import pages from EPiServer 4 to 7](http://krompaco.nu/2013/03/one-way-to-import-pages-from-episerver-4-to-7/) (Johan Kronberg)
- [Export from EPiServer CMS 4](http://blog.fredrikhaglund.se/blog/2008/02/28/export-from-episerver-cms-4/) (Fredrik Haglund)

## Contribute

### Issues

### Pull requests

## License


