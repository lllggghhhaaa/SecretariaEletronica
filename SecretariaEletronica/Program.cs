namespace SecretariaEletronica
{
    public class Program
    {
        public static void Main() => new Startup().RunBotAsync().GetAwaiter().GetResult();
    }
}