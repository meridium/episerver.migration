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
                <input type="submit" class="button-primary" name="Import" id="import" value="Import" />
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
                <input type="submit" class="button-primary" name="Run" value="Migrate" id="migrate" />
            </div>
            
            <h2>Clean up</h2>
            <div class="row">
                <div class="six columns">
                    <input type="submit" class="button-primary" name="delete-pagetypes" value="Delete page types" id="cleanup"/>
                </div>
                <div class="six columns">
                    <label>
                        <input checked type="checkbox" name="logging-only" value="logging-only">
                        <span class="label-body">Logging only</span>
                    </label>
                </div>
            </div>
            
            <div id="log" class="row log-output"></div>
        </form>
    </div>
    <script>
        (function () {
            var post = function(url, formdata) {
                var xhr = new XMLHttpRequest(),
                    log = document.getElementById('log'),
                    counter = 0,
                    updateLog = function() {
                        console.log('response received #'+(++counter), xhr.responseText);
                        log.innerHTML = '<pre id="messages">' + xhr.responseText + '</pre>';
                        document.getElementById('messages').scrollIntoView(false);
                    };

                xhr.onreadystatechange = updateLog;
                xhr.open('POST', url);
                xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
                xhr.send(formdata);
            };

            var readFormValues = function(fields) {
                var formValues = [],
                    currentFieldValue;
                for (var i = 0; i < fields.length; i++) {
                    currentFieldValue = getEncodedFieldValue(fields[i]);
                    formValues.push(fields[i] + '=' + currentFieldValue);
                }
                return formValues.join('&');
            };

            var getEncodedFieldValue = function(field) {
                var value = document.getElementsByName(field)[0].value;
                return encodeURIComponent(value);
            };

            var onclick = function(buttonid, handler) {
                var button = document.getElementById(buttonid);
                button.addEventListener('click', handler);
            };

            var clickHandler = function(fields) {
                return function(e) {
                    e.preventDefault();
                    var formdata = readFormValues(fields);
                    console.log('posting: ' + formdata, e);
                    post('/migration/migrate.aspx', formdata);
                };
            };

            onclick('import',  clickHandler(['UploadTarget', 'ImportPackage', 'Import']));
            onclick('migrate', clickHandler(['StartPageId',  'Mapper', 'Run']));
            onclick('cleanup', clickHandler(['delete-pagetypes',  'logging-only']));

        }());
    </script>
</body>
</html>
