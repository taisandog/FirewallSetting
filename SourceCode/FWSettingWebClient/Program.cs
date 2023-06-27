namespace FWSettingWebClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            string url = GetUrl(args);
            if (!string.IsNullOrWhiteSpace(url))
            {
                builder.WebHost.UseUrls(url);
            }
            else
            {
                builder.WebHost.UseUrls("http://0.0.0.0:8788");
            }

            // Add services to the container.
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }

        static string GetUrl(string[] args)
        {
            string url = "";

            bool findUrl = false;
            foreach (string arg in args)
            {
                if (string.IsNullOrWhiteSpace(arg))
                {
                    continue;
                }
                if (arg.StartsWith("--urls"))
                {
                    findUrl = true;
                    continue;
                }
                if (findUrl)
                {
                    url = arg;
                    break;
                }
            }
            return url;
        }
    }
}