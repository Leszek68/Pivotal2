using RestSharp;
using System.Text.Json.Nodes;

namespace Pivotal2
{
    public class Cities
    {
        string domains = "https://wft-geo-db.p.rapidapi.com/";
        string endPoint = "v1/geo/cities";
        string[] apiHost = { "X-RapidAPI-Host", "wft-geo-db.p.rapidapi.com" };
        string[] apiKey = { "X-RapidAPI-Key", "f0bd87afe4msh06ba220f660631dp18a49ejsne3ec020eb064" };
        int sleep = 2500;
        bool error = false;
        List<string> citiesIdList = new List<string>();
        List<JsonObject> valueList = new List<JsonObject>();

        private void ByOffset(string _city, int _maxCount)
        {
            int offset = 0;
            int limit = Math.Min(10,_maxCount);   //restriction free is limit 10

            while (offset < _maxCount)
            {
                RestClient client = new RestClient(domains);
                var request = new RestRequest(endPoint + "?namePrefix=" + _city + "&offset=" + offset.ToString() + "&limit=" + limit.ToString());
                request.AddHeader(apiHost[0], apiHost[1]);
                request.AddHeader(apiKey[0], apiKey[1]);
                var response = client.ExecuteGet(request);
                if (response.IsSuccessful)
                {
                    JsonObject obj = JsonNode.Parse(response.Content).AsObject();
                    JsonArray jsonArray = (JsonArray)obj["data"];
                    foreach (JsonObject item in jsonArray)
                    {
                        var cityId = item["id"].GetValue<int>().ToString();
                        citiesIdList.Add(cityId);
                    }
                    offset += limit;
                    if (_maxCount - offset < limit) limit = _maxCount - offset;
                }
                else
                {
                    citiesIdList.Clear();
                    error = true;
                    break;
                }
                Thread.Sleep(sleep);   //It doesn't work without
            }
            return;
        }

        private void ByCity(string _cityId)
        {
            RestClient client = new RestClient(domains);
            var request = new RestRequest(endPoint + "/" + _cityId);
            request.AddHeader(apiHost[0], apiHost[1]);
            request.AddHeader(apiKey[0], apiKey[1]);
            var response = client.ExecuteGet(request);
            if (response.IsSuccessful)
            {
                JsonObject obj = JsonNode.Parse(response.Content).AsObject();
                JsonObject value = (JsonObject)obj["data"];
                //var timeZone = value["timezone"].GetValue<string>().ToString();
                valueList.Add(value);
            }
            else
            {
                valueList.Clear();
                error = true;
            }
            return;
        }

        public JsonObject GetValue(string _city, int _maxCount)
        {
            if (_city.Length > 0 && _maxCount > 0) 
            {
                ByOffset(_city, _maxCount);
                if (citiesIdList.Count > 0)
                {
                    foreach (var cityId in citiesIdList)
                    {
                        ByCity(cityId);
                        if (valueList.Count > 0)
                        {
                            Thread.Sleep(sleep);   //It doesn't work without
                        }
                        else
                        {
                            error = true;
                            break;
                        }
                    }
                }
            }

            var returnValue = new JsonObject();
            if (error == false)
            {
                JsonArray jsonArray = new JsonArray();
                foreach (var value in valueList)
                {
                    var copy = JsonNode.Parse(value.ToJsonString());
                    jsonArray.Add(copy);
                }
                returnValue.Add("Data", jsonArray);
                returnValue.Add("RecordCount", valueList.Count);
                returnValue.Add("Timestamp", DateTime.Now);
            }

            return returnValue;
        }
    }
}
