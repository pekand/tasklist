using System.IO;
using System.Xml.Serialization;

namespace TaskList
{
    public static class SerializationManager
    {
        //*******// Serialization

        public static T Deserialize<T>(this string toDeserialize)
        {
            Log.write("TaskManager Deserialize");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringReader textReader = new StringReader(toDeserialize);
            return (T)xmlSerializer.Deserialize(textReader);
        }

        public static string Serialize<T>(this T toSerialize)
        {
            Log.write("TaskManager Serialize");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringWriter textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }
    }
}
