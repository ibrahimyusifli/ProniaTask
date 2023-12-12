namespace ProniaAB202.Middlewares
{
    public class GlobalExceptionHandlerMiddlerware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlerMiddlerware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception e)
            {

                context.Response.Redirect($"/home/errorpage?error={e.Message}");
            }
        }
    }
}
