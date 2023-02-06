using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Text.Json;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using System.IO;

namespace AirtableDownload.Core
{
    public interface IAirtableServices
    {
        Task<string> DownloadTableToFile(string path, string apitoken, string baseId, string tableName, string view = null);
        Task<string> DownloadTableJson(string apitoken, string baseId, string tableName, string view = null);
        Task<T[]> LoadTable<T>(string apitoken, string baseId, string tableName, string view = null);
    }

    public class AirtableServices : IAirtableServices
    {
        private const string AIRTABLE_API_URL = "https://api.airtable.com/v0/";

        private readonly HttpClient _client;
        public AirtableServices(HttpClient client)
        {
            this._client = client;
        }

        /// <summary>
        /// テーブルデータのJSONテキストをファイルに書き出す
        /// </summary>
        /// <param name="path">ファイルのフルパス</param>
        /// <param name="apitoken">Personal access tokenを表す文字列</param>
        /// <param name="baseId">BaseID</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="view">ビュー名</param>
        /// <returns>テーブルデータのJSONテキスト</returns>
        public async Task<string> DownloadTableToFile(string path, string apitoken, string baseId, string tableName, string view = null)
        {
            var tablestr = await DownloadTableJson(apitoken, baseId, tableName, view);
            File.WriteAllText(path, tablestr);

            return tablestr;
        }

        /// <summary>テーブルデータのJSONテキストを取得する</summary>
        /// <param name="apitoken">Personal access tokenを表す文字列</param>
        /// <param name="baseId">BaseID</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="view">ビュー名</param>
        /// <returns>
        ///   テーブルデータのJSONテキスト
        /// </returns>
        public async Task<string> DownloadTableJson(string apitoken, string baseId, string tableName, string view)
        {
            var result = await LoadTable<JsonElement>(apitoken, baseId, tableName, view);
            return string.Join("\n", result);
        }

        /// <summary>テーブルデータを取得する</summary>
        /// <typeparam name="T">マッピングクラス</typeparam>
        /// <param name="apitoken">Personal access tokenを表す文字列</param>
        /// <param name="baseId">BaseID</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="view">ビュー名</param>
        /// <returns>
        ///   <br />
        /// </returns>
        /// <exception cref="AirtableDownload.Core.AirtableBadRequestException"></exception>
        /// <exception cref="AirtableDownload.Core.AirtableForbiddenException"></exception>
        /// <exception cref="AirtableDownload.Core.AirtableNotFoundException"></exception>
        /// <exception cref="AirtableDownload.Core.AirtablePaymentRequiredException"></exception>
        /// <exception cref="AirtableDownload.Core.AirtableUnauthorizedException"></exception>
        /// <exception cref="AirtableDownload.Core.AirtableRequestEntityTooLargeException"></exception>
        /// <exception cref="AirtableDownload.Core.AirtableInvalidRequestException"></exception>
        /// <exception cref="AirtableDownload.Core.AirtableTooManyRequestsException"></exception>
        /// <exception cref="AirtableDownload.Core.AirtableUnrecognizedException"></exception>
        public async Task<T[]> LoadTable<T>(string apitoken, string baseId, string tableName, string view = null)
        {
            var result = new List<T>();
            string offset = null;
            do
            {
                var url = BuildURL(baseId, tableName, offset, view);
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apitoken);

                var response = await _client.SendAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            throw new AirtableBadRequestException();
                        case HttpStatusCode.Forbidden:
                            throw new AirtableForbiddenException();
                        case HttpStatusCode.NotFound:
                            throw new AirtableNotFoundException();
                        case HttpStatusCode.PaymentRequired:
                            throw new AirtablePaymentRequiredException();
                        case HttpStatusCode.Unauthorized:
                            throw new AirtableUnauthorizedException();
                        case HttpStatusCode.RequestEntityTooLarge:
                            throw new AirtableRequestEntityTooLargeException();
                        case (HttpStatusCode)422:
                            var error = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsByteArrayAsync());
                            throw new AirtableInvalidRequestException(error.ToString());
                        case (HttpStatusCode)429:
                            throw new AirtableTooManyRequestsException();
                        default:
                            throw new AirtableUnrecognizedException(response.StatusCode);
                    }
                }

                var rawdata = await response.Content.ReadAsByteArrayAsync();
                Debug.WriteLine(Encoding.UTF8.GetString(rawdata));
                var jsonBody = JsonSerializer.Deserialize<JsonBody<T>>(rawdata);
                offset = jsonBody.Offset;

                result.AddRange(jsonBody.Records.Select(x => x.Fields));
            } while (!string.IsNullOrWhiteSpace(offset));

            return result.ToArray();
        }

        /// <summary>Airtableレコード取得APIのURLを組み立てる</summary>
        /// <param name="tableName">テーブル名</param>
        /// <param name="offset">オフセット文字列</param>
        /// <param name="view">ビュー名</param>
        /// <returns>
        ///   <br />
        /// </returns>
        private string BuildURL(string baseId, string tableName, string offset, string view)
        {
            //var url = $"https://api.airtable.com/v0/{baseId}/{tableName}?api_key={client.ApiKey}&offset={offset}";

            var uriBuilder = new UriBuilder(AIRTABLE_API_URL + baseId + "/" + Uri.EscapeDataString(tableName));


            if (!string.IsNullOrEmpty(offset))
            {
                AddParametersToQuery(ref uriBuilder, $"offset={HttpUtility.UrlEncode(offset)}");
            }

            if (!string.IsNullOrEmpty(view))
            {
                AddParametersToQuery(ref uriBuilder, $"view={HttpUtility.UrlEncode(view)}");
            }

            return uriBuilder.ToString();
        }

        /// <summary>クエリ文字列を作成</summary>
        /// <param name="uriBuilder">uriBuilder</param>
        /// <param name="queryToAppend">クエリ文字列</param>
        private void AddParametersToQuery(ref UriBuilder uriBuilder, string queryToAppend)
        {
            if (uriBuilder.Query != null && uriBuilder.Query.Length > 1)
            {
                uriBuilder.Query = uriBuilder.Query.Substring(1) + "&" + queryToAppend;
            }
            else
            {
                uriBuilder.Query = queryToAppend;
            }
        }
    }

    public class JsonBody<T>
    {
        [JsonPropertyName("records")]
        [JsonInclude]
        public Record<T>[] Records;

        [JsonPropertyName("offset")]
        [JsonInclude]
        public string Offset;
    }

    public class Record<T>
    {
        [JsonPropertyName("id")]
        [JsonInclude]
        public string ID;

        [JsonPropertyName("fields")]
        [JsonInclude]
        public T Fields;

        [JsonPropertyName("createdTime")]
        [JsonInclude]
        public string createdTime;
    }
}
