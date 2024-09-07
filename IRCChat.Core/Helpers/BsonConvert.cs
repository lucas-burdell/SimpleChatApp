using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace SimpleChat.Core.Helpers
{
    public static class BsonConvert
    {
        public static T Deserialize<T>(byte[] data) where T : class
        {
            var stream = new MemoryStream(data);
            using (var reader = new BsonDataReader(stream))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                var result = jsonSerializer.Deserialize<T>(reader);
                if (result == null)
                {
                    throw new Exception($"Could not deserialize data to type {typeof(T).Name}");
                }
                return result;
            }
        }

        public static byte[] Serialize<T>(T value) where T : class
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BsonDataWriter writer = new BsonDataWriter(stream))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, value);
                    return stream.ToArray();
                }
            }
        }
    }
}
