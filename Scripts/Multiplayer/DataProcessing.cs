using Godot;
using System;
using System.Text;
using System.Text.RegularExpressions;

public class DataProcessing
{
    //public string ByteToStringWithFix(byte[] receivedBytes, int bytesRead)
    //{
    //    string receivedData = Encoding.ASCII.GetString(receivedBytes, 0, bytesRead);
    //    receivedData = FixPacket(receivedData);
    //    return receivedData;
    //}
    string FixPacket(string receivedData)
    {
        //string pattern = @"#(.*?)#";
        //string pattern = @"#(.*)#";

        // Use Regex.Match to find the first match
        Match match = Regex.Match(receivedData, @"#(.*)#");

        // Check if a match is found
        if (match.Success)
        {
            // Extract the value between the '#' characters
            string extractedValue = match.Groups[1].Value;

            int.TryParse(extractedValue, out int lengthOfJson);

            int firstHashIndex = receivedData.IndexOf('#');
            int secondHashIndex = receivedData.IndexOf('#', firstHashIndex + 1) + 1;

            int startIndex = secondHashIndex;
            int endIndex = lengthOfJson;

            string jsonData = receivedData.Substring(startIndex, endIndex);

            return jsonData;
        }
        else
        {
            throw new InvalidOperationException("Failed to process received packet");
        }
    }
}

