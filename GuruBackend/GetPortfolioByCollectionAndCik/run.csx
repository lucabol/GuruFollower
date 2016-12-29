#r "..\sharedBin\GuruLoader.dll"

using System;
using System.Net;

// Gets portfolio given collection and cik
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string collection, string cik, DisplayPortfolio port, TraceWriter log)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}, {collection}, {cik}");
    if (String.IsNullOrEmpty(collection)) throw new ArgumentException("Empty collection");
    if (String.IsNullOrEmpty(cik)) throw new ArgumentException("Empty cik");


    return port == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, $"Either no collection={collection} or no cik={cik} in such collection")
        : req.CreateResponse(HttpStatusCode.OK, port);
}

// If needed strongly typed, to change something before sending it back, but paying the price of serialization and deserialization
//#r "Newtonsoft.Json"
//using Newtonsoft.Json;
//public static async Task<HttpResponseMessage> RunStronglyTyped(HttpRequestMessage req, string collection, string cik, DisplayPortfolio port, TraceWriter log) {
//    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}, {collection}, {cik}");
//    if (String.IsNullOrEmpty(collection)) throw new Exception("Empty collection");
//    if (String.IsNullOrEmpty(cik)) throw new Exception("Empty cik");


//    return port == null //String.IsNullOrEmpty(port)
//        ? req.CreateResponse(HttpStatusCode.BadRequest, $"Either no collection={collection} or no cik={cik} in such collection")
//        : req.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(port));
//}