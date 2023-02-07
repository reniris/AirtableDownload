# AirtableDownload
Airtableのテーブルデータを読むライブラリ

## Usage
```csharp
    readonly string appKey = YOUR_PERSONAL_ACCESS_TOKEN;
    readonly string baseId = TARGET_BASE_ID;
    
    string table = TARGET_TABLE;
    string view = TARGET_VIEW;          //Optional
    string path = TARGET_FILE_FULLPATH;

    //Init HttpClientFactory
    var services = new ServiceCollection();
    services.AddHttpClient<IAirtableServices, AirtableServices>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));
    var provider = services.BuildServiceProvider();
    var api_service = provider.GetRequiredService<IAirtableServices>();

    //Execute API
    var tablestr = await api_service.DownloadTableToFile(path,appKey,baseId,table,view);        
```

## License
* [The MIT License (MIT)](LICENSE.txt)

## Acknowledgments

* [Airtable.Net](https://github.com/ngocnicholas/airtable.net)
* [AirtableClient](https://github.com/yKimisaki/AirtableClient)

* [IHttpClientFactory を使って今はこれ一択と思った話](https://qiita.com/SY81517/items/5253e8f363f7275b3588)
