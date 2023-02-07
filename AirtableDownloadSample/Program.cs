// See https://aka.ms/new-console-template for more information
using AirtableDownload;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System.Diagnostics;

internal class Program
{
    public const string AIRTABLEKEY_APITOKEN = "AirtableKey:APIToken";
    public const string AIRTABLEKEY_BASEID = "AirtableKey:BaseID";

    public const string APIPARAMS_SECTION = "APIParams";
    public const string APIPARAMS_TABLE = "Table";
    public const string APIPARAMS_VIEW = "View";

    private static async Task Main(string[] args)
    {
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        Debug.AutoFlush = true;

        Console.WriteLine("Hello, AirtableDownload!");

        string basedir = Path.GetDirectoryName(Environment.ProcessPath)!;
        //appsettings.jsonとsecret.jsonを使えるようにする
        var builder = new ConfigurationBuilder()
                        .SetBasePath(basedir)
                        .AddJsonFile(path: "appsettings.json")
                        .AddUserSecrets<Program>(optional: true);
        var config = builder.Build();

        //HttpClientを使いたいサービスを登録する
        var services = new ServiceCollection();
        services.AddHttpClient<IAirtableServices, AirtableServices>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        //サービスのインスタンスを取得
        var api_service = services.BuildServiceProvider().GetRequiredService<IAirtableServices>();

        //configからテーブル名とビュー名を抽出
        var param_tv = config.GetSection(APIPARAMS_SECTION).GetChildren()
            .Select(s => new { table = s[APIPARAMS_TABLE], view = s[APIPARAMS_VIEW] });

        foreach (var tv in param_tv)
        {
            //サービスからテーブルデータを落とすメソッドを実行
            var tablestr = await api_service.DownloadTableToFile(
                Path.Combine(basedir, $"{tv.table}.json"),
                config[AIRTABLEKEY_APITOKEN],
                config[AIRTABLEKEY_BASEID],
                tv.table,
                tv.view);

            Console.WriteLine($"Table : {tv.table} View : {tv.view}");
        }
    }
}