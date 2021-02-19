using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

/// <summary>
///
/// O Código deve retornar
///     Problema resolvido
///    
/// Não deve ser adicionado nenhum RETURN a mais
///
/// </summary>

class Program
{
    //Nao mexer aqui
    #region Nao mexer aqui
    static void Main(string[] args)
    {
        Console.WriteLine(Problema());
        Console.ReadLine();
    }
    #endregion
    //
    public static string Problema()
    {
        try
        {
            var ids = new List<string>() { "MLB832035381", "MLB938457671", "MLB691669454", "MLB837523349" };
            var dados = BuscarInformacoesML(ids);
            if (dados != null && dados.Count() == 4)
                return "Problema resolvido";
            else
                return "Problema com falha";
        }
        catch (Exception ex)
        {
            return "Problema com falha";
        }
    }

    /// <summary>
    ///
    ///     Usando RestSharp e Newtonsoft.Json, implemente um método que consulte a api do mercado livre e retorne em um array de "MlItem"
    ///     corrigindo qualquer erro que possa ocorrer
    ///    
    ///     ex https://api.mercadolibre.com/items/MLB832035381
    ///    
    ///     o metodo deverá retornar os seguintes items:
    ///    
    ///     MLB832035381
    ///     MLB938457671
    ///     MLB691669454
    ///     MLB837523349
    ///
    /// </summary>
    /// <returns></returns>

    private static IEnumerable<MlItem> BuscarInformacoesML(List<string> _ids)
    {
        try
        {
            // Lista de objetos MlItem.
            List<MlItem> _retorno = new List<MlItem>();
            // RestClient com o endpoint
            var client = new RestClient("https://api.mercadolibre.com");

            // Para cada ID faz uma consulta e adiciona o objeto resultado na lista.
            _ids.ForEach(_id =>
            {
                // Configura a requisição com ID e formato.
                var request = new RestRequest($"items/{_id}", Method.GET) { RequestFormat = DataFormat.Json };
                // Realiza a requisição.
                IRestResponse<List<MlItem>> queryResult = client.Execute<List<MlItem>>(request);
                if (queryResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Faz a requisição, manda deserializar e adiciona o resultado na lista de objetos MlItem.
                    //_retorno.Add(new JsonDeserializer().Deserialize<MlItem>(queryResult));
                    _retorno.Add(JsonConvert.DeserializeObject<MlItem>(queryResult.Content, new JsonConverter[]{ new UTCDateTimeConverter(), new IntConverter()}));
                }
            });
            return _retorno;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}

public class MlItem
{
    public string id { get; set; }
    public string title { get; set; }

    /// <summary>
    /// Custom Serializer para arredondar o preço e converter para int.
    /// Preferi manter o tipo de dado como int conforme me que me foi solicitado.
    /// O ideal seria mudar para flot para ter os centavos.
    /// </summary>
    [JsonProperty(ItemConverterType = typeof(IntConverter))]
    public int price { get; set; }

    /// <summary>
    /// Tratativa para quando vem null.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int official_store_id { get; set; }

    /// <summary>
    /// Não teve como deixar essa propriedade com o tipo int, pois a data em ticks precisa de valor maior que o máximo do int. Por isso colocquei como long.
    /// Poderia usar o tipo DateTime, até porque o dado vem como DateTime com TimeZone, porém como veio o projeto com essa propriedade como int, eu quis manter o mais original possível.
    /// </summary>
    [JsonProperty(ItemConverterType = typeof(UTCDateTimeConverter))]
    private long last_updated { get; set; }
}

public class UTCDateTimeConverter : Newtonsoft.Json.JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(long);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var _retorno = DateTime.Parse(reader.Value.ToString());
        return _retorno.Ticks;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
public class IntConverter : Newtonsoft.Json.JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(int);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        int retorno = 0;
        if (reader.Path.Equals("price"))
        {
            retorno = (int)Math.Round(Double.Parse(reader.Value.ToString()));

            return retorno;
        }
        if (reader.Value == null) return null;
        if (reader.TokenType == JsonToken.Null) return null;
        if (objectType != typeof(int)) return null;

        int.TryParse(reader.Value.ToString(), out retorno);
        return retorno;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}