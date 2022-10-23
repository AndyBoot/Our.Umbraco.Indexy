# Our.Umbraco.Indexy

Indexy allows a developer to effortlessly create custom Examine indexes within Umbraco by simply adding a few options to the website `appsettings.json` file. 

## Why call it Indexy?
Because good names are hard to come by. 🙃

## Installation 

### Visual Studio
Simply search for the `Our.Umbraco.Indexy` NuGet package and add it to your project.

### CLI
`dotnet add package Our.Umbraco.Indexy`

## Configuration

### Example:
```
{
  "Our.Umbraco.Indexy": {
    "Groups": [
      {
        "IndexName": "BlogsIndex",
        "DocumentTypes": "article",
        "Fields": {
          "SystemFields": "",
          "FullText": "authorName",
          "DateTime": "articleDate",
          "Integer": "",
          "Float": "",
          "Double": "",
          "Long": "",
          "DateYear": "",
          "DateMonth": "",
          "DateDay": "",
          "DateHour": "",
          "DateMinute": "",
          "Raw": "",
          "FullTextSortable": "",
          "InvariantCultureIgnoreCase": "",
          "EmailAddress": ""
        }
      }
    ]
  }
}
```

- **Groups** - An array of index groups which can hold multiple index configurations.
    - **IndexName** - The name of your custom index (no spaces!)
    - **DocumentTypes** - A comma delimeted list of document type aliases to include within your index
    - **Fields** - A set of field types & aliases to include within your index. Each field types value is a comma delimeted list of field aliases. All of these properties are optional and therefore do not all need to be included (the example above is displaying these for demo purposes). All except 'SystemFields' is an Examine field definition of which more information can be found here: https://shazwazza.github.io/Examine/configuration:
        - **SystemFields** - Use this to filter out the standard system fields added by default. Leave blank to not include any at all. The `Id` and `nodeName` fields are automatically added. Default value: `__Icon,__Key,__Path,__Published,__VariesByCulture,createDate,creatorID,creatorName,email,icon,level,nodeType,parentID,path,sortOrder,template,templateID,updateDate,urlName,version,writerID,writerName`
        - **FullText**
        - **DateTime**
        - **Integer**
        - **Float**
        - **Double**
        - **Long**
        - **DateYear**
        - **DateMonth**
        - **DateDay**
        - **DateHour**
        - **DateMinute**
        - **Raw**
        - **FullTextSortable**
        - **InvariantCultureIgnoreCase**
        - **EmailAddress**

## Usage
This plugin is designed to make available custom indexes for usage within your own code. However for your reference, as an example you can call this by doing the following:

`_examineManager.TryGetIndex("BlogsIndex", out IIndex? index)`

More information and helpful examples can be found here: https://our.umbraco.com/documentation/reference/searching/examine/quick-start/#creating-the-search-query

## Contributions
You're more than welcome to fork and do a pull request.

## Issues
Please report them on the issues tab of this repository.