public byte[] ConvertTTSAmazonPolly(string text)
{
    const int AMAZON_POLLY_MAX_REQUEST_SIZE = 1500;

    int chunks = (text.Length - 1) / AMAZON_POLLY_MAX_REQUEST_SIZE + 1;

    string[] chunk = new string[chunks];
    byte[][] soundChunk = new byte[chunks][];
    int totalTextLength = 0;
    int totalLength = 0;

    for (int i = 0; i < chunks; i++)
    {
        if (text.Length - totalTextLength < AMAZON_POLLY_MAX_REQUEST_SIZE)
        {
            chunk[i] = text.Substring(i * AMAZON_POLLY_MAX_REQUEST_SIZE, text.Length - totalTextLength);
        }
        else
        {
            chunk[i] = text.Substring(i * AMAZON_POLLY_MAX_REQUEST_SIZE, AMAZON_POLLY_MAX_REQUEST_SIZE);
        }

        totalTextLength = totalTextLength + chunk[i].Length;

        AmazonPollyClient client = new AmazonPollyClient(_awsAccessKeyID, _awsSecretAccessKey, Amazon.RegionEndpoint.USEast1); // https://blogs.msdn.microsoft.com/ansonh/2006/09/27/extern-alias-walkthrough/
        SynthesizeSpeechRequest request = new SynthesizeSpeechRequest();
        request.OutputFormat = OutputFormat.Mp3;
        request.Text = chunk[i];
        request.TextType = TextType.Text;
        request.VoiceId = VoiceId.Joanna;
        //request.SampleRate = "22050"; // Default
        SynthesizeSpeechResponse response = client.SynthesizeSpeech(request);

        using (Stream ms = new MemoryStream())
        {
            using (Stream stream = response.AudioStream)
            {
                byte[] buffer = new byte[32768];
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
            }
            soundChunk[i] = ms.ToByteArray();
            totalLength = totalLength + soundChunk[i].Length;
        }
    }
    byte[] audio = Combine(soundChunk); // https://stackoverflow.com/a/415396
    return audio;
}