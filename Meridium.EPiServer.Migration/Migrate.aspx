<%@ Page Language="C#" AutoEventWireup="false" CodeBehind="Migrate.aspx.cs" Inherits="Meridium.EPiServer.Migration.Migrate" %>
<%@ Import Namespace="Meridium.EPiServer.Migration.Support" %>

<!DOCTYPE html>

<html>
<head>
    <title>Import and migrate pages</title>
    <link href="https://cdnjs.cloudflare.com/ajax/libs/skeleton/2.0.4/skeleton.min.css" rel="stylesheet" />
    
    <style type="text/css">
        .log-output {
            background-color: #000000;
            color: greenyellow;
            font-family: consolas, menlo, monospace;
            font-size: 12px;
            overflow: scroll;
            padding: 3px;
            max-height: 600px;
        }
    </style>

</head>
<body>
    <div class="container">
        <h1>EPiServer migration</h1>
        <form action="Migrate.aspx" method="post">
            <h2>Import</h2>
            <div class="row">
                <div class="six columns">
                    <label for="UploadTarget">Id of import destination page</label>
                    <input id="UploadTarget" name="UploadTarget" type="text" class="u-full-width"/>
                </div>
                
                <div class="six columns">
                    <label for="ImportPackage">Absolute file path of import package</label>
                <% if( GetPackages().Any() ) { %>
                    <select id="ImportPackage" class="u-full-width" name="ImportPackage">
                        <option value="none">-- Select package --</option>
                    <% foreach (var package in GetPackages()) { %>
                        <option value="<%=package.FullPath %>"><%=package.Name %></option>
                    <% } %>
                    </select>
                <% } else { %>
                    <input id="ImportPackage" name="ImportPackage" type="text" class="u-full-width"/>
                <% } %>
                </div>
            </div>

            <div class="row">
                <input type="submit" class="button-primary" name="Import" value="Import" />
            </div>

            <h2>Migrate</h2>
            <div class="row">
                <div class="six columns">
                    <label for="StartPageId">Id of imported root page</label>
                    <input id="StartPageId" class="u-full-width" name="StartPageId" type="text" />
                </div>
                <div class="six columns">
                    <label for="Mapper">Page mapping</label>
                    <select id="Mapper" class="u-full-width" name="Mapper">
                        <option value="none">-- Select mappning --</option>
                    <% foreach (var mapper in MapperRegistry.Mappers) { %>
                        <option value="<%=mapper.Name %>"><%=mapper.Name %></option>
                    <% } %>
                    </select>
                </div>
            </div>

            <div class="row">
                <input type="submit" class="button-primary" name="Run" value="Migrate" />
            </div>
            
            <h2>Clean up</h2>
            <div class="row">
                <div class="six columns">
                    <input type="submit" class="button-primary" name="delete-pagetypes" value="Delete page types" />
                </div>
                <div class="six columns">
                    <label>
                        <input checked type="checkbox" name="logging-only" value="logging-only">
                        <span class="label-body">Logging only</span>
                    </label>
                </div>
            </div>
            
            <%= DisplayLog() %>
        </form>
    </div>
</body>
</html>
