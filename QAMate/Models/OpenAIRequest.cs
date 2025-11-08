namespace QAMate.Models
{
    public class OpenAIRequest
    {
        public string input { get; set; }
        public int MaxTokens { get; set; } = 100;
        public double Temperature { get; set; } = 0.7;
        //   public string Model { get; set; } = "ext-davinci-003";
      //  public string Model { get; set; } = "GPT-4o";

    }
}
