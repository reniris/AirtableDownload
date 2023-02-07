# AirtableDownload
Airtableのテーブルデータを読むライブラリ

## Installation
Install NuGet package(s).
```
PM> NuGet\Install-Package AirtableDownload
```
## Prerequisites
APIを叩くには、アカウント自身のPersonal Access Tokenと baseidが必要です。下記からコピーしてください。
1. Personal Access Tokenを作成  
  [GUIDE Personal access tokens](https://airtable.com/developers/web/guides/personal-access-tokens)をよく読んで操作したいbaseに対応するアクセストークンを作成する。Scopesはこのライブラリを使うだけならdata.records:readだけでよい。作成時にしかトークンをコピーできないので気を付けよう。
1. baseIDをコピー  
  [API Reference](https://airtable.com/developers/web/api/introduction)から操作したいbaseを選んでIDをコピーする。最後の色違いピリオドは無関係なので気を付けよう。

## Usage
AirtableDownload is available for download and installation as [NuGet packages.](https://www.nuget.org/packages/AirtableDownload/)
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
    //指定のテーブルの特定のビューのJSONテキストをファイルに書き込み
    var result1 = await api_service.DownloadTableToFile(path,appKey,baseId,table,view);
    //指定のテーブルの特定のビューのJSONテキストを取得
    var result2 = await api_service.DownloadTableJson(appKey,baseId,table,view);
    //指定のテーブルの特定のビューをDataClassにマッピング
    var result3 = await api_service.LoadTable<DataClass>(appKey,baseId,table,view);       
```

## License
* [The MIT License (MIT)](LICENSE.txt)

## Acknowledgments

* [Airtable.Net](https://github.com/ngocnicholas/airtable.net)
* [AirtableClient](https://github.com/yKimisaki/AirtableClient)

* [IHttpClientFactory を使って今はこれ一択と思った話](https://qiita.com/SY81517/items/5253e8f363f7275b3588)
