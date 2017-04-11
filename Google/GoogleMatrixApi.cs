using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;

using System.Text;
using System.IO;


namespace WpfApplication1.Google
{
    /// <summary>
    /// The Google Matrix API client.
    /// </summary>
    public class GoogleMatrixApiClient
    {
        //private const string requestUrlTemplate = "https://maps.googleapis.com/maps/api/distancematrix/json?origins={0}&destinations={1}&language=es&key={2}";
        private const string requestUrlTemplate = "https://maps.googleapis.com/maps/api/distancematrix/json?origins={0}&destinations={1}&mode=driving&language=es&key={2}";
        private string key;

        //"https://maps.googleapis.com/maps/api/distancematrix/xml?origins={0}&destinations={1}&mode=driving&key="
        /// <summary>
        /// Instantiates the <see cref="GoogleMatrixApiClient"/>.
        /// </summary>
        /// <param name="key"></param>
        public GoogleMatrixApiClient(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentOutOfRangeException("key", "Value can't be null or empty string.");

            // Trim the given key
            this.key = key.Trim();
        }



        /// <summary>
        /// Requests the Google Matrix APi for new data.
        /// </summary>
        /// <param name="origins">The origins points.</param>
        /// <param name="destinations">The destination points.</param>
        /// <returns>
        /// Returns collection of duration values (in seconds) between given points.
        /// First dimension contains origins in order as given.
        /// Second dimension contains duration value from origin to destination in order as given.
        /// </returns>
        /// 
        public IEnumerable<IEnumerable<int>>
            RequestMatrix(IEnumerable<string> origins, IEnumerable<string> destinations)
        {
            using (var client = new HttpClient())
            {
                // Construct the request URL
                var requestUrl = string.Format(requestUrlTemplate,
                    origins.Aggregate(string.Empty, (c, s) => c + (c == string.Empty ? string.Empty : "|") + s),
                    destinations.Aggregate(string.Empty, (c, s) => c + (c == string.Empty ? string.Empty : "|") + s),
                    this.key);

                // Send the request and deserialize the response
                //var response = await client.GetAsync(requestUrl);
                //var responseJson = await response.Content.ReadAsStringAsync();
                //var data = JsonConvert.DeserializeObject<GoogleResponse>(responseJson);

                try
                {
                  //  int alongroaddis = Convert.ToInt32(ConfigurationManager.AppSettings["alongroad"].ToString());
                  //  string keyString = ConfigurationManager.AppSettings["keyString"].ToString(); // passing API key
                  //  string clientID = ConfigurationManager.AppSettings["clientID"].ToString(); // passing client id

                  //  string urlRequest = "";

                    //string travelMode = "Driving"; //Driving, Walking, Bicycling, Transit.
                    //string urlRequest = @"http://maps.googleapis.com/maps/api/distancematrix/json?origins=" + "source" + "&destinations=" + "Destination" + "&mode='" + travelMode + "'&sensor=false";

                    //if (keyString.ToString() != "")
                    //{
                    //    urlRequest += "&client=" + clientID;
                    //    urlRequest = Sign(urlRequest, keyString); // request with api key and client id
                    //}

                    // WebRequest request = WebRequest.Create(urlRequest);
                    WebRequest request = WebRequest.Create(requestUrl);
                    request.Method = "POST";
                    string postData = "This is a test that posts this string to a Web server.";
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = byteArray.Length;

                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();

                    WebResponse Wresponse = request.GetResponse();
                    dataStream = Wresponse.GetResponseStream();

                    StreamReader reader = new StreamReader(dataStream);
                    string resp = reader.ReadToEnd();
                    var data2 = JsonConvert.DeserializeObject<GoogleResponse>(resp);
                    
                    reader.Close();
                    dataStream.Close();
                    Wresponse.Close();
                    
                    return data2.rows.Select(r => r.elements.Select(e => e.duration.value)); ;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                // Return only duration values
                //return data.rows.Select(r => r.elements.Select(e => e.duration.value));
            }
        }
       

        private class GoogleResponse
        {
            public string[] destination_addresses { get; set; }

            public string[] origin_addresses { get; set; }

            public GoogleResponseRow[] rows { get; set; }
        }
        
        private class GoogleResponseRow
        {
            public GoogleResponseElement[] elements { get; set; }
        }

        private class GoogleResponseElement
        {
            public GoogleResponseElementDistance distance { get; set; }

            public GoogleResponseElementDuration duration { get; set; }

            public string status { get; set; }
        }

        private class GoogleResponseElementDistance
        {
            public string text { get; set; }

            public int value { get; set; }
        }

        private class GoogleResponseElementDuration
        {
            public string text { get; set; }

            public int value { get; set; }
        }
    }
}
